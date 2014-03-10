//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

namespace Sce.Atf.VectorMath
{
    /// <summary>
    /// Defines maximum Euler angles and the axes (x, y, and/or z) that the limits
    /// apply to</summary>
    public struct EulerAngleLimits
    {
        /// <summary>
        /// Maximum angle allowed, in radians, if no limit is specified on a particular
        /// axis. Is PI radians.</summary>
        public const float MaxAngle = (float)System.Math.PI;
        
        /// <summary>
        /// Constructor</summary>
        /// <param name="angleLimits">Maximum angle values in radians</param>
        /// <param name="channels">Axes (x, y, and/or z) of 'angleLimits' that are valid</param>
        public EulerAngleLimits(Vec3F angleLimits, EulerAngleChannels channels)
        {
            m_angleLimits = angleLimits;
            m_channels = channels;
        }

        /// <summary>
        /// Constructor</summary>
        /// <param name="angles">Array of 3 floats containing the maximum angle values in radians</param>
        /// <param name="channels">Axes (x, y, and/or z) of 'angles' that are valid</param>
        public EulerAngleLimits(float[] angles, EulerAngleChannels channels)
        {
            m_angleLimits = new Vec3F(angles);
            m_channels = channels;
        }

        /// <summary>
        /// Gets or sets maximum angle values in radians</summary>
        public Vec3F Angles
        {
            get { return m_angleLimits; }
            set { m_angleLimits = value; }
        }

        /// <summary>
        /// Gets or sets axes (x, y, and/or z) that the limits apply to</summary>
        public EulerAngleChannels Channels
        {
            get { return m_channels; }
            set { m_channels = value; }
        }

        /// <summary>
        /// Gets or sets the x-axis maximum angle value. Gets MaxAngle if the axis is not
        /// specified as valid by the Channels property. Setting this property automatically
        /// sets the x-axis bit of the Channels property as being valid.</summary>
        public float X
        {
            get
            {
                if ((m_channels & EulerAngleChannels.X)!=0)
                    return m_angleLimits.X;
                
                return MaxAngle;
            }
            set
            {
                m_angleLimits.X = value;
                if (m_angleLimits.X>MaxAngle)
                    m_angleLimits.X=MaxAngle;
                    
                m_channels |= EulerAngleChannels.X;
            }
        }

        /// <summary>
        /// Gets or sets the y-axis maximum angle value. Gets MaxAngle if the axis is not
        /// specified as valid by the Channels property. Setting this property automatically
        /// sets the y-axis bit of the Channels property as being valid.</summary>
        public float Y
        {
            get
            {
                if ((m_channels & EulerAngleChannels.Y)!=0)
                    return m_angleLimits.Y;
                
                return MaxAngle;
            }
            set
            {
                m_angleLimits.Y = value;
                if (m_angleLimits.Y>MaxAngle)
                    m_angleLimits.Y=MaxAngle;
                    
                m_channels |= EulerAngleChannels.Y;
            }
        }

        /// <summary>
        /// Gets or sets the z-axis maximum angle value. Gets MaxAngle if the axis is not
        /// specified as valid by the Channels property. Setting this property automatically
        /// sets the z-axis bit of the Channels property as being valid.</summary>
        public float Z
        {
            get
            {
                if ((m_channels & EulerAngleChannels.Z)!=0)
                    return m_angleLimits.Z;
                
                return MaxAngle;
            }
            set
            {
                m_angleLimits.Z = value;
                if (m_angleLimits.Z>MaxAngle)
                    m_angleLimits.Z=MaxAngle;
                    
                m_channels |= EulerAngleChannels.Z;
            }
        }

        /// <summary>
        /// Gets whether or not the maximum x-axis angle value is valid</summary>
        public bool HasRotationX
        {
            get { return (m_channels & EulerAngleChannels.X) != 0; }
        }

        /// <summary>
        /// Gets whether or not the maximum y-axis angle value is valid</summary>
        public bool HasRotationY
        {
            get { return (m_channels & EulerAngleChannels.Y) != 0; }
        }

        /// <summary>
        /// Gets whether or not the maximum z-axis angle value is valid</summary>
        public bool HasRotationZ
        {
            get { return (m_channels & EulerAngleChannels.Z) != 0; }
        }

        private Vec3F m_angleLimits;
        private EulerAngleChannels m_channels;
    };
}
