//Sony Computer Entertainment Confidential

using System.Collections.Generic;

namespace Sce.Atf.Obj
{
    /// <summary>
    /// Set of faces of material</summary>
    public class FaceSet
    {
        /// <summary>
        /// Constructor</summary>
        public FaceSet()
        {
            
        }

        /// <summary>
        /// Constructor with material name</summary>
        /// <param name="materialName">Material name</param>
        public FaceSet(string materialName)
        {
            MaterialName = materialName;
        }

        /// <summary>
        /// Gets or sets whether faces have normals</summary>
        public bool HasNormals { get; set; }
        /// <summary>
        /// Gets or sets whether material has texture coordinate data</summary>
        public bool HasTexCoords { get; set; }

        public List<int> Indices = new List<int>();
        /// <summary>
        /// Material name</summary>
        public string MaterialName = "default";
        public List<int> Sizes = new List<int>();
    }
}
