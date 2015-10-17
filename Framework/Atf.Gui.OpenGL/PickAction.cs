//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;

using Sce.Atf.Rendering.OpenGL;
using Sce.Atf.VectorMath;

using Tao.OpenGl;

namespace Sce.Atf.Rendering.Dom
{
    /// <summary>
    /// Standard implementation of IPickAction. Uses OpenGL rendering, if necessary, to support
    /// "render picking" of objects, unless they implement IIntersectable.</summary>
    public class PickAction : RenderAction, IPickAction
    {
        /// <summary>
        /// Constructor</summary>
        /// <param name="renderStateGuardian">Render state guardian</param>
        public PickAction(RenderStateGuardian renderStateGuardian)
            : base(renderStateGuardian)
        {
            m_selectionBuffer = new int[65536];
            m_openGlHits = 0;
            m_pickTolerance = 3.0f;
            m_viewFrust0 = new Frustum();
        }

        #region IPickAction Members

        /// <summary>
        /// Sets the filter that determines the single type of RenderObjects to dispatch in the pick
        /// operation. For multiple types, use the TypesFilter property. To allow all types, set to
        /// 'null'. Setting this also clears the internal HitRecord array.</summary>
        public Type TypeFilter
        {
            set
            {
                if (value == null)
                {
                    m_typesFilter = null;
                }
                else
                {
                    m_typesFilter = new List<Type>(1);
                    m_typesFilter.Add(value);
                }
                ClearHitList();
            }
        }

        /// <summary>
        /// Gets and sets the filter that determines the multiple types of RenderObjects to dispatch
        /// in the pick operation. To allow all types, set to 'null'.  Setting this also clears the
        /// internal HitRecord array.</summary>
        public ICollection<Type> TypesFilter
        {
            get { return m_typesFilter; }
            set
            {
                m_typesFilter = value;
                ClearHitList();
            }
        }

        /// <summary>
        /// Initializes the pick selection area in screen coordinates</summary>
        /// <param name="camera">The camera</param>
        /// <param name="x1">Upper left x windows coordinate</param>
        /// <param name="y1">Upper left y windows coordinate</param>
        /// <param name="x2">Bottom right x windows coordinate</param>
        /// <param name="y2">Bottom right y windows coordinate</param>
        /// <param name="multiPick">True to perform a multiple pick operation</param>
        /// <param name="usePickingFrustum">True to set the frustum according to the picking rectangle</param>
        public void Init(Camera camera, int x1, int y1, int x2, int y2, bool multiPick, bool usePickingFrustum)
        {
            m_multiPick = multiPick;
            if (x1 == x2 && y1 == y2)
                m_frustumPick = false;
            else
                m_frustumPick = true;
            m_x = x1;
            m_y = y1;

            ClearHitList();
            Gl.glSelectBuffer(65536, m_selectionBuffer);
            Gl.glRenderMode(Gl.GL_SELECT);

            Gl.glMatrixMode(Gl.GL_PROJECTION);
            Gl.glLoadIdentity();

            int[] viewPort = new int[4];
            // Setup viewport
            viewPort[0] = viewPort[1] = 0;
            viewPort[2] = m_width;
            viewPort[3] = m_height;

            //Calculate the center in OpenGL viewing coordinates -- +x to the right, +y up, origin "lower left"
            float xCenter = (x1 + x2) * 0.5f;
            float yCenter = m_height - (y1 + y2) * 0.5f;
            float width = Math.Abs(x1 - x2);
            float height = Math.Abs(y1 - y2);

            Glu.gluPickMatrix(
                xCenter,
                yCenter,
                width + 2.0f * m_pickTolerance,
                height + 2.0f * m_pickTolerance,
                viewPort);
            
            // Calc new view frustum according to mouse coords
            m_viewFrust0.Set(camera.Frustum);

            if (usePickingFrustum)
            {
                //Calculate clipping coordinates in normalized viewing coordinates --
                //(-0.5,-0.5) is the lower left corner and (0.5,0.5) is the upper right corner.
                float left = (xCenter - width * 0.5f) / m_width - 0.5f;
                float right = (xCenter + width * 0.5f) / m_width - 0.5f;
                float top = (yCenter + height * 0.5f) / m_height - 0.5f;
                float bottom = (yCenter - height * 0.5f) / m_height - 0.5f;

                m_viewFrust0.Clip(left, right, top, bottom);
            }

            Gl.glInitNames();
        }

        /// <summary>
        /// Gets the HitRecord array from the picking operation</summary>
        /// <returns>HitRecord array</returns>
        /// <remarks>Must be called after Init() and Dispatch()</remarks>
        public HitRecord[] GetHits()
        {
            return GetHits(m_traverseList, m_multiPick);
        }

        /// <summary>
        /// Gets the HitRecord array from a given traverse list</summary>
        /// <param name="multiPick">True to return all available HitRecords. False to return just the closest</param>
        /// <returns>HitRecord array</returns>
        /// <remarks>Must be called after Init() and Dispatch()</remarks>
        public HitRecord[] GetHits(bool multiPick)
        {
            return GetHits(m_traverseList, multiPick);
        }

        /// <summary>
        /// Gets the HitRecord array from a given traverse list</summary>
        /// <param name="traverseList">The traverse list</param>
        /// <returns>HitRecord array</returns>
        /// <remarks>Must be called after Init() and Dispatch()</remarks>
        public HitRecord[] GetHits(ICollection<TraverseNode> traverseList)
        {
            return GetHits(traverseList, m_multiPick);
        }

        /// <summary>
        /// Gets the HitRecord array from a given traverse list</summary>
        /// <param name="traverseList">The traverse list</param>
        /// <param name="multiPick">True to return all available HitRecords. False to return just the closest</param>
        /// <returns>HitRecord array</returns>
        /// <remarks>Must be called after Init() and Dispatch(). There can potentially be duplicate
        /// HitRecords returned for two reasons: 1) Proxy objects, like the cube by default, render
        /// themselves twice, once with solid fill and once with the outline. Both renderings can
        /// yield a HitRecord, sometimes with slightly different z values. 2) Models are made up of
        /// multiple TransformNodes and each one can generate a HitRecord.</remarks>
        public HitRecord[] GetHits(ICollection<TraverseNode> traverseList, bool multiPick)
        {
            if (m_frustumPick == true && multiPick == false)
                throw new InvalidOperationException("results can't be sorted front-to-back with frustum picking");

            // First get the hits from OpenGL pick. Gl.glRenderMode resets this value when it is called,
            // so consecutive calls to GetHits() will fail. Thus, we need to cache glRenderMode(GL_RENDER).
            if (m_openGlHits == 0)
            {
                m_openGlHits = Gl.glRenderMode(Gl.GL_RENDER);
            }
            HitRecord[] renderSelect = null;

            if (m_openGlHits > 0 && traverseList.Count > 0)
            {
                renderSelect = PopulateOpenGlSelection(traverseList);
            }
            else
            {
                renderSelect = s_emptyHitRecordArray;
            }

            // now integrate the hits from geometrical picking.
            HitRecord[] select;
            if (m_geoHitList.Count == 0)
            {
                select = renderSelect;
            }
            else
            {
                int total = renderSelect.Length + m_geoHitList.Count;
                select = new HitRecord[total];
                renderSelect.CopyTo(select, 0);
                m_geoHitList.CopyTo(select, renderSelect.Length);
            }

            // sort the results by distance from camera eye, near-to-far
            if (m_frustumPick == false)
                HitRecord.Sort(select, m_eye);

            // if the caller only wants one result, just give them one.
            if (multiPick == false && select.Length > 1)
            {
                HitRecord[] singleHit = new HitRecord[1];
                singleHit[0] = select[0];
                select = singleHit;
            }

            // useful debug output
            //Console.WriteLine("multiPick:{0}, m_frustumPick:{1}", multiPick, m_frustumPick);
            //foreach (HitRecord hit in select)
            //{
            //    Console.WriteLine("  hit: {0} at z {1}", hit.RenderObject.InternalObject.Id, hit.ZMin);
            //}
            
            // return the results
            return select;
        }

        /// <summary>
        /// Shoots a ray into the scene and returns the intersection point</summary>
        /// <param name="camera">The camera</param>
        /// <param name="x">Ray x coordinate in screen space</param>
        /// <param name="y">Ray y coordinate in screen space</param>
        /// <param name="scene">The given scene</param>
        /// <param name="point">The point of intersection</param>
        /// <returns><c>True</c> if the ray intersects the scene</returns>
        public bool Intersect(Camera camera, int x, int y, Scene scene, ref Vec3F point)
        {
            Vec3F surfaceNormal;
            return Intersect(camera, x, y, scene, ref point, out surfaceNormal);
        }

        /// <summary>
        /// Shoots a ray into the scene and returns the intersection point</summary>
        /// <param name="camera">The camera</param>
        /// <param name="x">Ray x coordinate in screen space</param>
        /// <param name="y">Ray y coordinate in screen space</param>
        /// <param name="scene">The given scene</param>
        /// <param name="point">The point of intersection</param>
        /// <param name="surfaceNormal">The surface normal of the target object at the intersection
        /// point, or the zero vector if the surface normal could not be found</param>
        /// <returns><c>True</c> if the ray intersects the scene</returns>
        public bool Intersect(Camera camera, int x, int y, Scene scene, ref Vec3F point,
            out Vec3F surfaceNormal)
        {
            //Too small of a pick tolerance makes Design View tooltips and right-click context
            //menus not work on wireframe / linework.
            //m_pickTolerance = 0.001f;
            Init(camera, x, y, x, y, false, true);
            Dispatch(scene, camera);
            HitRecord[] hits = GetHits();
            //m_pickTolerance = 3.0f;

            return Intersect(x, y, hits, ref point, out surfaceNormal);
        }

        /// <summary>
        /// Shoots a ray into the scene and returns the intersection point</summary>
        /// <param name="camera">The camera</param>
        /// <param name="x">Ray x coordinate in screen space</param>
        /// <param name="y">Ray y coordinate in screen space</param>
        /// <param name="scene">The given scene</param>
        /// <param name="traverseList">Traverse list to use when performing the intersection</param>
        /// <param name="point">The point of intersection</param>
        /// <returns><c>True</c> if the ray intersects the scene</returns>
        public bool Intersect(Camera camera, int x, int y, Scene scene, ICollection<TraverseNode> traverseList
            , ref Vec3F point)
        {
            m_pickTolerance = 0.001f;
            Init(camera, x, y, x, y, false, true);
            Dispatch(traverseList, scene, camera);
            HitRecord[] hits = GetHits(traverseList);
            m_pickTolerance = 3.0f;

            Vec3F surfaceNormal;
            return Intersect(x, y, hits, ref point, out surfaceNormal);
        }

        /// <summary>
        /// Shoots a ray into the scene and returns the intersection point</summary>
        /// <param name="camera">The camera</param>
        /// <param name="x">Ray X coordinate in screen space</param>
        /// <param name="y">Ray y coordinate in screen space</param>
        /// <param name="scene">The given scene</param>
        /// <param name="point">The point of intersection</param>
        /// <param name="firstHit">The HitRecord giving possible nearest vertex and surface normal</param>
        /// <returns><c>True</c> if the ray intersects the scene</returns>
        public bool Intersect(Camera camera, int x, int y, Scene scene, ref Vec3F point,
            out HitRecord firstHit)
        {
            firstHit = null;

            //Too small of a pick tolerance makes Design View tooltips and right-click context
            //menus not work on wireframe / linework.
            //m_pickTolerance = 0.001f;
            Init(camera, x, y, x, y, false, true);
            Dispatch(scene, camera);
            HitRecord[] hits = GetHits();
            //m_pickTolerance = 3.0f;

            if (hits.Length == 0)
                return false;

            firstHit = hits[0];
            if (firstHit.HasWorldIntersection)
            {
                point = firstHit.WorldIntersection;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Clears out references to all the objects that were used by Dispatch() and BuildTraverseList()
        /// to prevent large amounts of managed memory from being held on to unnecessarily</summary>
        public override void Clear()
        {
            ClearHitList();
            base.Clear();
        }
        #endregion

        /// <summary>
        /// Builds a traverse list from the Scene and dispatches it for rendering</summary>
        /// <param name="scene">The scene to dispatch</param>
        /// <param name="camera">The camera</param>
        public override void Dispatch(Scene scene, Camera camera)
        {
            //We no longer want render stats for picking because when testing for the manipulator,
            //  this is very fast and leads to an artificially high frame rate. Better to let the
            //  displayed frame rate mean what people expect -- the number of times the visible
            //  frame buffer is rendered each second.
            //Util3D.RenderStats.ResetFrame();
            PreDispatch(scene, camera);

            // Cache current view frustum
            Frustum frust = new Frustum();
            frust.Set(camera.Frustum);

            // Set pick frustum
            camera.Frustum.Set(m_viewFrust0);

            // Clear traverse list
            m_traverseList.Clear();
            m_traverseList.SetViewMatrix(camera.ViewMatrix);

            // Make sure that solid-coloring is default, so that objects that don't implement
            // IRenderPick can still be picked when wireframe mode is on.
            RenderState origState = scene.StateStack.Peek();
            RenderState pickState;
            if (origState != null)
            {
                scene.StateStack.Pop();
                pickState = new RenderState(origState);
            }
            else
            {
                pickState = new RenderState();
            }
            pickState.RenderMode |= RenderMode.Smooth | RenderMode.SolidColor;
            pickState.RenderMode &= ~RenderMode.Wireframe;
            pickState.OverrideChildState |= RenderMode.Wireframe;
            scene.StateStack.Push(pickState);

            //s_stopWatch.Reset();
            //s_stopWatch.Start();
            BuildTraverseList(camera, m_traverseList, scene);
            //Util3D.RenderStats.TraverseNodeCount = m_traverseList.Count;
            //Util3D.RenderStats.TimeForTraverse = s_stopWatch.ElapsedMilliseconds;

            // Restore original render state.
            scene.StateStack.Pop();
            if (origState != null)
            {
                scene.StateStack.Push(origState);
            }

            //Restore view frustum for pick rendering
            camera.Frustum.Set(frust);

            //s_stopWatch.Reset();
            //s_stopWatch.Start();
            DispatchTraverseList(m_traverseList, camera);
            //Util3D.RenderStats.TimeForDispatchTraverseList = s_stopWatch.ElapsedMilliseconds;

            PostDispatch(scene, camera);            
        }

        /// <summary>
        /// Dispatches the given traverse list for rendering</summary>
        /// <param name="traverseList">The traverse list</param>
        /// <param name="scene">The scene to dispatch</param>
        /// <param name="camera">The camera</param>
        public override void Dispatch(ICollection<TraverseNode> traverseList, Scene scene, Camera camera)
        {
            //Util3D.RenderStats.ResetFrame();
            PreDispatch(scene, camera);

            DispatchTraverseList(traverseList, camera);

            PostDispatch(scene, camera);            
        }

        /// <summary>
        /// Dispatches untyped items. Replaces DispatchNotTyped(). To get the same behavior as
        /// the old DispatchNotTyped(), set the TypeFilter property to null prior to calling.</summary>
        /// <param name="traverseList">The traverse list</param>
        /// <param name="camera">The camera</param>
        protected void DispatchTraverseList(ICollection<TraverseNode> traverseList, Camera camera)
        {
            // Prepare for geometric picking -- create the ray in world space and reset geometric hit-list.
            // First create the ray in viewing coordinates and transform to world coordinates.
            float nx = (m_x / (float)m_width) - 0.5f;//normalized x
            float ny = 0.5f - (m_y / (float)m_height);//normalized y
            Ray3F rayWorld = camera.CreateRay(nx, ny);
            Matrix4F worldToView = camera.ViewMatrix;
            Matrix4F viewToWorld = new Matrix4F();
            viewToWorld.Invert(worldToView);
            rayWorld.Transform(viewToWorld);
            ClearHitList();

            // for geometric picking. will be cleared for each HitRecord.
            List<uint> userData = new List<uint>(1);

            // Dispatch traverse list
            int index = 0;
            foreach (TraverseNode node in traverseList)
            {
                // First test for filtering. 
                IRenderObject renderObject = node.RenderObject;
                if (FilterByType(renderObject))
                {
                    IIntersectable intersectable = renderObject.GetIntersectable();
                    IGeometricPick geometricPick = intersectable as IGeometricPick;
                    if (geometricPick != null)
                    {
                        // Picking by geometry.
                        Matrix4F objToWorld = new Matrix4F(node.Transform);
                        Matrix4F worldToObj = new Matrix4F();
                        worldToObj.Invert(objToWorld);
                        Matrix4F viewToObj = Matrix4F.Multiply(viewToWorld, worldToObj);

                        if (m_frustumPick)
                        {
                            //The pick frustum is in view space. Transform to world space then object space.
                            Frustum frustumObj = new Frustum(m_viewFrust0);
                            frustumObj.Transform(viewToObj);

                            //Multi-pick. Get everything in the pick frustum (m_viewFrust0).
                            Vec3F eyeObj;
                            worldToObj.Transform(camera.Eye, out eyeObj);
                            userData.Clear();

                            if (geometricPick.IntersectFrustum(frustumObj, eyeObj, node.RenderState, userData))
                            {
                                // Prepare a multi-pick HitRecord, as if OpenGL had calculated this.
                                HitRecord hit = new HitRecord(
                                    node.GraphPath,
                                    renderObject,
                                    objToWorld,
                                    userData.ToArray());

                                m_geoHitList.Add(hit);
                            }
                        }
                        else
                        {   //Single pick. We care about distance from camera eye.
                            //Make a copy of the ray in world-space and tranform it to object space.
                            Ray3F rayObj = rayWorld; //remember, Ray3F is a value type, not a reference type.
                            rayObj.Transform(worldToObj);

                            // Do the intersection test in object space.
                            userData.Clear();
                            Vec3F intersectionPt, surfaceNormal;
                            Vec3F nearestVert;
                            bool intersected;
                            intersected = geometricPick.IntersectRay(
                                rayObj, camera, node.RenderState, objToWorld, this,
                                out intersectionPt, out nearestVert, out surfaceNormal, userData);

                            if (intersected)
                            {
                                // Transform to world space and then to screen space.
                                objToWorld.Transform(intersectionPt, out intersectionPt);
                                objToWorld.Transform(nearestVert, out nearestVert);
                                // Prepare a single-pick HitRecord, as if OpenGL had calculated this.
                                HitRecord hit = new HitRecord(
                                    node.GraphPath,
                                    renderObject,
                                    objToWorld,
                                    userData.ToArray());

                                // This is the one difference from OpenGL pick. We have the world pt already.
                                hit.WorldIntersection = intersectionPt;
                                hit.NearestVert = nearestVert;

                                // Another difference is that it's possible to get the surface normal.
                                if (surfaceNormal != Vec3F.ZeroVector)
                                {
                                    objToWorld.TransformNormal(surfaceNormal, out surfaceNormal);
                                    surfaceNormal.Normalize();
                                    hit.Normal = surfaceNormal;
                                }

                                m_geoHitList.Add(hit);
                            }
                        }
                    }
                    else
                    {
                        // Picking by "rendering", using OpenGL pick.
                        PushMatrix(node.Transform, false);
                        Gl.glPushName(index);
                        IRenderPick pickInterface = renderObject as IRenderPick;
                        if (pickInterface != null)
                        {
                            pickInterface.PickDispatch(node.GraphPath, node.RenderState, this, camera);
                        }
                        else
                        {
                            renderObject.Dispatch(node.GraphPath, node.RenderState, this, camera);
                        }
                        Gl.glPopName();
                        PopMatrix();
                    }
                }

                index++;
            }
        }

        /// <summary>
        /// Sets up projection given a Camera</summary>
        /// <param name="camera">Camera that determines projection</param>
        protected override void SetupProjection(Camera camera)
        {
            m_projectionMatrix = new Matrix4F(camera.ProjectionMatrix);
            m_eye = camera.WorldEye;

            // Calling the base method causes every render-picked object to be selected.
            //base.SetupProjection(camera);

            Gl.glViewport(0, 0, m_width, m_height);
            Gl.glMatrixMode(Gl.GL_PROJECTION);

            if (camera.Frustum.IsOrtho)
            {
                Gl.glOrtho(
                    camera.Frustum.Left,
                    camera.Frustum.Right,
                    camera.Frustum.Bottom,
                    camera.Frustum.Top,
                    camera.Frustum.Near,
                    camera.Frustum.Far);

            }
            else
            {
                Glu.gluPerspective(
                    camera.Frustum.FovY * 180 / Math.PI,
                    (double)m_width / (double)m_height,
                    camera.Frustum.Near,
                    camera.Frustum.Far);
            }
        }

        /// <summary>
        /// Sets up the view matrix given a Camera</summary>
        /// <param name="camera">Camera to get view matrix for</param>
        protected override void SetupView(Camera camera)
        {
            m_viewMatrix = new Matrix4F(camera.ViewMatrix);
            base.SetupView(camera);
        }

        /// <summary>
        /// Intersects the specified client point with the scene, knowing that the given
        /// HitRecords were the result of having called Dispatch with these same client points</summary>
        /// <param name="x">Client x coordinate</param>
        /// <param name="y">Client y coordinate</param>
        /// <param name="hits">The hits</param>
        /// <param name="pt">The intersection point</param>
        /// <param name="surfaceNormal">The surface normal of the target object at the intersection
        /// point, or the zero vector if the surface normal could not be found. Check against
        /// Vec3F.ZeroVector before using.</param>
        /// <returns><c>True</c> if client point intersects scene</returns>
        protected bool Intersect(int x, int y, HitRecord[] hits, ref Vec3F pt, out Vec3F surfaceNormal)
        {
            surfaceNormal = new Vec3F(); 
            
            if (hits.Length == 0)
                return false;

            HitRecord hit = hits[0];
            
            if (hit.HasNormal)
                surfaceNormal = hit.Normal;

            if (hit.HasWorldIntersection)
            {
                pt = hit.WorldIntersection;
                return true;
            }

            return false;
        }

        private Vec3F GetWorldIntersectionFromScreen(
            float screenX, float screenY, float screenZ,
            double[] viewMat, double[] projectionMat, int[] viewport)
        {
            double posX, posY, posZ;

            double winX = (double)screenX;
            double winY = (double)viewport[3] - (double)screenY;

            Glu.gluUnProject(
                winX,
                winY,
                screenZ,
                viewMat,
                projectionMat,
                viewport,
                out posX,
                out posY,
                out posZ);
            
            Vec3F intersectionPt = new Vec3F(
                (float)posX,
                (float)posY,
                (float)posZ);
            return intersectionPt;
        }

        private HitRecord[] PopulateOpenGlSelection(ICollection<TraverseNode> traverseList)
        {
            // Ensure that OpenGL is in correct state.
            double[] viewMat = null;
            double[] projectionMat = null;
            int[] viewport = null;
            if (m_frustumPick == false)
            {
                Gl.glViewport(0, 0, m_width, m_height);
                Gl.glMatrixMode(Gl.GL_PROJECTION);
                Gl.glLoadIdentity();
                Util3D.glMultMatrixf(m_projectionMatrix);

                Gl.glMatrixMode(Gl.GL_MODELVIEW);
                Gl.glLoadIdentity();
                Util3D.glMultMatrixf(m_viewMatrix);

                viewMat = new double[16];
                Gl.glGetDoublev(Gl.GL_MODELVIEW_MATRIX, viewMat);

                projectionMat = new double[16];
                Gl.glGetDoublev(Gl.GL_PROJECTION_MATRIX, projectionMat);

                viewport = new int[4];
                viewport[0] = viewport[1] = 0;
                viewport[2] = m_width;
                viewport[3] = m_height;
            }

            // Construct traverse array
            HitRecord[] selection = null;
            List<HitRecord> selectionList = new List<HitRecord>();
            TraverseNode[] travArray = new TraverseNode[traverseList.Count];
            traverseList.CopyTo(travArray, 0);

            uint start = 0;
            for (int i = 0; i < m_openGlHits; ++i)
            {
                uint nameCount = (uint)m_selectionBuffer[start];

                if (nameCount > 0)
                {
                    uint travNodeIndex = (uint)m_selectionBuffer[start + 3];
                    TraverseNode travNode = travArray[travNodeIndex];
                    HitRecord hitRecord;
                    if (m_frustumPick)
                    {
                        hitRecord = new HitRecord(
                            travNode.GraphPath,
                            travNode.RenderObject,
                            new Matrix4F(travNode.Transform),
                            new uint[nameCount - 1]);
                    }
                    else
                    {
                        hitRecord = new HitRecord(
                            travNode.GraphPath,
                            travNode.RenderObject,
                            new Matrix4F(travNode.Transform),
                            new uint[nameCount - 1]);

                        // Transform screen to world and record world-space intersection point.
                        float zMin = ((float)((uint)m_selectionBuffer[start + 1])) / 0xFFFFFFFF;
                        Vec3F intersectionPt = GetWorldIntersectionFromScreen(
                            m_x, m_y, zMin,
                            viewMat, projectionMat, viewport);
                        hitRecord.WorldIntersection = intersectionPt;
                    }

                    // Populate object data
                    for (uint j = 0; j < nameCount - 1; j++)
                    {
                        hitRecord.RenderObjectData[j] = (uint)m_selectionBuffer[start + 4 + j];
                    }

                    selectionList.Add(hitRecord);

                    start += 3 + nameCount;
                }
            }
            selection = new HitRecord[selectionList.Count];
            selectionList.CopyTo(selection, 0);
            return selection;
        }

        /// <summary>
        /// Checks the given RenderObject against the filter and returns true if it is pickable</summary>
        /// <param name="renderObject">RenderObject checked</param>
        /// <returns><c>True</c> if the RenderObject is pickable according to the filter</returns>
        private bool FilterByType(IRenderObject renderObject)
        {
            if (m_typesFilter == null)
                return true;

            foreach (Type t in m_typesFilter)
            {
                if (t.IsAssignableFrom(renderObject.GetType()))
                    return true;
            }
            
            return false;
        }

        private void ClearHitList()
        {
            m_openGlHits = 0;
            m_geoHitList.Clear();
        }

        //'m_selectionBuffer' will be filled in by a series of hit records by a call to glSelectBuffer.
        // The values should be treated as unsigned ints, but our OpenGL wrapper needs int[].
        //From OpenGL documenation:
        //"The hit record consists of the number of names in the name stack at the time of the event,
        // followed by the minimum and maximum depth values of all vertices that hit since the previous
        // event, followed by the name stack contents, bottom name first."
        private readonly int[] m_selectionBuffer;

        //m_openGlHits must be set to null whenever we re-render. The problem is that the OpenGl
        //call to glRenderMode(GL_RENDER) resets the # of render hits, so we can't call it twice
        //in between renderings. Set m_openGlHits to 0 whenever m_geoHitList is cleared.
        private int m_openGlHits;
        private readonly List<HitRecord> m_geoHitList = new List<HitRecord>();
        private bool m_frustumPick; //is a frustum used rather than a ray? No surface points or surface normals.
        private bool m_multiPick; //does the caller want multiple results?
        private ICollection<Type> m_typesFilter;
        private float m_pickTolerance;
        private readonly Frustum m_viewFrust0;
        private float m_x, m_y; //screen coordinates of selection pick
        private Matrix4F m_viewMatrix, m_projectionMatrix; //cached from SetupProjection and SetupView
        private Vec3F m_eye; //cached from SetupProjection

        private static readonly HitRecord[] s_emptyHitRecordArray = new HitRecord[0];
    }
}
