//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;

using Sce.Atf.Rendering.OpenGL;
using Sce.Atf.VectorMath;

//using Tao.OpenGl;
using OTK = OpenTK.Graphics;

namespace Sce.Atf.Rendering.Dom
{
    /// <summary>
    /// Translator elements that can be picked</summary>
    public enum HitElement
    {
        /// <summary>
        /// X arrow</summary>
        X_ARROW = 1,

        /// <summary>
        /// Y arrow</summary>
        Y_ARROW = 2,

        /// <summary>
        /// Z arrow</summary>
        Z_ARROW = 3,

        /// <summary>
        /// Square in the X-Y plane</summary>
        XY_SQUARE = 4,

        /// <summary>
        /// Square in the Y-Z plane</summary>
        YZ_SQUARE = 5,

        /// <summary>
        /// Square in the X-Z plane</summary>
        XZ_SQUARE = 6,
    }

    /// <summary>
    /// Reusable 3D translator control</summary>
    public class TranslatorControl
    {
        /// <summary>
        /// Constructor</summary>
        /// <param name="name">Control name</param>
        public TranslatorControl(int name)
        {
            m_nameBase = name;
        }

        /// <summary>
        /// Gets and sets the translator size ratio</summary>
        public float TranslatorSizeRatio
        {
            get { return m_axisRatio; }
            set { m_axisRatio = value; }
        }

        /// <summary>
        /// Traverses the control for rendering</summary>
        /// <param name="graphPath">The path leading to the control</param>
        /// <param name="action">Render action</param>
        /// <param name="list">Traverse list</param>
        public void Traverse(
            Stack<SceneNode> graphPath, 
            IRenderAction action, 
            ICollection<TraverseNode> list)
        {
            TraverseNode node = action.GetUnusedNode();
            node.Init(action.RenderObject, action.TopMatrix, graphPath, s_renderState);
            list.Add(node);
        }
        
        /// <summary>
        /// Renders the control</summary>
        /// <param name="action">Render action</param>
        /// <param name="camera">Camera</param>
        /// <param name="state">Render state</param>
        /// <param name="transform">Transform</param>
        public void Render(IRenderAction action, Camera camera, RenderState state, Matrix4F transform)
        {
            float s1, s2, s3;

            // apply xform
            OTK.OpenGL.GL.PushMatrix();
            Util3D.glMultMatrixf(transform);

            CalcAxisLengths(camera, transform, out s1, out s2, out s3);

            bool drawX, drawY, drawZ;
            DecideArrowDrawing(transform, camera, out drawX, out drawY, out drawZ);

            if (drawX)
            {
                RenderXArrow(s1);
                RenderXAxis(s1);
            }
            if (drawY)
            {
                RenderYArrow(s2);
                RenderYAxis(s2);
            }
            if (drawZ)
            {
                RenderZArrow(s3);
                RenderZAxis(s3);
            }

            RenderXYSquare(s1 * SquareLength, s2 * SquareLength, true);
            RenderYZSquare(s2 * SquareLength, s3 * SquareLength, true);
            RenderXZSquare(s1 * SquareLength, s3 * SquareLength, true);
            
            OTK.OpenGL.GL.PopMatrix();
        }

        /// <summary>
        /// Renders control for picking</summary>
        /// <param name="action">Render action</param>
        /// <param name="camera">Camera</param>
        /// <param name="renderState">Render state</param>
        /// <param name="transform">Transform, local to world</param>
        public void RenderPick(IRenderAction action, Camera camera, RenderState renderState, Matrix4F transform)
        {
            action.RenderStateGuardian.Commit(renderState);

            // apply xform
            OTK.OpenGL.GL.PushMatrix();
            Util3D.glMultMatrixf(transform);

            float s1, s2, s3;
            CalcAxisLengths(camera, transform, out s1, out s2, out s3);
            float squareSideX = s1 * SquareLength;
            float squareSideY = s2 * SquareLength;
            float squareSideZ = s3 * SquareLength;

            bool drawX, drawY, drawZ;
            DecideArrowDrawing(transform, camera, out drawX, out drawY, out drawZ);

            // Mark each axis with a different name.
            // Since the OpenGl selection buffer does a bad job in detecting lines we'll use
            //  triangles instead.
            OTK.OpenGL.GL.PushName(m_nameBase);

            if (drawX)
            {
                OTK.OpenGL.GL.PushName((int) HitElement.X_ARROW);
                OTK.OpenGL.GL.Begin(OTK.OpenGL.PrimitiveType.Triangles);
                OTK.OpenGL.GL.Vertex3(squareSideX, 0, 0);
                OTK.OpenGL.GL.Vertex3(s1, 0, 0);
                OTK.OpenGL.GL.Vertex3(squareSideX, 0, 0);
                OTK.OpenGL.GL.End();
                RenderXArrow(s1);
                OTK.OpenGL.GL.PopName();
            }

            if (drawY)
            {
                OTK.OpenGL.GL.PushName((int)HitElement.Y_ARROW);
                OTK.OpenGL.GL.Begin(OTK.OpenGL.PrimitiveType.Triangles);
                OTK.OpenGL.GL.Vertex3(0, squareSideY, 0);
                OTK.OpenGL.GL.Vertex3(0, s2, 0);
                OTK.OpenGL.GL.Vertex3(0, squareSideY, 0);
                OTK.OpenGL.GL.End();
                RenderYArrow(s2);
                OTK.OpenGL.GL.PopName();
            }

            if (drawZ)
            {
                OTK.OpenGL.GL.PushName((int)HitElement.Z_ARROW);
                OTK.OpenGL.GL.Begin(OTK.OpenGL.PrimitiveType.Triangles);
                OTK.OpenGL.GL.Vertex3(0, 0, squareSideZ);
                OTK.OpenGL.GL.Vertex3(0, 0, s3);
                OTK.OpenGL.GL.Vertex3(0, 0, squareSideZ);
                OTK.OpenGL.GL.End();
                RenderZArrow(s3);
                OTK.OpenGL.GL.PopName();
            }

            if (drawX && drawY)
            {
                OTK.OpenGL.GL.PushName((int)HitElement.XY_SQUARE);
                RenderXYSquare(squareSideX, squareSideY, true);
                OTK.OpenGL.GL.PopName();
            }

            if (drawY && drawZ)
            {
                OTK.OpenGL.GL.PushName((int)HitElement.YZ_SQUARE);
                RenderYZSquare(squareSideY, squareSideZ, true);
                OTK.OpenGL.GL.PopName();
            }

            if (drawX && drawZ)
            {
                OTK.OpenGL.GL.PushName((int)HitElement.XZ_SQUARE);
                RenderXZSquare(squareSideX, squareSideZ, true);
                OTK.OpenGL.GL.PopName();
            }

            OTK.OpenGL.GL.PopMatrix();
            OTK.OpenGL.GL.PopName();
        }

        /// <summary>
        /// Gets and sets a value indicating if free form projection is to be used</summary>
        public bool FreeFormProjection
        {
            get { return m_freeFormProjection; }
            set { m_freeFormProjection = value; }
        }

        /// <summary>
        /// Gets and sets the free form projection plane</summary>
        public Vec4F FreeFormProjectionPlane
        {
            get { return m_freeFormAxis; }
            set { m_freeFormAxis = value; }
        }

        /// <summary>
        /// Returns whether the control was hit during a picking operation</summary>
        /// <param name="hit">Hit record</param>
        /// <returns>True if the control was hit during a picking operation</returns>
        public bool IsHit(HitRecord hit)
        {
            return (hit.RenderObjectData[0] == m_nameBase);
        }

        /// <summary>
        /// Performs actions at the beginning of control drag</summary>
        /// <param name="hit">Hit record</param>
        /// <param name="x">Mouse x position</param>
        /// <param name="y">Mouse y position</param>
        /// <param name="action">Render action</param>
        /// <param name="camera">Camera</param>
        /// <param name="transform">Transform</param>
        /// <returns>Code for control element under mouse x, y</returns>
        public HitElement OnHit(HitRecord hit, float x, float y, IRenderAction action, Camera camera, Matrix4F transform)
        {
            // Store pick coords
            m_iX = x;
            m_iY = y;
            return (HitElement)hit.RenderObjectData[1];
        }

        /// <summary>
        /// Performs actions during control drag</summary>
        /// <param name="hit">Hit record</param>
        /// <param name="x">Mouse x position</param>
        /// <param name="y">Mouse y position</param>
        /// <param name="action">Render action</param>
        /// <param name="camera">Camera</param>
        /// <param name="transform">Transform</param>
        /// <returns>Translation, in world coordinates</returns>
        public Vec3F OnDrag(HitRecord hit, float x, float y, IRenderAction action, Camera camera, Matrix4F transform)
        {
            float a1, a2;
            Matrix4F W = new Matrix4F();
            W.Mul(transform, camera.ViewMatrix);

            // Setup rays, in view space. (-z goes into the screen.)
            Ray3F ray0 = camera.CreateRay(m_iX, m_iY);
            Ray3F ray = camera.CreateRay(x, y);

            // Build axis and origin in view space
            Vec3F xAxis = W.XAxis;
            Vec3F yAxis = W.YAxis;
            Vec3F zAxis = W.ZAxis;
            Vec3F origin = W.Translation;

            Vec3F trans = new Vec3F();

            // Choose the best projection plane according to the projection angle
            switch ((HitElement)hit.RenderObjectData[1])
            {
                case HitElement.X_ARROW:
                    {
                        a1 = Math.Abs(Vec3F.Dot(ray0.Direction, yAxis));
                        a2 = Math.Abs(Vec3F.Dot(ray0.Direction, zAxis));
                        Vec3F axis = (a1 > a2 ? yAxis : zAxis);
                        Vec3F p0 = ray0.IntersectPlane(axis, -Vec3F.Dot(axis, origin));
                        Vec3F p1 = ray.IntersectPlane(axis, -Vec3F.Dot(axis, origin));
                        float dragAmount = Vec3F.Dot(xAxis, p1 - p0);
                        Vec3F xLocal = transform.XAxis;
                        trans = dragAmount * xLocal;
                    }
                    break;

                case HitElement.Y_ARROW:
                    {
                        a1 = Math.Abs(Vec3F.Dot(ray0.Direction, zAxis));
                        a2 = Math.Abs(Vec3F.Dot(ray0.Direction, xAxis));
                        Vec3F axis = (a1 > a2 ? zAxis : xAxis);
                        Vec3F p0 = ray0.IntersectPlane(axis, -Vec3F.Dot(axis, origin));
                        Vec3F p1 = ray.IntersectPlane(axis, -Vec3F.Dot(axis, origin));
                        float dragAmount = Vec3F.Dot(yAxis, p1 - p0);
                        Vec3F yLocal = transform.YAxis;
                        trans = dragAmount * yLocal;
                    }
                    break;

                case HitElement.Z_ARROW:
                    {
                        a1 = Math.Abs(Vec3F.Dot(ray0.Direction, xAxis));
                        a2 = Math.Abs(Vec3F.Dot(ray0.Direction, yAxis));
                        Vec3F axis = (a1 > a2 ? xAxis : yAxis);
                        Vec3F p0 = ray0.IntersectPlane(axis, -Vec3F.Dot(axis, origin));
                        Vec3F p1 = ray.IntersectPlane(axis, -Vec3F.Dot(axis, origin));
                        float dragAmount = Vec3F.Dot(zAxis, p1 - p0);
                        Vec3F zLocal = transform.ZAxis;
                        trans = dragAmount * zLocal;
                    }
                    break;
                
                case HitElement.XY_SQUARE:
                    {
                        Vec3F p0 = ray0.IntersectPlane(zAxis, -Vec3F.Dot(zAxis, origin));
                        Vec3F p1 = ray.IntersectPlane(zAxis, -Vec3F.Dot(zAxis, origin));
                        Vec3F deltaLocal = p1 - p0;
                        float dragX = Vec3F.Dot(xAxis, deltaLocal);
                        float dragY = Vec3F.Dot(yAxis, deltaLocal);
                        Vec3F xLocal = transform.XAxis;
                        Vec3F yLocal = transform.YAxis;
                        trans = dragX * xLocal + dragY * yLocal;
                    }
                    break;

                case HitElement.YZ_SQUARE:
                    {
                        Vec3F p0 = ray0.IntersectPlane(xAxis, -Vec3F.Dot(xAxis, origin));
                        Vec3F p1 = ray.IntersectPlane(xAxis, -Vec3F.Dot(xAxis, origin));
                        Vec3F deltaLocal = p1 - p0;
                        float dragY = Vec3F.Dot(yAxis, deltaLocal);
                        float dragZ = Vec3F.Dot(zAxis, deltaLocal);
                        Vec3F yLocal = transform.YAxis;
                        Vec3F zLocal = transform.ZAxis;
                        trans = dragY * yLocal + dragZ * zLocal;
                    }
                    break;

                case HitElement.XZ_SQUARE:
                    {
                        Vec3F p0 = ray0.IntersectPlane(yAxis, -Vec3F.Dot(yAxis, origin));
                        Vec3F p1 = ray.IntersectPlane(yAxis, -Vec3F.Dot(yAxis, origin));
                        Vec3F deltaLocal = p1 - p0;
                        float dragX = Vec3F.Dot(xAxis, deltaLocal);
                        float dragZ = Vec3F.Dot(zAxis, deltaLocal);
                        Vec3F xLocal = transform.XAxis;
                        Vec3F zLocal = transform.ZAxis;
                        trans = dragX * xLocal + dragZ * zLocal;
                    }
                    break;
            }
            return trans;
        }

        private void RenderXAxis(float s)
        {
            OTK.OpenGL.GL.Color3(1.0f, 0.0f, 0.0f);
            OTK.OpenGL.GL.Begin(OTK.OpenGL.PrimitiveType.Lines);
            OTK.OpenGL.GL.Vertex3(1.5f * SquareLength * s, 0, 0);
            OTK.OpenGL.GL.Vertex3(s, 0, 0);
            OTK.OpenGL.GL.End();

        }

        private void RenderXArrow(float s)
        {
            OTK.OpenGL.GL.Color3(1, 0, 0);

            OTK.OpenGL.GL.PushMatrix();
            OTK.OpenGL.GL.Translate(s, 0, 0);
            OTK.OpenGL.GL.Rotate(90, 0, 1, 0);
            Util3D.DrawCube(s / 8, Util3D.RenderStyle.Solid);
            OTK.OpenGL.GL.PopMatrix();
        }

        private void RenderYAxis(float s)
        {
            OTK.OpenGL.GL.Color3(0.0f, 1.0f, 0.0f);
            OTK.OpenGL.GL.Begin(OTK.OpenGL.PrimitiveType.Lines);
            OTK.OpenGL.GL.Vertex3(0, 1.5f * SquareLength * s, 0);
            OTK.OpenGL.GL.Vertex3(0, s, 0);
            OTK.OpenGL.GL.End();
        }

        private void RenderYArrow(float s)
        {
            OTK.OpenGL.GL.Color3(0, 1, 0);

            OTK.OpenGL.GL.PushMatrix();
            OTK.OpenGL.GL.Translate(0, s, 0);
            OTK.OpenGL.GL.Rotate(-90, 1, 0, 0);
            Util3D.DrawCube(s / 8, Util3D.RenderStyle.Solid);
            OTK.OpenGL.GL.PopMatrix();
        }

        private void RenderZAxis(float s)
        {
            OTK.OpenGL.GL.Color3(0.0f, 0.0f, 1.0f);
            OTK.OpenGL.GL.Begin(OTK.OpenGL.PrimitiveType.Lines);
            OTK.OpenGL.GL.Vertex3(0, 0, 1.5f * SquareLength * s);
            OTK.OpenGL.GL.Vertex3(0, 0, s);
            OTK.OpenGL.GL.End();
        }

        private void RenderZArrow(float s)
        {
            OTK.OpenGL.GL.Color3(0.0f, 0.0f, 1.0f);

            OTK.OpenGL.GL.PushMatrix();
            OTK.OpenGL.GL.Translate(0, 0, s);
            Util3D.DrawCube(s / 8, Util3D.RenderStyle.Solid);
            OTK.OpenGL.GL.PopMatrix();
        }

        private void RenderXYSquare(float a, float b, bool solid)
        {
            if (solid)
            {
                OTK.OpenGL.GL.Color4(0.5f, 0.5f, 0.5f, 0.4f);
                OTK.OpenGL.GL.Begin(OTK.OpenGL.PrimitiveType.TriangleFan);
                OTK.OpenGL.GL.Vertex3(0, 0, 0);
                OTK.OpenGL.GL.Vertex3(a, 0, 0);
                OTK.OpenGL.GL.Vertex3(a, b, 0);
                OTK.OpenGL.GL.Vertex3(0, b, 0);
                OTK.OpenGL.GL.End();
            }

            OTK.OpenGL.GL.Color3(1.0f, 0.0f, 0.0f);
            OTK.OpenGL.GL.Begin(OTK.OpenGL.PrimitiveType.Lines);
            OTK.OpenGL.GL.Vertex3(0, 0, 0);
            OTK.OpenGL.GL.Vertex3(a, 0, 0);
            OTK.OpenGL.GL.Vertex3(a, 0, 0);
            OTK.OpenGL.GL.Vertex3(a, b, 0);
            OTK.OpenGL.GL.End();

            OTK.OpenGL.GL.Color3(0.0f, 1.0f, 0.0f);
            OTK.OpenGL.GL.Begin(OTK.OpenGL.PrimitiveType.Lines);
            OTK.OpenGL.GL.Vertex3(a, b, 0);
            OTK.OpenGL.GL.Vertex3(0, b, 0);
            OTK.OpenGL.GL.Vertex3(0, b, 0);
            OTK.OpenGL.GL.Vertex3(0, 0, 0);
            OTK.OpenGL.GL.End();
        }

        private void RenderYZSquare(float a, float b, bool solid)
        {
            if (solid)
            {
                OTK.OpenGL.GL.Color4(0.5f, 0.5f, 0.5f, 0.4f);
                OTK.OpenGL.GL.Begin(OTK.OpenGL.PrimitiveType.TriangleFan);
                OTK.OpenGL.GL.Vertex3(0, 0, 0);
                OTK.OpenGL.GL.Vertex3(0, a, 0);
                OTK.OpenGL.GL.Vertex3(0, a, b);
                OTK.OpenGL.GL.Vertex3(0, 0, b);
                OTK.OpenGL.GL.End();
            }

            OTK.OpenGL.GL.Color3(0.0f, 1.0f, 0.0f);
            OTK.OpenGL.GL.Begin(OTK.OpenGL.PrimitiveType.Lines);
            OTK.OpenGL.GL.Vertex3(0, 0, 0);
            OTK.OpenGL.GL.Vertex3(0, a, 0);
            OTK.OpenGL.GL.Vertex3(0, a, 0);
            OTK.OpenGL.GL.Vertex3(0, a, b);
            OTK.OpenGL.GL.End();

            OTK.OpenGL.GL.Color3(0.0f, 0.0f, 1.0f);
            OTK.OpenGL.GL.Begin(OTK.OpenGL.PrimitiveType.Lines);
            OTK.OpenGL.GL.Vertex3(0, a, b);
            OTK.OpenGL.GL.Vertex3(0, 0, b);
            OTK.OpenGL.GL.Vertex3(0, 0, b);
            OTK.OpenGL.GL.Vertex3(0, 0, 0);
            OTK.OpenGL.GL.End();
        }

        private void RenderXZSquare(float a, float b, bool solid)
        {
            if (solid)
            {
                OTK.OpenGL.GL.Color4(0.5f, 0.5f, 0.5f, 0.4f);
                OTK.OpenGL.GL.Begin(OTK.OpenGL.PrimitiveType.TriangleFan);
                OTK.OpenGL.GL.Vertex3(0, 0, 0);
                OTK.OpenGL.GL.Vertex3(a, 0, 0);
                OTK.OpenGL.GL.Vertex3(a, 0, b);
                OTK.OpenGL.GL.Vertex3(0, 0, b);
                OTK.OpenGL.GL.End();
            }

            OTK.OpenGL.GL.Color3(1.0f, 0.0f, 0.0f);
            OTK.OpenGL.GL.Begin(OTK.OpenGL.PrimitiveType.Lines);
            OTK.OpenGL.GL.Vertex3(0, 0, 0);
            OTK.OpenGL.GL.Vertex3(a, 0, 0);
            OTK.OpenGL.GL.Vertex3(a, 0, 0);
            OTK.OpenGL.GL.Vertex3(a, 0, b);
            OTK.OpenGL.GL.End();

            OTK.OpenGL.GL.Color3(0.0f, 0.0f, 1.0f);
            OTK.OpenGL.GL.Begin(OTK.OpenGL.PrimitiveType.Lines);
            OTK.OpenGL.GL.Vertex3(a, 0, b);
            OTK.OpenGL.GL.Vertex3(0, 0, b);
            OTK.OpenGL.GL.Vertex3(0, 0, b);
            OTK.OpenGL.GL.Vertex3(0, 0, 0);
            OTK.OpenGL.GL.End();
        }

        private void CalcAxisLengths(Camera camera, Matrix4F globalTransform, out float s1, out float s2, out float s3)
        {
            float worldHeight;

            // Calc view space matrix
            Matrix4F V = new Matrix4F();
            V.Mul(globalTransform, camera.ViewMatrix);

            // World height on origin's z value
            if (camera.Frustum.IsOrtho)
            {
                worldHeight = (camera.Frustum.Top - camera.Frustum.Bottom) / 2;
            }
            else
            {
                worldHeight = -V.ZTranslation * (float)Math.Tan(camera.Frustum.FovY / 2.0f);
            }

            s1 = (m_axisRatio * worldHeight) / V.XAxis.Length;
            s2 = (m_axisRatio * worldHeight) / V.YAxis.Length;
            s3 = (m_axisRatio * worldHeight) / V.ZAxis.Length;
        }
        
        private void DecideArrowDrawing(Matrix4F localToWorld, Camera camera,
            out bool drawX, out bool drawY, out bool drawZ)
        {
            // Use our axis in world space, so we can prevent picking of lines and squares that
            //  are being viewed edge-on which leads to numerical instability.
            Vec3F xAxis = localToWorld.XAxis;
            Vec3F yAxis = localToWorld.YAxis;
            Vec3F zAxis = localToWorld.ZAxis;
            Vec3F control = localToWorld.Translation;

            Vec3F eyeToControl;
            if (camera.ProjectionType == ProjectionType.Perspective)
            {
                eyeToControl = control - camera.WorldEye;
                eyeToControl.Normalize();
            }
            else
            {
                eyeToControl = camera.WorldLookAt;
            }

            drawX = Math.Abs(Vec3F.Dot(eyeToControl, xAxis)) < 1.0f - EdgeAngle;
            drawY = Math.Abs(Vec3F.Dot(eyeToControl, yAxis)) < 1.0f - EdgeAngle;
            drawZ = Math.Abs(Vec3F.Dot(eyeToControl, zAxis)) < 1.0f - EdgeAngle;
        }

        static TranslatorControl()
        {
            s_renderState = new RenderState();
            s_renderState.RenderMode = RenderMode.Smooth | RenderMode.DisableZBuffer | RenderMode.Alpha;
        }

        private float m_axisRatio = 0.25f;
        private float m_iX;
        private float m_iY;
        private readonly int m_nameBase;
        private bool m_freeFormProjection = false;
        private Vec4F m_freeFormAxis = new Vec4F(0, 0, 1, 0);

        // Minimum cosine of the angle between view ray and "most perpendicular axis".
        private const float EdgeAngle = 0.004f;
        private const float SquareLength = 0.3f; //the ratio of square length to arrow length

        private static readonly RenderState s_renderState;
    }
}
