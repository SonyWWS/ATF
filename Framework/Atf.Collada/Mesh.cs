//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.Collections.Generic;

using Sce.Atf.Adaptation;
using Sce.Atf.Dom;
using Sce.Atf.Rendering;
using Sce.Atf.VectorMath;


namespace Sce.Atf.Collada 
{
    /// <summary>
    /// COLLADA mesh</summary>
    public class Mesh : DomNodeAdapter, IMesh, ISceneGraphHierarchy
    {

        #region IMesh Members

        /// <summary>
        /// Gets the DataSet list</summary>
        public IEnumerable<IDataSet> DataSets
        {      
            get { return m_inputs.AsIEnumerable<IDataSet>(); }
        }

        /// <summary>
        /// Gets the PrimitiveSets list</summary>
        public IEnumerable<IPrimitiveSet> PrimitiveSets
        {
            get { return GetChildren().AsIEnumerable<IPrimitiveSet>(); }
        }
        
        #endregion
        /// <summary>
        /// Gets or sets mesh name</summary>
        public string Name
        {
            get { return "Collada-Mesh"; }
            set { }
        }
        #region IBoundable Members

        /// <summary>
        /// Gets a bounding box in local space</summary>
        public Box BoundingBox
        {            
            get { return m_boundingBox;}
        }

        #endregion

        /// <summary>
        /// Gets vertex inputs</summary>
        public IEnumerable<PrimInput> VertexInputs
        {
            get { return m_inputs; }
        }
        /// <summary>
        /// Gets sources</summary>
        public IEnumerable<Source> Sources
        {
            get { return GetChildList<Source>(Schema.mesh.sourceChild); }
        }

        /// <summary>
        /// Performs initialization when the adapter's node is set.
        /// This method is called each time the adapter is connected to its underlying node.
        /// Typically overridden by creators of DOM adapters.</summary>
        protected override void OnNodeSet()
        {
            base.OnNodeSet();

            DomNode vertexChild = this.DomNode.GetChild(Schema.mesh.verticesChild);
            foreach (DomNode dome in vertexChild.GetChildList(Schema.vertices.inputChild))
            {
                PrimInput input = new PrimInput(dome, this);
                m_inputs.Add(input);
            }

            m_boundingBox = CalculateBoundingBox();
        }

        private List<PrimInput> m_inputs = new List<PrimInput>();

        private Box CalculateBoundingBox()
        {            
            var box = new Box();
            foreach (PrimInput input in m_inputs)
            {
                if (input.Semantic == "POSITION")
                {
                    box.Extend(input.Data);
                    break;
                }
            }
            return box;
        }

        private Box m_boundingBox;

        #region ISceneGraphHierarchy Members

        /// <summary>
        /// Gets enumeration of the children of the object</summary>
        /// <returns>Enumeration of the children of the object</returns>
        public IEnumerable<object> GetChildren()
        {
            foreach (DomNode dom in DomNode.GetChildList(Schema.mesh.trianglesChild))
            {
                yield return dom;
            }

            foreach (DomNode dom in DomNode.GetChildList(Schema.mesh.tristripsChild))
            {
                yield return dom;
            }

            foreach (DomNode dom in DomNode.GetChildList(Schema.mesh.trifansChild))
            {
                yield return dom;
            }

            foreach (DomNode dom in DomNode.GetChildList(Schema.mesh.polylistChild))
            {
                yield return dom;
            }
        }
        #endregion      
    }
}
