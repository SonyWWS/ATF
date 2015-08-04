// -------------------------------------------------------------------------------------------------------------------
// Generated code, do not edit
// Command Line:  DomGen "obj.xsd" "Schema.cs" "http://www.fileformat.info/format/wavefrontobj" "Sce.Atf.Obj"
// -------------------------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;

using Sce.Atf.Dom;

namespace Sce.Atf.Obj
{
    public static class Schema
    {
        public const string NS = "http://www.fileformat.info/format/wavefrontobj";

        public static void Initialize(XmlSchemaTypeCollection typeCollection)
        {
            Initialize((ns,name)=>typeCollection.GetNodeType(ns,name),
                (ns,name)=>typeCollection.GetRootElement(ns,name));
        }

        public static void Initialize(IDictionary<string, XmlSchemaTypeCollection> typeCollections)
        {
            Initialize((ns,name)=>typeCollections[ns].GetNodeType(name),
                (ns,name)=>typeCollections[ns].GetRootElement(name));
        }

        private static void Initialize(Func<string, string, DomNodeType> getNodeType, Func<string, string, ChildInfo> getRootElement)
        {
            meshType.Type = getNodeType("http://www.fileformat.info/format/wavefrontobj", "meshType");
            meshType.boundingBoxAttribute = meshType.Type.GetAttributeInfo("boundingBox");
            meshType.nameAttribute = meshType.Type.GetAttributeInfo("name");
            meshType.vertexArrayChild = meshType.Type.GetChildInfo("vertexArray");

            meshType_vertexArray.Type = getNodeType("http://www.fileformat.info/format/wavefrontobj", "meshType_vertexArray");
            meshType_vertexArray.primitivesChild = meshType_vertexArray.Type.GetChildInfo("primitives");
            meshType_vertexArray.arrayChild = meshType_vertexArray.Type.GetChildInfo("array");

            vertexArray_primitives.Type = getNodeType("http://www.fileformat.info/format/wavefrontobj", "vertexArray_primitives");
            vertexArray_primitives.indicesAttribute = vertexArray_primitives.Type.GetAttributeInfo("indices");
            vertexArray_primitives.sizesAttribute = vertexArray_primitives.Type.GetAttributeInfo("sizes");
            vertexArray_primitives.nameAttribute = vertexArray_primitives.Type.GetAttributeInfo("name");
            vertexArray_primitives.typeAttribute = vertexArray_primitives.Type.GetAttributeInfo("type");
            vertexArray_primitives.bindingChild = vertexArray_primitives.Type.GetChildInfo("binding");
            vertexArray_primitives.shaderChild = vertexArray_primitives.Type.GetChildInfo("shader");

            primitives_binding.Type = getNodeType("http://www.fileformat.info/format/wavefrontobj", "primitives_binding");
            primitives_binding.sourceAttribute = primitives_binding.Type.GetAttributeInfo("source");

            shaderType.Type = getNodeType("http://www.fileformat.info/format/wavefrontobj", "shaderType");
            shaderType.nameAttribute = shaderType.Type.GetAttributeInfo("name");
            shaderType.ambientAttribute = shaderType.Type.GetAttributeInfo("ambient");
            shaderType.diffuseAttribute = shaderType.Type.GetAttributeInfo("diffuse");
            shaderType.shininessAttribute = shaderType.Type.GetAttributeInfo("shininess");
            shaderType.specularAttribute = shaderType.Type.GetAttributeInfo("specular");
            shaderType.textureAttribute = shaderType.Type.GetAttributeInfo("texture");

            vertexArray_array.Type = getNodeType("http://www.fileformat.info/format/wavefrontobj", "vertexArray_array");
            vertexArray_array.Attribute = vertexArray_array.Type.GetAttributeInfo("");
            vertexArray_array.countAttribute = vertexArray_array.Type.GetAttributeInfo("count");
            vertexArray_array.nameAttribute = vertexArray_array.Type.GetAttributeInfo("name");
            vertexArray_array.strideAttribute = vertexArray_array.Type.GetAttributeInfo("stride");

            nodeType.Type = getNodeType("http://www.fileformat.info/format/wavefrontobj", "nodeType");
            nodeType.boundingBoxAttribute = nodeType.Type.GetAttributeInfo("boundingBox");
            nodeType.transformAttribute = nodeType.Type.GetAttributeInfo("transform");
            nodeType.nameAttribute = nodeType.Type.GetAttributeInfo("name");
            nodeType.meshChild = nodeType.Type.GetChildInfo("mesh");
            nodeType.shaderChild = nodeType.Type.GetChildInfo("shader");

        }

        public static class meshType
        {
            public static DomNodeType Type;
            public static AttributeInfo boundingBoxAttribute;
            public static AttributeInfo nameAttribute;
            public static ChildInfo vertexArrayChild;
        }

        public static class meshType_vertexArray
        {
            public static DomNodeType Type;
            public static ChildInfo primitivesChild;
            public static ChildInfo arrayChild;
        }

        public static class vertexArray_primitives
        {
            public static DomNodeType Type;
            public static AttributeInfo indicesAttribute;
            public static AttributeInfo sizesAttribute;
            public static AttributeInfo nameAttribute;
            public static AttributeInfo typeAttribute;
            public static ChildInfo bindingChild;
            public static ChildInfo shaderChild;
        }

        public static class primitives_binding
        {
            public static DomNodeType Type;
            public static AttributeInfo sourceAttribute;
        }

        public static class shaderType
        {
            public static DomNodeType Type;
            public static AttributeInfo nameAttribute;
            public static AttributeInfo ambientAttribute;
            public static AttributeInfo diffuseAttribute;
            public static AttributeInfo shininessAttribute;
            public static AttributeInfo specularAttribute;
            public static AttributeInfo textureAttribute;
        }

        public static class vertexArray_array
        {
            public static DomNodeType Type;
            public static AttributeInfo Attribute;
            public static AttributeInfo countAttribute;
            public static AttributeInfo nameAttribute;
            public static AttributeInfo strideAttribute;
        }

        public static class nodeType
        {
            public static DomNodeType Type;
            public static AttributeInfo boundingBoxAttribute;
            public static AttributeInfo transformAttribute;
            public static AttributeInfo nameAttribute;
            public static ChildInfo meshChild;
            public static ChildInfo shaderChild;
        }
    }
}
