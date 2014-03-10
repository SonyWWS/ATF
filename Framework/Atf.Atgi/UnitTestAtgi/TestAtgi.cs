//Sony Computer Entertainment Confidential

using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.IO;

using NUnit.Framework;

using Scea.Editors.Plugins;
using Scea.Collections;
using Scea.Dom;
using Scea.Pipelines.Geometry;
using Scea.Utilities;
using Sce.Atf;
using Sce.Atf.VectorMath;

namespace UnitTestAtgi
{
    [TestFixture]
    public class TestAtgi
    {
        private string m_appPath;
        private DomCollection m_domCollection = null;
       
        public TestAtgi()
        {
            PluginDictionary plugins = Singleton<PluginDictionary>.Instance;
            m_appPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            // Setup schema path
            string schemaPath = Path.Combine(m_appPath, "schemas");
            DomSchemaRegistry.SchemaResolver = new FileStreamResolver(schemaPath);

            // load atgi plugin
            string atgiPath = m_appPath + @"\Plugins\Scea.Atgi.pll";
            Assembly assembly = Assembly.LoadFrom(atgiPath);
            plugins.Add(assembly);
            plugins.GetPlugins<PluginBase>();


            // 2- create colleciton            
            DomRepository repository = new DomRepository();
            string atgiFile = m_appPath + @"\cube.atgi";
            m_domCollection = repository.ReadCollection(new Uri(atgiFile));
            m_domCollection.IsReadOnly = true;
            // add dc to DomRepository            
            repository.Add(m_domCollection);            
        }

        [Test(Description = "Can create IWorld on the root dom object")]
        public void RootObjectIsWorld()
        {
            IWorld world = m_domCollection.RootDomObject.CreateInterface<IWorld>();
            Assert.IsNotNull(world);
        }

        [Test(Description = "The World must have a Scene")]
        public void WorldMustHaveScene()
        {
            IWorld world = m_domCollection.RootDomObject.CreateInterface<IWorld>();
            IScene scene = world.Scene;
            Assert.IsNotNull(scene);
        }

        [Test(Description = "The Scene must only have five nodes")]
        public void NumberOfNodes()
        {
            IWorld world = m_domCollection.RootDomObject.CreateInterface<IWorld>();
            IScene scene = world.Scene;
            IList<INode> nodes = scene.Nodes;
            Assert.AreEqual(5,nodes.Count,"Invalid node count");
        }

        [Test(Description = "validate node names")]
        public void NodeNames()
        {
            IWorld world = m_domCollection.RootDomObject.CreateInterface<IWorld>();
            IScene scene = world.Scene;
            IList<INode> nodes = scene.Nodes;

            string n = "";
            foreach (INode node in nodes)
                n += node.Name + "  " + node.Transform.ToString() + " ";            
            bool test = n.Contains("persp") && n.Contains("top") && n.Contains("front") &&
                n.Contains("side") && n.Contains("pCube1");
            Assert.IsTrue(test);
        }

        [Test(Description = "All the transformation matrix must be Identity")]
        public void NodeTransformMatrix()
        {
            IWorld world = m_domCollection.RootDomObject.CreateInterface<IWorld>();
            IScene scene = world.Scene;
            IList<INode> nodes = scene.Nodes;

            foreach (INode node in nodes)
                Assert.IsTrue(node.Transform.Equals(Matrix4F.Identity));
        }

        [Test(Description = "Only pCube1 node have one mesh\r\n the rest of the nodes do not have mesh.")]
        public void NumberOfMeshes()
        {
            IWorld world = m_domCollection.RootDomObject.CreateInterface<IWorld>();
            IScene scene = world.Scene;
            IList<INode> nodes = scene.Nodes;
            foreach (INode node in nodes)
                if (node.Name.Contains("pCube1"))
                {
                    Assert.AreEqual(1,node.Meshes.Count);
                    break;
                }
                else
                    Assert.AreEqual(0,node.Meshes.Count);
        }

        [Test(Description = "there is no node hierarchy")]
        public void ChildNodes()
        {
            IWorld world = m_domCollection.RootDomObject.CreateInterface<IWorld>();
            IScene scene = world.Scene;
            IList<INode> nodes = scene.Nodes;
            foreach (INode node in nodes)
                Assert.AreEqual(0,node.ChildNodes.Count);
        }

        [Test(Description = "validating mesh for pCube1 node")]
        public void DataSet()
        {
            IWorld world = m_domCollection.RootDomObject.CreateInterface<IWorld>();
            IScene scene = world.Scene;
            IList<INode> nodes = scene.Nodes;
            IMesh mesh = null;
            foreach (INode node in nodes)
                if (node.Name.Contains("pCube1"))
                    mesh = node.Meshes[0];
            Assert.AreEqual(2,mesh.DataSets.Count);
            Assert.AreEqual(1, mesh.PrimitiveSets.Count);
            Assert.IsNull(mesh.BoundingBox,"bounding box must be null");
            
            
        }

        [Test(Description = "validate position data")]
        public void PositionData()
        {
            IWorld world = m_domCollection.RootDomObject.CreateInterface<IWorld>();
            IScene scene = world.Scene;
            IList<INode> nodes = scene.Nodes;
            IMesh mesh = null;
            foreach (INode node in nodes)
                if (node.Name.Contains("pCube1"))
                    mesh = node.Meshes[0];
            IList<IDataSet> datasets = mesh.DataSets;

            float[] data = new float[] {
                -0.005f, -0.005f, 0.005f, 0.005f, -0.005f, 0.005f, -0.005f, 0.005f,
                0.005f, 0.005f, 0.005f, 0.005f, -0.005f, 0.005f, -0.005f, 0.005f, 0.005f, -0.005f, 
                -0.005f, -0.005f, -0.005f, 0.005f, -0.005f, -0.005f};
            IDataSet position = datasets[0];
            Assert.AreEqual("position",position.Name );
            Assert.AreEqual(3,position.ElementSize);
            Assert.AreEqual(24,position.Data.Length);

            for (int i = 0; i < data.Length; i++)
                Assert.AreEqual(data[i],position.Data[i]);
        }

        [Test(Description = "validate normal data")]
        public void NormalData()
        {
            IWorld world = m_domCollection.RootDomObject.CreateInterface<IWorld>();
            IScene scene = world.Scene;
            IList<INode> nodes = scene.Nodes;
            IMesh mesh = null;
            foreach (INode node in nodes)
                if (node.Name.Contains("pCube1"))
                    mesh = node.Meshes[0];
            IList<IDataSet> datasets = mesh.DataSets;

            float[] data = new float[]{
                0f, 0f, 1f, 0f, 0f, 1f, 0f, 0f, 1f, 0f, 0f, 1f, 0f, 1f, 0f, 0f, 1f,
                0f, 0f, 1f, 0f, 0f, 1f, 0f, 0f, 0f, -1f, 0f, 0f, -1f, 0f, 0f, -1f, 0f, 0f, -1f,
                0f, -1f, 0f, 0f, -1f, 0f, 0f, -1f, 0f, 0f, -1f, 0f, 1f, 0f, 0f, 1f, 0f, 0f, 1f,
                0f, 0f, 1f, 0f, 0f, -1f, 0f, 0f, -1f, 0f, 0f, -1f, 0f, 0f, -1f, 0f, 0f};
            IDataSet normal = datasets[1];
            Assert.AreEqual("normal",normal.Name);
            Assert.AreEqual(72,normal.Data.Length);
            Assert.AreEqual(3,normal.ElementSize);
            for (int i = 0; i < data.Length; i++)
                Assert.AreEqual(data[i], normal.Data[i]);           
        }

        [Test(Description = "The mesh for pCube1 node must only have one primset")]
        public void NumberOfPrimSets()
        {
            
            IWorld world = m_domCollection.RootDomObject.CreateInterface<IWorld>();
            IScene scene = world.Scene;
            IList<INode> nodes = scene.Nodes;
            IMesh mesh = null;
            foreach (INode node in nodes)
                if (node.Name.Contains("pCube1"))
                    mesh = node.Meshes[0];

            IList<IPrimitiveSet> primSets = mesh.PrimitiveSets;
            Assert.AreEqual(1,primSets.Count);
        }

        [Test(Description = "Validating primset")]
        public void PrimSet()
        {
            IWorld world = m_domCollection.RootDomObject.CreateInterface<IWorld>();
            IScene scene = world.Scene;
            IList<INode> nodes = scene.Nodes;
            IMesh mesh = null;
            foreach (INode node in nodes)
                if (node.Name.Contains("pCube1"))
                    mesh = node.Meshes[0];
            IList<IPrimitiveSet> primSets = mesh.PrimitiveSets;
            IPrimitiveSet primSet = primSets[0];
            Assert.AreEqual(2, primSet.Bindings.Count);
            Assert.AreEqual(primSet.Bindings.Count, primSet.BindingCount);
            Assert.AreEqual(48,primSet.PrimitiveIndices.Length);
            Assert.AreEqual(6,primSet.PrimitiveSizes.Length);
            Assert.AreEqual("POLYGONS",primSet.PrimitiveType);
            Assert.IsNotNull(primSet.Shader,"Shader must not be null");
        }

        [Test(Description = "validating bindinings")]
        public void Bindings()
        {
            IWorld world = m_domCollection.RootDomObject.CreateInterface<IWorld>();
            IScene scene = world.Scene;
            IList<INode> nodes = scene.Nodes;
            IMesh mesh = null;
            foreach (INode node in nodes)
                if (node.Name.Contains("pCube1"))
                    mesh = node.Meshes[0];
            IList<IPrimitiveSet> primSets = mesh.PrimitiveSets;
            IPrimitiveSet primSet = primSets[0];
            IList<IBinding> bindings = primSet.Bindings;
            IBinding firstBinding = bindings[0];
            Assert.AreEqual("position",bindings[0].Source.GetAttribute("name").ToString());
            Assert.AreEqual("normal",bindings[1].Source.GetAttribute("name").ToString());
        }

        [Test(Description="Validating primset indexes")]
        public void PrimSetIndices()
        {
            IWorld world = m_domCollection.RootDomObject.CreateInterface<IWorld>();
            IScene scene = world.Scene;
            IList<INode> nodes = scene.Nodes;
            IMesh mesh = null;
            foreach (INode node in nodes)
                if (node.Name.Contains("pCube1"))
                    mesh = node.Meshes[0];
            IList<IPrimitiveSet> primSets = mesh.PrimitiveSets;
            IPrimitiveSet primSet = primSets[0];
            int[] indexes = primSet.PrimitiveIndices;
            int[] data = new int[] {
                0, 0, 1, 1, 3, 2, 2, 3, 2, 4, 3, 5, 5, 6, 4, 7, 4, 8, 5, 9, 7, 10,
                6, 11, 6, 12, 7, 13, 1, 14, 0, 15, 1, 16, 7, 17, 5, 18, 3, 19, 6, 20, 0, 21, 2,
                22, 4, 23};
            for (int i = 0; i < data.Length; i++)
                Assert.AreEqual(data[i], indexes[i]);
        }

        [Test(Description="Validating primitive sizes")]
        public void PrimSetSizes()
        {
            IWorld world = m_domCollection.RootDomObject.CreateInterface<IWorld>();
            IScene scene = world.Scene;
            IList<INode> nodes = scene.Nodes;
            IMesh mesh = null;
            foreach (INode node in nodes)
                if (node.Name.Contains("pCube1"))
                    mesh = node.Meshes[0];
            IList<IPrimitiveSet> primSets = mesh.PrimitiveSets;
            IPrimitiveSet primSet = primSets[0];
            int[] sizes = primSet.PrimitiveSizes;
            int[] data = new int[]{4, 4, 4, 4, 4, 4};
            for (int i = 0; i < data.Length; i++)
                Assert.AreEqual(data[i], sizes[i]);
            
        }

        [Test(Description="Validating shader")]
        public void Shader()
        {
            IWorld world = m_domCollection.RootDomObject.CreateInterface<IWorld>();
            IScene scene = world.Scene;
            IList<INode> nodes = scene.Nodes;
            IMesh mesh = null;
            foreach (INode node in nodes)
                if (node.Name.Contains("pCube1"))
                    mesh = node.Meshes[0];
            IList<IPrimitiveSet> primSets = mesh.PrimitiveSets;
            IPrimitiveSet primSet = primSets[0];
            IShader shader = primSet.Shader;
            IList<IBinding> binding = shader.Bindings;
            Assert.AreEqual(0, shader.Bindings.Count);
            Assert.IsNull(shader.CustomAttributes);            
        }

    }
}
