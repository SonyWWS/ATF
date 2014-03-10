//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.Collections.Generic;

namespace Sce.Atf.Rendering
{
    /// <summary>
    /// Interface for submesh extending IPrimitiveSet</summary>
    public interface ISubMesh : IPrimitiveSet
    {
        /// <summary>
        /// Gets the DataSet for the submesh</summary>
        IEnumerable<IDataSet> DataSets
        {
            get;
        }

        /// <summary>
        /// Gets primitive count</summary>
        int Count
        {
            get;            
        }

        /// <summary>
        /// Gets parent IMesh</summary>
        IMesh Parent
        {
            get;
        }        
    }
}
