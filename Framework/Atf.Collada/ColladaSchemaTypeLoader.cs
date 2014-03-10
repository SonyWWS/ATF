//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.Xml.Schema;

using Sce.Atf.Dom;

namespace Sce.Atf.Collada
{
    /// <summary>
    /// XML COLLADA schema loader that creates DomNodeTypes, parses annotations, and creates attribute rules</summary>
    class ColladaSchemaTypeLoader : XmlSchemaTypeLoader
    {
        /// <summary>
        /// Gets or sets schema loader namespace</summary>
        public string Namespace { get; private set; }

        /// <summary>
        /// Method called after the schema set has been loaded and the DomNodeTypes have been created, but
        /// before the DomNodeTypes have been frozen. This means that DomNodeType.SetIdAttribute, for example, has
        /// not been called on the DomNodeTypes. Is called shortly before OnDomNodeTypesFrozen.</summary>
        /// <param name="schemaSet">XML schema sets being loaded</param>
        protected override void OnSchemaSetLoaded(XmlSchemaSet schemaSet)
        {
            foreach (XmlSchemaTypeCollection typeCollection in GetTypeCollections())
            {
                if (typeCollection.TargetNamespace.EndsWith("COLLADASchema"))
                {
                    Namespace = typeCollection.TargetNamespace;
                    Schema.Initialize(typeCollection);

                    foreach (DomNodeType nodeType in typeCollection.GetNodeTypes())
                        nodeType.SetIdAttribute("id");

                    // Register base interfaces
                    Schema.animation.Type.Define(new ExtensionInfo<Animation>());
                    Schema.animation_clip.Type.Define(new ExtensionInfo<AnimationClip>());
                    Schema.channel.Type.Define(new ExtensionInfo<AnimationChannel>());
                    Schema.COLLADA.Type.Define(new ExtensionInfo<Collada>());
                    Schema.COLLADA.Type.Define(new ExtensionInfo<DomResource>());
                    Schema.COLLADA_scene.Type.Define(new ExtensionInfo<Scene>());
                    Schema.effect.Type.Define(new ExtensionInfo<Effect>());
                    Schema.geometry.Type.Define(new ExtensionInfo<Geometry>());
                    Schema.instance_controller.Type.Define(new ExtensionInfo<InstanceController>());
                    Schema.instance_geometry.Type.Define(new ExtensionInfo<InstanceGeometry>());                    
                    Schema.mesh.Type.Define(new ExtensionInfo<Mesh>());
                    Schema.node.Type.Define(new ExtensionInfo<Node>());
                    Schema.sampler.Type.Define(new ExtensionInfo<AnimationSampler>());
                    Schema.polylist.Type.Define(new ExtensionInfo<SubMesh>());
                    Schema.source.Type.Define(new ExtensionInfo<Source>());
                    Schema.triangles.Type.Define(new ExtensionInfo<SubMesh>());
                    Schema.trifans.Type.Define(new ExtensionInfo<SubMesh>());
                    Schema.tristrips.Type.Define(new ExtensionInfo<SubMesh>());
                    Schema.visual_scene.Type.Define(new ExtensionInfo<VisualScene>());
                   
                    break;
                }
            }
        }
    }
}
