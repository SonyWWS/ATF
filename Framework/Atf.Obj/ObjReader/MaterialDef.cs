//Sony Computer Entertainment Confidential

using Sce.Atf.VectorMath;

namespace Sce.Atf.Obj
{
    /// <summary>
    /// Material definition</summary>
    public class MaterialDef
    {
        /// <summary>
        /// Constructor</summary>
        /// <param name="name">Material name</param>
        public MaterialDef(string name)
        {
            Name = name;
        }

        /// <summary>
        /// Gets or sets material's alpha channel</summary>
        public float Alpha { get; set; }
        /// <summary>
        /// Gets or sets material's ambient lighting</summary>
        public Vec4F Ambient { get; set; }
        /// <summary>
        /// Gets or sets material's diffuse lighting</summary>
        public Vec4F Diffuse { get; set; }
        /// <summary>
        /// Gets or sets material's specular highlighting</summary>
        public Vec4F Specular { get; set; }
        /// <summary>
        /// Gets or sets material's shininess</summary>
        public float Shininess { get; set; }

        /// <summary>
        /// Gets or sets material's name</summary>
        public string Name { get; set; }
        /// <summary>
        /// Gets or sets material's texture name</summary>
        public string TextureName { get; set; }
    }
}
