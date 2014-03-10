//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;

namespace Sce.Atf.Rendering
{
    /// <summary>
    /// Enumeration for interpolation types</summary>
    [Flags]
    public enum AnimDataInterpType
    {
        /// <summary>
        /// Timestep with values only</summary>
        kTimeStep,
        
        /// <summary>
        /// Linear interpolation with key times</summary>
        kLinear,

        /// <summary>
        /// Flat interpolation with key times</summary>
        kFlat,

        /// <summary>
        /// Smooth interpolation with key times</summary>
        kSmooth,

        /// <summary>
        /// Single tangent interpolation with key times</summary>
        kTangent,

        /// <summary>
        /// Single weighted tangent interpolation with key times</summary>
        kWTangent,

        /// <summary>
        /// Two tangent interpolation with key times</summary>
        kSplitTangent,

        /// <summary>
        /// Two weighted tangent interpolation with key times</summary>
        kWSplitTangent,

        /// <summary>
        /// Fitted cubic curve segments</summary>
        kATGCurve,

        /// <summary>
        /// Fitted Hermite curve segments</summary>
        kATGHermite,

        /// <summary>
        /// Used to keep tags on how many interpolation types there are</summary>
        kNumInterpTypes
    };


    /// <summary>
    /// Interface for AnimChannel</summary>
    public interface IAnimData //: IDomNodeInterface
    {
        /// <summary>
        /// Gets or sets the number of keys</summary>
        int NumKeys
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the key stride</summary>
        int KeyStride
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the time offset</summary>
        float TimeOffset
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the duration</summary>
        float Duration
        {
            get;
            set;
        }
        
        /// <summary>
        /// Gets or sets the child AnimData list</summary>
        float[] KeyValues
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the key time array</summary>
        float[] KeyTimes
        {
            get;
            set;
        }

        /// <summary>
        /// Get or sets the tangent array</summary>
        float[] Tangents
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the interpolation type string</summary>
        string InterpolationType
        {
            get;
            set;
        }
    }

    /// <summary>
    /// Interface for animation channel</summary>
    public interface IAnimChannel //: IDomNodeInterface
    {
        /// <summary>
        /// Gets the child AnimData list</summary>
        IAnimData Data
        {
            get;
        }

        /// <summary>
        /// Gets or sets the channel name</summary>
        string Channel
        {
            get;
            set;
        }

        /// <summary>
        /// Gets the input object name</summary>
        string InputObject
        {
            get;
        }

        /// <summary>
        /// Gets the input channel name</summary>
        string InputChannel
        {
            get;
        }

        /// <summary>
        /// Gets or sets the target DOM object</summary>
        object Target
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the value index</summary>
        int ValueIndex
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets this animation to be enabled or disabled</summary>
        bool Enabled
        {
            get;
            set;
        }
    }
}
