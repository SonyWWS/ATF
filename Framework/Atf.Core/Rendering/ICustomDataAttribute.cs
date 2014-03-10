//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

namespace Sce.Atf.Rendering
{
    /// <summary>
    /// Types of a custom data attribute</summary>
    public enum CustomDataEnumType
    {
        /// <summary>
        /// String</summary>
        kString,
        /// <summary>
        /// Enumeration</summary>
        kEnum,
        /// <summary>
        /// Boolean</summary>
        kBool,
        /// <summary>
        /// Integer</summary>
        kInt,
        /// <summary>
        /// Float</summary>
        kFloat,
        /// <summary>
        /// Array of floats</summary>
        kFloatArray,
        /// <summary>
        /// 2D vector</summary>
        kVector2,
        /// <summary>
        /// 3D vector</summary>
        kVector3,
        /// <summary>
        /// Object link</summary>
        kObjLink,
    };

    /// <summary>
    /// Interface for ATGI CustomDataAttribute</summary>
    public interface ICustomDataAttribute : INameable
    {
        /// <summary>
        /// Gets or sets the data type name</summary>
        string DataType
        {
            get;
            set;
        }
        
        /// <summary>
        /// Gets or sets the value attribute name</summary>
        string ValueAttr
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the default</summary>
        string Default
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the minimum value</summary>
        string Min
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the maximum value</summary>
        string Max
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the count</summary>
        int Count
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the index</summary>
        int Index
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets whether value is an array</summary>
        bool isArray
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the value</summary>
        object Value
        {
            get;
            set;
        }
    }
}
