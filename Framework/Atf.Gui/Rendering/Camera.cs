//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Globalization;
using System.Xml;

using Sce.Atf.VectorMath;

namespace Sce.Atf.Rendering
{
    /// <summary>
    /// Camera provides a centralized representation of the viewing transform</summary>
    public class Camera
    {
        /// <summary>
        /// Constructs a camera near the origin with default perspective projection</summary>
        public Camera() 
        {
            Set(
                new Vec3F(1, 1, 1),    // eye point
                new Vec3F(0, 0, 0),    // look at point
                new Vec3F(0, 1, 0));   // up vector

            SetPerspective(
                (float)Math.PI / 4.0f,  // y FOV
                1.0f,                   // aspect ratio
                0.01f,                  // near Z
                2048.0f);               // far Z
        }

        /// <summary>
        /// Sets this camera to have the same state as the given camera</summary>
        /// <param name="source">Source camera, which is not modified. This camera becomes a copy of it.</param>
        public void Init(Camera source)
        {
            ViewTypes perspective;
            Vec3F eye;
            Vec3F lookAtPoint;
            Vec3F upVector;
            float yFov;
            float nearZ;
            float farZ;
            float focusRadius;
            source.GetState(
                out perspective,
                out eye,
                out lookAtPoint,
                out upVector,
                out yFov,
                out nearZ,
                out farZ,
                out focusRadius);
            SetState(
                perspective,
                eye,
                lookAtPoint,
                upVector,
                yFov,
                nearZ,
                farZ,
                focusRadius);
        }

        /// <summary>
        /// Gets all the data required to completely represent this camera's state, so that it can
        /// later be restored by calling SetState(). Note that the aspect ratio is not saved and
        /// restored, because it should be set depending on the aspect ratio of the viewport; it would
        /// be wrong to restore the camera's aspect ratio after the user has resized the window, for example.</summary>
        /// <param name="perspective">Camera view type</param>
        /// <param name="eye">Eye point in "world view" space</param>
        /// <param name="lookAtPoint">Camera look-at point in "world view" space</param>
        /// <param name="upVector">Camera up direction in "world view" space</param>
        /// <param name="yFov">Y-field-of-view in radians</param>
        /// <param name="nearZ">Current near plane distance value for the current projection type</param>
        /// <param name="farZ">Current far plane distance value for the current projection type</param>
        /// <param name="focusRadius">Focus radius around the "look at" point</param>
        public void GetState(
            out ViewTypes perspective,
            out Vec3F eye,
            out Vec3F lookAtPoint,
            out Vec3F upVector,
            out float yFov,
            out float nearZ,
            out float farZ,
            out float focusRadius)
        {
            perspective = m_viewType;
            eye = Eye;
            lookAtPoint = LookAtPoint;
            upVector = Up;
            yFov = YFov;
            nearZ = NearZ;
            farZ = FarZ;
            focusRadius = FocusRadius;
        }

        /// <summary>
        /// Sets this cammera's state completely. Complements GetState().</summary>
        /// <param name="viewType">Camera view type</param>
        /// <param name="eye">Eye point in "world view" space</param>
        /// <param name="lookAtPoint">Camera look-at point in "world view" space</param>
        /// <param name="upVector">Camera up direction in "world view" space</param>
        /// <param name="yFov">Y-field-of-view in radians</param>
        /// <param name="nearZ">Current near plane distance value for the current projection type</param>
        /// <param name="farZ">Current far plane distance value for the current projection type</param>
        /// <param name="focusRadius">Focus radius around the "look at" point</param>
        public void SetState(
            ViewTypes viewType,
            Vec3F eye,
            Vec3F lookAtPoint,
            Vec3F upVector,
            float yFov,
            float nearZ,
            float farZ,
            float focusRadius)
        {
            // to avoid duplicate camera events, let's batch up all the changes.
            m_changingCamera = true;
            try
            {
                m_viewType = viewType;
                Set(eye, lookAtPoint, upVector);
                if (viewType == ViewTypes.Perspective)
                {
                    SetPerspective(yFov, m_aspectRatio, nearZ, farZ);
                }
                else
                {
                    float d = DistanceFromLookAt;
                    SetOrthographic(d * m_aspectRatio / 2,
                        -d * m_aspectRatio / 2,
                        d / 2,
                        -d / 2,
                        nearZ,
                        farZ);
                }
            }
            finally
            {
                m_changingCamera = false;
            }
            OnCameraChanged(EventArgs.Empty);
        }

        /// <summary>
        /// Gets all of the data required to completely represent this camera's state so that it can
        /// later be restored by calling SetState</summary>
        /// <param name="root">The XML element that stores this camera's state</param>
        /// <param name="xmlDoc">The XML document that owns 'root'</param>
        public void GetState(XmlElement root, XmlDocument xmlDoc)
        {
            // Get the state of this camera in an easy-to-consume format.
            ViewTypes viewType;
            Vec3F eye;
            Vec3F lookAtPoint;
            Vec3F upVector;
            float yFov;
            float nearZ;
            float farZ;
            float focusRadius;
            GetState(out viewType, out eye, out lookAtPoint, out upVector, out yFov,
                out nearZ, out farZ, out focusRadius);

            // Set the attributes.
            root.SetAttribute("viewType", string.Format("{0:x}", (int)viewType));
            root.SetAttribute("eye", eye.ToString());
            root.SetAttribute("lookAtPoint", lookAtPoint.ToString());
            root.SetAttribute("upVector", upVector.ToString());
            root.SetAttribute("yFov", yFov.ToString());
            root.SetAttribute("nearZ", nearZ.ToString());
            root.SetAttribute("farZ", farZ.ToString());
            root.SetAttribute("focusRadius", focusRadius.ToString());
        }

        /// <summary>
        /// Sets this cammera's state completely. Complements GetState().</summary>
        /// <param name="root">The XML element containing the state of a camera</param>
        /// <param name="xmlDoc">The XML document that owns 'root'</param>
        public void SetState(XmlElement root, XmlDocument xmlDoc)
        {
            ViewTypes viewType;
            Vec3F eye;
            Vec3F lookAtPoint;
            Vec3F upVector;
            float yFov;
            float nearZ;
            float farZ;
            float focusRadius;

            string value;

            value = root.GetAttribute("viewType");
            viewType = (ViewTypes)int.Parse(value, NumberStyles.AllowHexSpecifier);

            value = root.GetAttribute("eye");
            eye = Vec3F.Parse(value);

            value = root.GetAttribute("lookAtPoint");
            lookAtPoint = Vec3F.Parse(value);

            value = root.GetAttribute("upVector");
            upVector = Vec3F.Parse(value);

            value = root.GetAttribute("yFov");
            yFov = float.Parse(value);

            value = root.GetAttribute("nearZ");
            nearZ = float.Parse(value);

            value = root.GetAttribute("farZ");
            farZ = float.Parse(value);

            value = root.GetAttribute("focusRadius");
            focusRadius = float.Parse(value);

            SetState(viewType, eye, lookAtPoint, upVector, yFov, nearZ, farZ, focusRadius);
        }

        /// <summary>
        /// Event that is raised after the camera has changed</summary>
        public event EventHandler CameraChanged;

        /// <summary>
        /// Gets and sets the axis system matrix, which converts world space into a
        /// "world view" space used to support different handedness or up direction
        /// in a design view</summary>
        public Matrix4F AxisSystem
        {
            get { return m_axisSystem; }
            set
            {
                m_axisSystem = value;

                OnCameraChanged(EventArgs.Empty);
            }
        }

        /// <summary>
        /// Gets the inverse axis system matrix, which converts "world view" space into
        /// world space</summary>
        public Matrix4F InverseAxisSystem
        {
            get
            {
                Matrix4F inverseAxisSystem = new Matrix4F();
                inverseAxisSystem.Transpose(m_axisSystem);
                return inverseAxisSystem;
            }
        }

        /// <summary>
        /// Gets the camera projection type</summary>
        public ProjectionType ProjectionType
        {
            get { return m_projectionType; }
        }

        /// <summary>
        /// Gets and sets the camera view type</summary>
        public ViewTypes ViewType
        {
            get { return m_viewType; }
            set
            {
                if (m_viewType != value)
                {
                    m_viewType = value;

                    if (m_viewType == ViewTypes.Perspective)
                    {
                        // leave basis as-is
                        SetPerspective(m_yFov, m_aspectRatio, m_perspectiveNearZ, m_frustum.FarZ);
                    }
                    else
                    {
                        Vec3F lookAt;
                        Vec3F up;
                        GetViewVectors(out lookAt, out up);

                        float d = DistanceFromLookAt;
                        Set(m_lookAtPoint - lookAt * d, m_lookAtPoint, up);

                        SetOrthographic(d);
                    }
                }                
            }
        }

        /// <summary>
        /// Gets the localized name of the current ViewType</summary>
        public string ViewTypeName
        {
            get
            {
                switch( m_viewType)
                {
                    case ViewTypes.Back:
                        return "Back".Localize("the back side of the model that is being viewed");
                    case ViewTypes.Bottom:
                        return "Bottom".Localize("the bottom side of the model that is being viewed");
                    case ViewTypes.Front:
                        return "Front".Localize("the front side of the model that is being viewed");
                    case ViewTypes.Left:
                        return "Left".Localize("the left side of the model that is being viewed");
                    case ViewTypes.Right:
                        return "Right".Localize("the right side of the model that is being viewed");
                    case ViewTypes.Top:
                        return "Top".Localize("the top side of the model that is being viewed");
                    default:
                    case ViewTypes.Perspective:
                        return "Perspective".Localize("a 3D perspective view of the model");
                }
            }
        }

        /// <summary>
        /// Gets the view frustum</summary>
        public Frustum Frustum
        {
            get { return m_frustum; }
        }

        /// <summary>
        /// Gets the projection matrix</summary>
        /// <value>The projection matrix</value>
        public Matrix4F ProjectionMatrix
        {
            get
            {
                Matrix4F projection;

                if (Frustum.IsOrtho)
                {
                    float right = Frustum.Right;
                    float left = Frustum.Left;
                    float top = Frustum.Top;
                    float bottom = Frustum.Bottom;
                    float zNear = Frustum.Near;
                    float zFar = Frustum.Far;
                    float tx = -(right + left) / (right - left);
                    float ty = -(top + bottom) / (top - bottom);
                    float tz = -(zFar + zNear) / (zFar - zNear);

                    projection = new Matrix4F(
                             2.0f / (right - left), 0.0f, 0.0f, 0.0f,
                             0.0f, 2.0f / (top - bottom), 0.0f, 0.0f,
                             0.0f, 0.0f, -2.0f / (zFar - zNear), 0.0f,
                             tx, ty, tz, 1.0f
                             );

                }
                else
                {
                    float f = 1.0f / (float)Math.Tan(0.5f * Frustum.FovY);
                    float zNear = Frustum.Near;
                    float zFar = Frustum.Far;

                    projection = new Matrix4F(
                    f / m_aspectRatio, 0.0f, 0.0f, 0.0f,
                    0.0f, f, 0.0f, 0.0f,
                    0.0f, 0.0f, (zFar + zNear) / (zNear - zFar), -1.0f,
                    0.0f, 0.0f, (2 * zFar * zNear) / (zNear - zFar), 0.0f
                    );
                }

                return projection;
            }
        }

        /// <summary>
        /// Gets the view matrix</summary>
        public Matrix4F ViewMatrix
        {
            get
            {
                // concatenate translation
                float tx = -Vec3F.Dot(m_right, m_eye);
                float ty = -Vec3F.Dot(m_up, m_eye);
                float tz = Vec3F.Dot(m_lookAt, m_eye);

                // set matrix
                Matrix4F result = new Matrix4F(
                    m_right.X, m_up.X, -m_lookAt.X, 0,
                    m_right.Y, m_up.Y, -m_lookAt.Y, 0,
                    m_right.Z, m_up.Z, -m_lookAt.Z, 0,
                    tx, ty, tz, 1
                );

                // adjust handedness
                result.Mul(AxisSystem, result);

                return result;
            }
        }

        /// <summary>
        /// Gets the eye point in "world view" space, which is world space times AxisSystem</summary>
        /// <remarks>To set the eye point, call Set(Vec3F eye).</remarks>
        public Vec3F Eye
        {
            get { return m_eye; }
        }

        /// <summary>
        /// Gets the eye point in world space</summary>
        /// <remarks>Useful for back-face culling or creating rays in world coordinates.
        /// To set the eye point, call Set(Vec3F eye).</remarks>
        public Vec3F WorldEye
        {
            get
            {
                Vec3F worldEye;
                InverseAxisSystem.Transform(m_eye, out worldEye);
                return worldEye;
            }
        }

        /// <summary>
        /// Gets the camera look-at point in "world view" space, which is world space times AxisSystem</summary>
        public Vec3F LookAtPoint
        {
            get { return m_lookAtPoint; }
        }

        /// <summary>
        /// Gets the camera look-at point in world space</summary>
        public Vec3F WorldLookAtPoint
        {
            get
            {
                Vec3F worldLookAtPoint;
                InverseAxisSystem.Transform(m_lookAtPoint, out worldLookAtPoint);
                return worldLookAtPoint;
            }
        }

        /// <summary>
        /// Gets the camera look-at direction, a unit vector pointing in the direction from the eye
        /// to the look-at point, in "world view" space</summary>
        public Vec3F LookAt
        {
            get { return m_lookAt; }
        }

        /// <summary>
        /// Gets the camera look-at direction, a unit vector pointing in the direction from the eye
        /// to the look-at point, in world space</summary>
        public Vec3F WorldLookAt
        {
            get
            {
                Vec3F worldLookAt;
                InverseAxisSystem.TransformVector(m_lookAt, out worldLookAt);
                return worldLookAt;
            }
        }

        /// <summary>
        /// Gets the camera up direction, in "world view" space</summary>
        public Vec3F Up
        {
            get { return m_up; }
        }

        /// <summary>
        /// Gets the camera up direction, in world space</summary>
        public Vec3F WorldUp
        {
            get
            {
                Vec3F worldUp;
                InverseAxisSystem.TransformVector(m_up, out worldUp);
                return worldUp;
            }
        }

        /// <summary>
        /// Gets the camera right direction, in "world view" space</summary>
        /// <remarks>No setter; use the Set() methods instead</remarks>
        public Vec3F Right
        {
            get { return m_right; }
        }

        /// <summary>
        /// Gets the camera right direction, in world space</summary>
        public Vec3F WorldRight
        {
            get
            {
                Vec3F worldRight;
                InverseAxisSystem.TransformVector(m_right, out worldRight);
                return worldRight;
            }
        }

        /// <summary>
        /// Gets distance from eye point to lookAt point</summary>
        public float DistanceFromLookAt
        {
            get { return m_lookAtDistance; }
        }

        /// <summary>
        /// Gets and sets the focus radius around the "look at" point</summary>
        public float FocusRadius
        {
            get { return m_focusRadius; }
            set { m_focusRadius = value; }
        }

        /// <summary>
        /// Gets and sets the far clipping plane</summary>
        public float FarZ
        {
            get { return m_frustum.FarZ; }
            set
            {
                if (value <= 0)
                    throw new ArgumentOutOfRangeException();

                m_frustum.FarZ = value;

                OnCameraChanged(EventArgs.Empty);
            }
        }

        /// <summary>
        /// Gets the current near z value for the current projection type</summary>
        public float NearZ
        {
            get { return m_frustum.NearZ; }
        }

        /// <summary>
        /// Gets and sets the near z for orthographic (e.g., "top") modes. Is normally a large
        /// negative number so that foreground objects don't suddenly disappear when zooming in.</summary>
        public float OrthographicNearZ
        {
            get { return m_orthographicNearZ; }
            set
            {
                m_orthographicNearZ = value;
                if (m_projectionType != ProjectionType.Perspective)
                {
                    m_frustum.NearZ = value;
                    OnCameraChanged(EventArgs.Empty);
                }
            }
        }

        /// <summary>
        /// Gets or sets the near z value for the perspective view mode. Can't be less than or
        /// equal to zero so that divide-by-zero doesn't occur.</summary>
        public float PerspectiveNearZ
        {
            get { return m_perspectiveNearZ; }
            set
            {
                if (value <= 0)
                    throw new ArgumentOutOfRangeException();

                m_perspectiveNearZ = value;
                if (m_projectionType == ProjectionType.Perspective)
                {
                    m_frustum.NearZ = value;
                    OnCameraChanged(EventArgs.Empty);
                }
            }
        }

        /// <summary>
        /// Gets the y-field-of-view in radians</summary>
        public float YFov
        {
            get { return m_yFov; }
        }
       
        /// <summary>
        /// Gets or sets the aspect ratio, the ratio of width to height of the viewport</summary>
        public float Aspect
        {
            get { return m_aspectRatio; }
            set
            {
                if (value < 0.0f)
                    throw new ArgumentOutOfRangeException();

                m_aspectRatio = value;
                if (m_projectionType == ProjectionType.Orthographic)
                {
                    float d = m_frustum.Top - m_frustum.Bottom;
                    m_frustum.SetOrtho(
                        d * m_aspectRatio / 2,
                        -d * m_aspectRatio / 2,
                        d / 2,
                        -d / 2,
                        m_frustum.NearZ,
                        m_frustum.FarZ);
                }
                else
                {
                    m_frustum.SetPerspective(m_yFov, m_aspectRatio, m_frustum.NearZ, m_frustum.FarZ);
                }

                OnCameraChanged(EventArgs.Empty);
            }
        }

        /// <summary>
        /// Sets camera to perspective projection. Be sure to set the ViewType property as well</summary>
        /// <param name="yFov">Y field-of-view</param>
        /// <param name="aspectRatio">Aspect ratio of frustum cross section</param>
        /// <param name="nearZ">Near plane distance</param>
        /// <param name="farZ">Far plane distance</param>
        public void SetPerspective(float yFov, float aspectRatio, float nearZ, float farZ)
        {
            if (yFov <= 0 || aspectRatio <= 0 || nearZ <= 0 || farZ <= 0)
                throw new ArgumentOutOfRangeException();

            m_perspectiveNearZ = nearZ;
            m_projectionType = ProjectionType.Perspective;
            m_yFov = yFov;
            m_aspectRatio = aspectRatio;
            m_frustum.SetPerspective(yFov, aspectRatio, nearZ, farZ);

            OnCameraChanged(EventArgs.Empty);
        }

        /// <summary>
        /// Sets camera to orthographic projection. Be sure to set the ViewType property as well.</summary>
        /// <param name="right">Right plane constant</param>
        /// <param name="left">Left plane constant</param>
        /// <param name="top">Top plane constant</param>
        /// <param name="bottom">Bottom plane constant</param>
        /// <param name="near">Near plane constant</param>
        /// <param name="far">Far plane constant</param>
        public void SetOrthographic(float right, float left, float top, float bottom, float near, float far)
        {
            m_orthographicNearZ = near;
            m_frustum.SetOrtho(right, left, top, bottom, near, far);
            m_projectionType = ProjectionType.Orthographic;

            OnCameraChanged(EventArgs.Empty);
        }

        /// <summary>
        /// Sets camera to orthographic projection</summary>
        /// <param name="d">Distance from look at point</param>
        public void SetOrthographic(float d)
        {
            SetOrthographic(
                d * m_aspectRatio / 2,
                -d * m_aspectRatio / 2,
                d / 2,
                -d / 2,
                m_orthographicNearZ,
                m_frustum.FarZ);
        }

        /// <summary>
        /// Sets the camera eye point, lookAt point, and up vector</summary>
        /// <param name="eye">Eye point, in "world view" space</param>
        /// <param name="lookAtPoint">LookAt point, in "world view" space</param>
        /// <param name="up">Up vector, in "world view" space</param>
        public void Set(Vec3F eye, Vec3F lookAtPoint, Vec3F up)
        {
            m_eye = eye;
            m_lookAtPoint = lookAtPoint;
            m_up = up;

            UpdateGeometry();
        }

        /// <summary>
        /// Moves the camera to the new eye point, otherwise maintaining the same
        /// viewing geometry</summary>
        /// <param name="eye">New eye point, in "world view" space</param>
        public void Set(Vec3F eye)
        {
            // move the eye and look at points together
            Vec3F delta = eye - m_eye;
            m_eye = eye;
            m_lookAtPoint += delta;

            UpdateGeometry();
        }

        /// <summary>
        /// Positions and orients the camera so that the given sphere fills the view</summary>
        /// <param name="sphere">Sphere to zoom to, in world space</param>
        public void ZoomOnSphere(Sphere3F sphere)
        {
            float d = sphere.Radius;
            if (d == 0)
                d = 1;
            
            // avoid getting too far away or too close
            if (d > FarZ)
                d = FarZ;
            else if (d < NearZ)
                d = NearZ;

            m_focusRadius = d;

            // The eye and the look-at-point are in what might be called the "world view" system.
            // So, the world coordinate of the sphere center needs to be xformed by AxisSystem.
            Vec3F lookAt;
            Vec3F up;
            GetViewVectors(out lookAt, out up);

            Vec3F lookAtPoint;
            AxisSystem.Transform(sphere.Center, out lookAtPoint);

            Vec3F eye = lookAtPoint - lookAt * d;
            
            Set(eye, lookAtPoint, up);

            // Adjust near plane if we're in perspective mode. Otherwise, we should leave it alone
            //  because orthographic modes might need a large negative near-Z.
            if (ViewType == ViewTypes.Perspective)
            {
                float nearZ = Math.Abs(sphere.Radius) * 0.1f;
                nearZ = Math.Max(nearZ, 0.001f);
                PerspectiveNearZ = nearZ;
            }
        }

        /// <summary>
        /// Creates a ray originating at the given normalized window coordinates and pointing into
        /// the screen, along -Z axis. Normalized window coordinates are in the range [-0.5,0.5]
        /// with +x pointing to the right and +y pointing up.</summary>
        /// <param name="x">The x normalized window coordinate</param>
        /// <param name="y">The y normalized window coordinate</param>
        /// <returns>The ray</returns>
        public Ray3F CreateRay(float x, float y)
        {
            float height, width;

            // Setup ray
            Ray3F ray = new Ray3F();

            // World height on origin's z value
            if (Frustum.IsOrtho)
            {
                width = Frustum.Right - Frustum.Left;
                height = Frustum.Top - Frustum.Bottom;
                ray.Origin = new Vec3F(x * width, y * height, -NearZ);
                ray.Direction = new Vec3F(0, 0, -1);
            }
            else
            {
                height = Frustum.Far * (float)Math.Tan(Frustum.FovY / 2.0f) * 2.0f;
                width = Frustum.Far * (float)Math.Tan(Frustum.FovX / 2.0f) * 2.0f;

                ray.Origin = new Vec3F(0, 0, 0);
                ray.Direction = new Vec3F(x * width, y * height, -Frustum.Far);
                float l = ray.Direction.Length;
                ray.Direction = ray.Direction / l;
            }

            return ray;
        }

        /// <summary>
        /// Raises the CameraChanged event</summary>
        /// <param name="e">EventArgs for event</param>
        protected void OnCameraChanged(EventArgs e)
        {
            if (m_changingCamera)
                return;

            EventHandler h = CameraChanged;
            if (h != null)
                h(this, e);
        }

        private void UpdateGeometry()
        {
            // create orthornormal basis from lookAt and up vectors
            m_lookAt = m_lookAtPoint - m_eye;
            m_lookAt.Normalize();

            m_right = Vec3F.Cross(m_lookAt, m_up);
            m_right.Normalize();

            m_up = Vec3F.Cross(m_right, m_lookAt);
            //m_up.Normalize(); // m_right is already unit length

            m_lookAtDistance = Vec3F.Distance(m_eye, m_lookAtPoint);

            if (ProjectionType == ProjectionType.Orthographic)
                SetOrthographic(m_lookAtDistance);
            else
                OnCameraChanged(EventArgs.Empty);
        }

        private void GetViewVectors(out Vec3F lookAt, out Vec3F up)
        {
            switch (m_viewType)
            {
                case ViewTypes.Front:
                    lookAt = new Vec3F(0, 0, -1);
                    up = new Vec3F(0, 1, 0);
                    break;
                case ViewTypes.Back:
                    lookAt = new Vec3F(0, 0, 1);
                    up = new Vec3F(0, 1, 0);
                    break;
                case ViewTypes.Top:
                    lookAt = new Vec3F(0, -1, 0);
                    up = new Vec3F(0, 0, -1);
                    break;
                case ViewTypes.Bottom:
                    lookAt = new Vec3F(0, 1, 0);
                    up = new Vec3F(0, 0, -1);
                    break;
                case ViewTypes.Left:
                    lookAt = new Vec3F(1, 0, 0);
                    up = new Vec3F(0, 1, 0);
                    break;
                case ViewTypes.Right:
                    lookAt = new Vec3F(-1, 0, 0);
                    up = new Vec3F(0, 1, 0);
                    break;
                case ViewTypes.Perspective:
                default:
                    lookAt = m_lookAt;
                    up = m_up;
                    break;
            }
        }

        private ViewTypes m_viewType = ViewTypes.Perspective;

        private Vec3F m_eye;// in "world view" space, which is world space times AxisSystem.
        private Vec3F m_lookAtPoint;// in "world view" space, which is world space times AxisSystem.
        private Vec3F m_lookAt;//vector of unit length, in the direction from the eye to the look-at point.
        private Vec3F m_up;//unit vector, in "world view", along the "up" view direction.
        private Vec3F m_right;//unit vector, in "world view", along the "right" view direction.
        private float m_lookAtDistance;
        private float m_focusRadius = 1;
        private Matrix4F m_axisSystem = new Matrix4F();

        private float m_aspectRatio;
        private float m_yFov;
        private float m_perspectiveNearZ = 0.01f, m_orthographicNearZ = -10000.0f;
        private ProjectionType m_projectionType;
        private readonly Frustum m_frustum = new Frustum();

        // Set to 'true' and then restore to 'false' after a batch of changes are made to avoid
        // creating unnecessary camera events.
        private bool m_changingCamera;
    }
}
