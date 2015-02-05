//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.ComponentModel.Composition;

using Sce.Atf;
using Sce.Atf.Adaptation;
using Sce.Atf.Applications;
using Sce.Atf.Rendering.Dom;
using Sce.Atf.Rendering.OpenGL;
using Sce.Atf.VectorMath;

namespace ModelViewerSample
{

    /// <summary>
    /// MEF component that registers a Windows Control with IControlHostService that is used to
    /// display a 3D scene. Builds the 3D scene from the active document. Consider also using
    /// RenderCommands.</summary>
    [Export(typeof(IInitializable))]
    [Export(typeof(RenderView))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class RenderView : IInitializable
    {
        /// <summary>
        /// Construct render view</summary>
        public RenderView()
        {
            m_scene = new Scene();
            m_designControl = new DesignControl(m_scene);
        }

        /// <summary>
        /// Gets DesignControl</summary>
        public DesignControl ViewControl
        {
            get { return m_designControl; }
        }

        /// <summary>
        /// Fits current 3D scene</summary>
        public void Fit()
        {
            // calculate bounding sphere
            if (m_scene.Children.Count > 0)
            {
                // calculate bounding sphere
                Sphere3F sphere = CalcBoundSphere(m_scene.Children[0]);
                sphere.Radius *= 2.0f;

                float aspect = (float)m_designControl.Width / (float)m_designControl.Height;

                m_designControl.Camera.Frustum.SetPerspective(
                    (float)Math.PI / 4,
                    aspect,
                    sphere.Radius * 0.01f,
                    sphere.Radius * 5.0f);

                m_designControl.Camera.ZoomOnSphere(sphere);   
            }

        }
        #region IInitializable Members

        /// <summary>
        /// Registers rendering control and subscribes to ActiveDocumentChanged event</summary>
        void IInitializable.Initialize()
        {
            ControlInfo cinfo = new ControlInfo("3D View", "3d viewer", StandardControlGroup.CenterPermanent);
            m_controlHostService.RegisterControl(m_designControl, cinfo, null);

            m_documentRegistry.ActiveDocumentChanged += (sender, e) =>
            {
                ClearRenderGraph(m_context);
                m_context = null;

                ModelDocument doc = m_documentRegistry.GetActiveDocument<ModelDocument>();
                if (doc != null)
                {
                    m_context = doc.RootNode;
                    SceneGraphBuilder builder = new SceneGraphBuilder(typeof(IRenderThumbnail));
                    builder.Build(doc.RootNode, m_scene);
                    Fit();                                                    
                }
            };

        }

        #endregion
        
        private void ClearRenderGraph(object  context)
        {            
            if (context != null)
            {
                VboManager.TheInstance.ReleaseVboList(context);
                Global<TextureManager>.Instance.DestroyTextures(context);
            }         
            Global<TextureManager>.Instance.DestroyTextures();            
            // Remove all render graph resources
            m_scene.ClearSubGraph();
        }
        private Sphere3F CalcBoundSphere(SceneNode root)
        {
            Sphere3F sphere = Sphere3F.Empty;

            foreach (SceneNode node in root.Children)
            {
                IBoundable boundable = node.DomNode.As<IBoundable>();
                Sphere3F localSphere = new Sphere3F();

                if (boundable != null)
                {
                    localSphere.Extend(boundable.BoundingBox);
                }

                if (localSphere.Radius > 0)
                {
                    sphere.Extend(localSphere);
                }
            }

            return sphere;
        }
        
        private object m_context;

        [Import(AllowDefault = false)]
        private IDocumentRegistry m_documentRegistry;

        [Import(AllowDefault = false)]
        private IControlHostService m_controlHostService;

        private Scene m_scene;        
        private DesignControl m_designControl;
    }
}
