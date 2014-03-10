//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;

using Sce.Atf.Adaptation;
using Sce.Atf.Dom;
using Sce.Atf.VectorMath;

namespace Sce.Atf.Rendering.Dom
{
    /// <summary>
    /// 3D transformation utilities</summary>
    public static class TransformUtils
    {
        /// <summary>
        /// Calculates the world space matrix of the given SceneNode path</summary>
        /// <param name="path">The SceneNode path</param>
        /// <returns>The world space matrix</returns>
        public static Matrix4F CalcPathTransform(SceneNode[] path)
        {
            return CalcPathTransform(path, 0);
        }

        /// <summary>
        /// Calculates the world space matrix of the given SceneNode path, starting from a given index</summary>
        /// <param name="path">The SceneNode path</param>
        /// <param name="start">Starting index within the path</param>
        /// <returns>The world space matrix</returns>
        public static Matrix4F CalcPathTransform(SceneNode[] path, int start)
        {
            Matrix4F M = new Matrix4F();

            for (int i = start; i < path.Length; i++)
            {
                if (path[i].Source != null)
                {
                    ITransformable renderable =
                        path[i].Source.As<ITransformable>();

                    if (renderable != null)
                    {
                        M.Mul(M, renderable.Transform);
                    }
                }
            }

            return M;
        }

        /// <summary>
        /// Calculates the world space matrix of the given path</summary>
        /// <param name="path">The path</param>
        /// <param name="start">Starting index</param>
        /// <returns>The world space matrix</returns>
        public static Matrix4F CalcPathTransform(Path<DomNode> path, int start)
        {
            Matrix4F M = new Matrix4F();

            for (int i = start; i >= 0; i--)
            {
                if (path[i] != null)
                {
                    ITransformable renderable =
                        path[i].As<ITransformable>();

                    if (renderable != null)
                    {
                        M.Mul(M, renderable.Transform);
                    }
                }
            }

            return M;
        }

        /// <summary>
        /// Calculates the bounding box, in world space, for the given path to a DomNode.
        /// Typically this comes from a selection set.</summary>
        /// <param name="path">Path</param>
        /// <returns>The bounding box in world space, or a new empty uninitialized box
        /// if no IBoundable could be found</returns>
        public static Box CalcWorldBoundingBox(Path<DomNode> path)
        {
            // Find the lowest level IBoundable in the given path.
            int startIndex = path.Count;
            IBoundable boundable = null;
            while (--startIndex >= 0)
            {
                if (path[startIndex] != null)
                {
                    boundable = path[startIndex].As<IBoundable>();

                    if (boundable != null)
                        break;
                }
            }
            if (boundable == null)
                return new Box();

            // startIndex is the index of the lowest-level IBoundable that we could find.
            //  But we want to calculate the transform down to boundable's parent because
            //  the bounding box needs to be transformed by the parent's transform, not
            //  boundable's transform (if any).
            Matrix4F localToWorld = CalcPathTransform(path, startIndex - 1);

            Box box = boundable.BoundingBox;
            box.Transform(localToWorld);
            return box;
        }

        /// <summary>
        /// Gets an array of the snap-from modes understood by CalcSnapFromOffset().
        /// These are:
        /// "Pivot" -- the pivot point.
        /// "Origin" -- the object's origin.
        /// "Bottom Center" -- the bottom center of the bounding box.</summary>
        public static string[] SnapFromModes
        {
            get
            {
                return new[]
                {
                    Pivot,
                    Origin,
                    BottomCenter,
                };
            }
        }

        /// <summary>
        /// Gets the SnapFromMode enum value that corresponds to the given snap-from mode string</summary>
        /// <param name="modeString">Must be a valid string name that came from the SnapFromModes property.
        /// If null, the default SnapFromMode value, Pivot, is returned</param>
        /// <returns>SnapFromMode enum value that corresponds to the given snap-from mode string</returns>
        public static SnapFromMode GetSnapFromMode(string modeString)
        {
            if (modeString == null || modeString == Pivot)
                return SnapFromMode.Pivot;
            if (modeString == Origin)
                return SnapFromMode.Origin;
            if (modeString == BottomCenter)
                return SnapFromMode.BottomCenter;
            throw new ArgumentException("Unknown snap-from mode string", "modeString");
        }

        /// <summary>
        /// Gets the user-readable string that corresponds to the given SnapFromMode enum</summary>
        /// <param name="mode">SnapFromMode value</param>
        /// <returns>String corresponding to given SnapFromMode value</returns>
        public static string GetSnapFromMode(SnapFromMode mode)
        {
            switch (mode)
            {
                case SnapFromMode.Pivot:
                    return Pivot;
                case SnapFromMode.Origin:
                    return Origin;
                case SnapFromMode.BottomCenter:
                    return BottomCenter;
                default:
                    throw new ArgumentOutOfRangeException("mode");
            }
        }

        /// <summary>
        /// For an object 'node' that is being manipulated, this function returns
        /// the offset (in world space) from the object's origin to to the point that is being
        /// "snapped from". This calculation is determined by the snapping modes.</summary>
        /// <param name="node">The object that is being snapped-to some other object</param>
        /// <param name="snapFromMode">The "snap from" mode, as a string. See SnapFromModes property.
        /// Pass in 'null' to get the default, which is the offset to the pivot point.</param>
        /// <param name="axisType">Axis system type (y or z is up)</param>
        /// <param name="pivot">Pass in either node.RotatePivot or node.ScalePivot</param>
        /// <returns>Offset from this object's origin to the "snap from" point, in world space</returns>
        /// <remarks>Must be kept in sync with SnapFromModes property.</remarks>
        public static Vec3F CalcSnapFromOffset(
            ITransformable node,
            string snapFromMode,
            AxisSystemType axisType, Vec3F pivot)
        {
            SnapFromMode mode = GetSnapFromMode(snapFromMode);
            return CalcSnapFromOffset(node, mode, axisType, pivot);
        }

        /// <summary>
        /// For an object 'node' that is being manipulated, this function returns
        /// the offset (in world space) from the object's origin to to the point that is being
        /// "snapped from". This calculation is determined by the snapping modes.</summary>
        /// <param name="node">The object that is being snapped-to some other object</param>
        /// <param name="snapFromMode">The "snap from" mode, as an enum</param>
        /// <param name="axisType">Axis system type (y or z is up)</param>
        /// <param name="pivot">Pass in either node.RotatePivot or node.ScalePivot</param>
        /// <returns>Offset from this object's origin to the "snap from" point, in world space</returns>
        /// <remarks>Must be kept in sync with SnapFromModes property.</remarks>
        public static Vec3F CalcSnapFromOffset(
            ITransformable node,
            SnapFromMode snapFromMode,
            AxisSystemType axisType, Vec3F pivot)
        {
            switch (snapFromMode)
            {
                case SnapFromMode.Pivot:
                    {
                        Vec3F offset;
                        Path<DomNode> path = new Path<DomNode>(node.Cast<DomNode>().Ancestry);
                        Matrix4F parentToWorld = TransformUtils.CalcPathTransform(path, path.Count - 2);
                        node.Transform.TransformVector(pivot, out offset); //local-to-parent
                        parentToWorld.TransformVector(offset, out offset); //parent-to-world
                        return offset; //world
                    }
                case SnapFromMode.Origin:
                    return new Vec3F(0, 0, 0);
                case SnapFromMode.BottomCenter:
                    {
                        Box box = node.BoundingBox;
                        Vec3F btmWorld;
                        if (axisType == AxisSystemType.YIsUp)
                        {
                            btmWorld = new Vec3F(
                                (box.Min.X + box.Max.X) * 0.5f,
                                box.Min.Y,
                                (box.Min.Z + box.Max.Z) * 0.5f);
                        }
                        else
                        {
                            btmWorld = new Vec3F(
                                (box.Min.X + box.Max.X) * 0.5f,
                                (box.Min.Y + box.Max.Y) * 0.5f,
                                box.Min.Z);
                        }
                        Vec3F origin = node.Transform.Translation;
                        Vec3F offset = btmWorld - origin; //local space offset

                        Path<DomNode> path = new Path<DomNode>(node.Cast<DomNode>().GetPath());
                        Matrix4F parentToWorld = TransformUtils.CalcPathTransform(path, path.Count - 2);
                        parentToWorld.TransformVector(offset, out offset);

                        return offset;
                    }
                default:
                    throw new ArgumentException("Invalid snap-from node");
            }
        }

        /// <summary>
        /// For an object 'node' that is being manipulated, this function returns
        /// the offset (in world space) from the object's origin to to the point that is being
        /// "snapped from". This calculation is determined by the snapping modes.</summary>
        /// <param name="node">The object that is being snapped-to some other object</param>
        /// <param name="snapFromMode">The "snap from" mode, as a string. See SnapFromModes property.
        /// Pass in 'null' to get the default which is the offset to the pivot point.</param>
        /// <param name="axisType">Whether the Y axis is up or down</param>
        /// <returns>Offset from this object's origin to the "snap from" point, in world space</returns>
        /// <remarks>Must be kept in sync with SnapFromModes property.</remarks>
        public static Vec3F CalcSnapFromOffset(
            ITransformable node,
            string snapFromMode,
            AxisSystemType axisType)
        {
            SnapFromMode mode = GetSnapFromMode(snapFromMode);
            return CalcSnapFromOffset(node, mode, axisType);
        }

        /// <summary>
        /// For an object 'node' that is being manipulated, this function returns
        /// the offset (in world space) from the object's origin to to the point that is being
        /// "snapped from". This calculation is determined by the snapping modes.</summary>
        /// <param name="node">The object that is being snapped-to some other object</param>
        /// <param name="snapFromMode">The "snap from" mode, as an enum</param>
        /// <param name="axisType">Whether the Y axis is up or down</param>
        /// <returns>Offset from this object's origin to the "snap from" point, in world space</returns>
        /// <remarks>Must be kept in sync with SnapFromModes property.</remarks>
        public static Vec3F CalcSnapFromOffset(
            ITransformable node,
            SnapFromMode snapFromMode,
            AxisSystemType axisType)
        {
            Vec3F rotatePivot = new Vec3F();
            if ((node.TransformationType & TransformationTypes.RotatePivot) != 0)
                rotatePivot = node.RotatePivot;

            return CalcSnapFromOffset(node, snapFromMode, axisType, rotatePivot);
        }

        /// <summary>
        /// Given an object's current Euler angles and a surface normal, calculates
        /// the Euler angles necessary to rotate the object so that its up-vector is
        /// aligned with the surface normal</summary>
        /// <param name="originalEulers">Original Euler angles, from an IRenderableNode.Rotation, for example</param>
        /// <param name="surfaceNormal">A unit vector that the object should be rotate-snapped to</param>
        /// <param name="upAxis">Whether the Y axis is up or down</param>
        /// <returns>The resulting angles to be assigned to IRenderableNode.Rotation</returns>
        /// <remarks>
        /// Note that QuatF was attempted to be used, but I could not get it to work reliably
        /// with the Matrix3F.GetEulerAngles(). Numerical instability? The basis vector
        /// method below works well, except for when the target surface is 90 degrees different
        /// than the starting up vector. In this case, the rotation around the up vector is lost
        /// (gimbal lock), but the results are always valid in the sense that the up vector
        /// is aligned with the surface normal. --Ron Little</remarks>
        public static Vec3F RotateToVector(Vec3F originalEulers, Vec3F surfaceNormal, AxisSystemType upAxis)
        {
            // get basis vectors for the current rotation
            Matrix3F rotMat = new Matrix3F();
            rotMat.Rotation(originalEulers);
            Vec3F a1 = rotMat.XAxis;
            Vec3F a2 = rotMat.YAxis;
            Vec3F a3 = rotMat.ZAxis;

            // calculate destination basis vectors
            Vec3F b1, b2, b3;
            if (upAxis == AxisSystemType.YIsUp)
            {
                // a2 is the current up vector. b2 is the final up vector.
                // now, find either a1 or a3, whichever is most orthogonal to surface
                b2 = new Vec3F(surfaceNormal);
                float a1DotS = Vec3F.Dot(a1, surfaceNormal);
                float a3DotS = Vec3F.Dot(a3, surfaceNormal);
                if (Math.Abs(a1DotS) < Math.Abs(a3DotS))
                {
                    b1 = new Vec3F(a1);
                    b3 = Vec3F.Cross(b1, b2);
                    b1 = Vec3F.Cross(b2, b3);
                }
                else
                {
                    b3 = new Vec3F(a3);
                    b1 = Vec3F.Cross(b2, b3);
                    b3 = Vec3F.Cross(b1, b2);
                }
            }
            else
            {
                // a3 is the current up vector. b3 is the final up vector.
                // now, find either a1 or a2, whichever is most orthogonal to surface
                b3 = new Vec3F(surfaceNormal);
                float a1DotS = Vec3F.Dot(a1, surfaceNormal);
                float a2DotS = Vec3F.Dot(a2, surfaceNormal);
                if (Math.Abs(a1DotS) < Math.Abs(a2DotS))
                {
                    b1 = new Vec3F(a1);
                    b2 = Vec3F.Cross(b3, b1);
                    b1 = Vec3F.Cross(b2, b3);
                }
                else
                {
                    b2 = new Vec3F(a2);
                    b1 = Vec3F.Cross(b2, b3);
                    b2 = Vec3F.Cross(b3, b1);
                }
            }

            // in theory, this isn't necessary, but in practice...
            b1.Normalize();
            b2.Normalize();
            b3.Normalize();

            // construct new rotation matrix and extract euler angles
            rotMat.XAxis = b1;
            rotMat.YAxis = b2;
            rotMat.ZAxis = b3;

            Vec3F newEulers = new Vec3F();
            rotMat.GetEulerAngles(out newEulers.X, out newEulers.Y, out newEulers.Z);
            return newEulers;
        }

        /// <summary>
        /// Gets the vector in world space that points "up"</summary>
        /// <param name="axis">The axis system indicating which axis is up</param>
        /// <returns>The vector that points up</returns>
        public static Vec3F GetUpVector(AxisSystemType axis)
        {
            if (axis == AxisSystemType.YIsUp)
                return new Vec3F(0, 1, 0);
            else
                return new Vec3F(0, 0, 1);
        }

        /// <summary>
        /// Calculates the width and height of the view frustum at the given 3D location</summary>
        /// <param name="camera">The camera</param>
        /// <param name="globalTransform">The world space matrix specifying a position</param>
        /// <param name="h">The height of the view frustum at the given position</param>
        /// <param name="w">The width of the view frustum at the given position</param>
        public static void CalcWorldDimensions(Camera camera, Matrix4F globalTransform, out float h, out float w)
        {
            Matrix4F W = new Matrix4F();
            W.Mul(globalTransform, camera.ViewMatrix);

            // World height on origin's z value
            if (camera.Frustum.IsOrtho)
            {
                w = camera.Frustum.Right - camera.Frustum.Left;
                h = camera.Frustum.Top - camera.Frustum.Bottom;
            }
            else
            {
                float minusZ = -W.Translation.Z;
                h = minusZ * (float)Math.Tan(camera.Frustum.FovY / 2.0f) * 2.0f;
                w = minusZ * (float)Math.Tan(camera.Frustum.FovX / 2.0f) * 2.0f;
            }
        }

        /// <summary>
        /// Creates a ray originating at the given normalized window coordinates and pointing into
        /// the screen, along the z axis. Normalized window coordinates are in the range [-0.5,0.5],
        /// with +x pointing to the right and +y pointing up.</summary>
        /// <param name="x">The x normalized window coordinate</param>
        /// <param name="y">The y normalized window coordinate</param>
        /// <param name="camera">The camera</param>
        /// <returns>The ray</returns>
        public static Ray3F CreateRay(float x, float y, Camera camera)
        {
            return camera.CreateRay(x, y);
        }

        /// <summary>
        /// Adjusts child transform, making it relative to new parent node's transform.
        /// Is recursive, looking for parents that also implement IRenderableNode.</summary>
        /// <param name="parent">Parent node</param>
        /// <param name="child">Child node</param>
        public static void AddChild(ITransformable parent, ITransformable child)
        {
            Path<DomNode> path = new Path<DomNode>(parent.Cast<DomNode>().GetPath());
            Matrix4F parentToWorld = TransformUtils.CalcPathTransform(path, path.Count - 1);

            // We want 'child' to appear in the same place in the world after adding to 'parent'.
            // local-point * original-local-to-world = world-point
            // new-local-point * new-local-to-parent * parent-to-world = world-point
            // ==> new-local-to-parent * parent-to-world = original-local-to-world
            // (multiply both sides by inverse of parent-to-world; call it world-to-parent)
            // ==> new-local-to-parent = original-local-to-world * world-to-parent
            Matrix4F worldToParent = new Matrix4F();
            worldToParent.Invert(parentToWorld);
            Matrix4F originalLocalToWorld = child.Transform;
            Matrix4F newLocalToParent = Matrix4F.Multiply(originalLocalToWorld, worldToParent);

            // The translation component of newLocalToParent consists of pivot translation
            //  as well as the child.Translation. So, start with the original child.Translation
            //  and transform it into our new space.
            Vec3F newTranslation = child.Translation;
            worldToParent.Transform(ref newTranslation);

            // There's only one way of getting rotation info, so get it straight from matrix.
            Vec3F newRotation = new Vec3F();
            newLocalToParent.GetEulerAngles(out newRotation.X, out newRotation.Y, out newRotation.Z);
            child.Rotation = newRotation;

            // Likewise with scale.
            Vec3F newScale = newLocalToParent.GetScale();
            child.Scale = newScale;

            // We can compose together all of the separate transformations now.
            Matrix4F newTransform = CalcTransform(
                newTranslation,
                newRotation,
                newScale,
                child.ScalePivot,
                child.ScalePivotTranslation,
                child.RotatePivot,
                child.RotatePivotTranslation);

            // However, the composed matrix may not equal newLocalToParent due to rotating
            //  or scaling around a pivot. In the general case, it may be impossible to
            //  decompose newLocalToParent into all of these separate components. For example,
            //  a sheer transformation cannot be reproduced by a single rotation and scale.
            //  But for common cases, only the translation is out-of-sync now, so apply a fix.
            Vec3F desiredTranslation = newLocalToParent.Translation;
            Vec3F currentTranslation = newTransform.Translation;
            Vec3F fixupTranslation = desiredTranslation - currentTranslation;
            Matrix4F fixupTransform = new Matrix4F(fixupTranslation);
            newTransform.Mul(newTransform, fixupTransform);

            // Save the fix and the final transform. Storing the fix in RotatePivotTranslation
            //  is done elsewhere, as well.
            child.Translation = newTranslation + fixupTranslation;
            child.Transform = newTransform;
        }

        /// <summary>
        /// Adjusts child transform, making it the concatenation with its parent's transform.
        /// Is recursive, looking for parents that also implement IRenderableNode.</summary>
        /// <param name="parent">Parent node</param>
        /// <param name="child">Child node</param>
        public static void RemoveChild(ITransformable parent, ITransformable child)
        {
            Path<DomNode> path = new Path<DomNode>(parent.Cast<DomNode>().GetPath());
            Matrix4F parentMatrix = TransformUtils.CalcPathTransform(path, path.Count - 1);

            Matrix4F childMatrix = child.Transform;
            Matrix4F newChildMatrix = Matrix4F.Multiply(childMatrix, parentMatrix);

            Vec3F newTranslation = child.Translation;
            parentMatrix.Transform(ref newTranslation);

            Vec3F newRotation = new Vec3F();
            newChildMatrix.GetEulerAngles(out newRotation.X, out newRotation.Y, out newRotation.Z);
            child.Rotation = newRotation;

            Vec3F newScale = newChildMatrix.GetScale();
            child.Scale = newScale;

            // We can compose together all of the separate transformations now.
            Matrix4F newTransform = CalcTransform(
                newTranslation,
                newRotation,
                newScale,
                child.ScalePivot,
                child.ScalePivotTranslation,
                child.RotatePivot,
                child.RotatePivotTranslation);

            // However, the composed matrix may not equal newChildMatrix due to rotating
            //  or scaling around a pivot. In the general case, it may be impossible to
            //  decompose newChildMatrix into all of these separate components. For example,
            //  a sheer transformation cannot be reproduced by a single rotation and scale.
            //  But for common cases, only the translation is out-of-sync now, so apply a fix.
            Vec3F desiredTranslation = newChildMatrix.Translation;
            Vec3F currentTranslation = newTransform.Translation;
            Vec3F fixupTranslation = desiredTranslation - currentTranslation;
            Matrix4F fixupTransform = new Matrix4F(fixupTranslation);
            newTransform.Mul(newTransform, fixupTransform);

            // Save the fix and the final transform.
            child.Translation = newTranslation + fixupTranslation;
            child.Transform = newTransform;
        }

        /// <summary>
        /// Calculates the transformation matrix corresponding to the given renderable node</summary>
        /// <param name="node">Renderable node</param>
        /// <returns>Transformation matrix corresponding to the node's transform components</returns>
        public static Matrix4F CalcTransform(ITransformable node)
        {
            return CalcTransform(
                node.Translation,
                node.Rotation,
                node.Scale,
                node.ScalePivot,
                node.ScalePivotTranslation,
                node.RotatePivot,
                node.RotatePivotTranslation);
        }

        /// <summary>
        /// Calculates the transformation matrix corresponding to the given transform components</summary>
        /// <param name="translation">Translation</param>
        /// <param name="rotation">Rotation</param>
        /// <param name="scale">Scale</param>
        /// <param name="scalePivot">Translation to origin of scaling</param>
        /// <param name="scalePivotTranslate">Translation after scaling</param>
        /// <param name="rotatePivot">Translation to origin of rotation</param>
        /// <param name="rotatePivotTranslate">Translation after rotation</param>
        /// <returns>Transformation matrix corresponding to the given transform components</returns>
        public static Matrix4F CalcTransform(
            Vec3F translation,
            Vec3F rotation,
            Vec3F scale,
            Vec3F scalePivot,
            Vec3F scalePivotTranslate,
            Vec3F rotatePivot,
            Vec3F rotatePivotTranslate)
        {
            Matrix4F M = new Matrix4F();
            Matrix4F temp = new Matrix4F();

            M.Set(-scalePivot);

            temp.Scale(scale);
            M.Mul(M, temp);

            temp.Set(scalePivot + scalePivotTranslate - rotatePivot);
            M.Mul(M, temp);

            if (rotation.X != 0)
            {
                temp.RotX(rotation.X);
                M.Mul(M, temp);
            }

            if (rotation.Y != 0)
            {
                temp.RotY(rotation.Y);
                M.Mul(M, temp);
            }

            if (rotation.Z != 0)
            {
                temp.RotZ(rotation.Z);
                M.Mul(M, temp);
            }

            temp.Set(rotatePivot + rotatePivotTranslate + translation);
            M.Mul(M, temp);

            return M;
        }

        /// <summary>
        /// Transforms the given world or local point into viewport (Windows) coordinates</summary>
        /// <param name="localPoint">World or local point to be transformed</param>
        /// <param name="localToScreen">Tranformation matrix composed of object-to-world times
        /// world-to-view times view-to-projection</param>
        /// <param name="viewportWidth">The viewport width, for example Control.Width or
        /// IRenderAction.ViewportWidth</param>
        /// <param name="viewportHeight">The viewport height, for example Control.Height or
        /// IRenderAction.ViewportHeight</param>
        /// <returns>The viewport or Window coordinate in the range [0,Width] and [0,Height]
        /// where the origin is the upper left corner of the viewport. The coordinate could be
        /// outside of this range if localPoint is not visible.</returns>
        /// <example>
        /// To calculate localToScreen using an object's local-to-world and a Camera:
        ///     localToScreen = Matrix4F.Multiply(localToWorld, camera.ViewMatrix);
        ///     localToScreen.Mul(localToScreen, camera.ProjectionMatrix);
        /// </example>
        public static Vec2F TransformToViewport(
            Vec3F localPoint,
            Matrix4F localToScreen,
            float viewportWidth,
            float viewportHeight)
        {
            // transform to clip space and do perspective divide. Result is in range of [-1, 1]
            Vec4F xScreen = new Vec4F(localPoint);
            localToScreen.Transform(xScreen, out xScreen);
            xScreen = Vec4F.Mul(xScreen, 1.0f / xScreen.W);

            // get viewport coordinates. Convert [-1, 1] to [0, view size]
            Vec2F xViewport = new Vec2F(
                (xScreen.X + 1) * 0.5f * viewportWidth,
                (1 - (xScreen.Y + 1) * 0.5f) * viewportHeight);

            return xViewport;
        }

        // Strings for the "snap-from" feature.
        private static readonly string Pivot = "Pivot";
        private static readonly string Origin = "Origin";
        private static readonly string BottomCenter = "BottomCenter";
    }

    /// <summary>
    /// Enums that correspond to the strings in TransformUtils.SnapFromModes</summary>
    public enum SnapFromMode
    {
        Pivot,
        Origin,
        BottomCenter
    }
}
