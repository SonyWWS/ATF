//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.Collections.Generic;

using Sce.Atf.Dom;
using Sce.Atf.Rendering;
using Sce.Atf.VectorMath;

namespace Sce.Atf.Collada
{
    /// <summary>
    /// COLLADA geometry</summary>
    public class Geometry : DomNodeAdapter, ISceneGraphHierarchy
    {
        /// <summary>
        /// Gets or sets key/COLLADA Effect dictionary</summary>
        public Dictionary<string, Effect> Effects { get; set; }

        //#region IMesh Members

        //public IEnumerable<IDataSet> DataSets
        //{
        //    get { return MeshChild.DataSets; }
        //}

        //public IEnumerable<IPrimitiveSet> PrimitiveSets
        //{
        //    get { return MeshChild.PrimitiveSets; }
        //}

        //#endregion
             
        #region INameable Members

        /// <summary>
        /// Gets and sets DOM object's name</summary>
        public string Name
        {
            get { return GetAttribute<string>(Schema.geometry.nameAttribute); }
            set { }
        }

        #endregion

        #region ISceneGraphHierarchy Members

        /// <summary>
        /// Gets enumeration of the children of the object</summary>
        /// <returns>Enumeration of the children of the object</returns>
        public IEnumerable<object> GetChildren()
        {
            yield return this.DomNode.GetChild(Schema.geometry.meshChild);                       
        }

        #endregion


        #region IBoundable Members

        /// <summary>
        /// Gets a bounding box in local space</summary>
        public Box BoundingBox
        {
            get 
            {
                Mesh mesh = GetChild<Mesh>(Schema.geometry.meshChild);
                return mesh.BoundingBox;
            }
        }

        #endregion
    }
}
