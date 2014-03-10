//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using Sce.Atf.Adaptation;

namespace Sce.Atf.Rendering
{
    /// <summary>
    /// Interface for DataSet</summary>
    public interface IDataSet : IAdaptable, INameable
    {
        /// <summary>
        /// Gets and sets the data array</summary>
        float[] Data
        {
            get;
            set;
        }

        /// <summary>
        /// Gets and sets the data element size</summary>
        int ElementSize
        {
            get;
            set;
        }
    }
}
