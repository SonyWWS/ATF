// -------------------------------------------------------------------------------------------------------------------
// Generated code, do not edit
// Command Line:  DomGen "Circuit.xsd" "Schema.cs" "http://sony.com/gametech/circuits/1_0" "CircuitEditorSample"
// -------------------------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;

using Sce.Atf.Dom;

namespace CircuitEditorSample
{
    public static class Schema
    {
        public const string NS = "http://sony.com/gametech/circuits/1_0";

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
            circuitDocumentType.Type = getNodeType("http://sony.com/gametech/circuits/1_0", "circuitDocumentType");
            circuitDocumentType.moduleChild = circuitDocumentType.Type.GetChildInfo("module");
            circuitDocumentType.connectionChild = circuitDocumentType.Type.GetChildInfo("connection");
            circuitDocumentType.layerFolderChild = circuitDocumentType.Type.GetChildInfo("layerFolder");
            circuitDocumentType.annotationChild = circuitDocumentType.Type.GetChildInfo("annotation");
            circuitDocumentType.subCircuitChild = circuitDocumentType.Type.GetChildInfo("subCircuit");
            circuitDocumentType.prototypeFolderChild = circuitDocumentType.Type.GetChildInfo("prototypeFolder");
            circuitDocumentType.templateFolderChild = circuitDocumentType.Type.GetChildInfo("templateFolder");

            circuitType.Type = getNodeType("http://sony.com/gametech/circuits/1_0", "circuitType");
            circuitType.moduleChild = circuitType.Type.GetChildInfo("module");
            circuitType.connectionChild = circuitType.Type.GetChildInfo("connection");
            circuitType.layerFolderChild = circuitType.Type.GetChildInfo("layerFolder");
            circuitType.annotationChild = circuitType.Type.GetChildInfo("annotation");

            moduleType.Type = getNodeType("http://sony.com/gametech/circuits/1_0", "moduleType");
            moduleType.nameAttribute = moduleType.Type.GetAttributeInfo("name");
            moduleType.labelAttribute = moduleType.Type.GetAttributeInfo("label");
            moduleType.xAttribute = moduleType.Type.GetAttributeInfo("x");
            moduleType.yAttribute = moduleType.Type.GetAttributeInfo("y");
            moduleType.visibleAttribute = moduleType.Type.GetAttributeInfo("visible");
            moduleType.sourceGuidAttribute = moduleType.Type.GetAttributeInfo("sourceGuid");

            connectionType.Type = getNodeType("http://sony.com/gametech/circuits/1_0", "connectionType");
            connectionType.labelAttribute = connectionType.Type.GetAttributeInfo("label");
            connectionType.outputModuleAttribute = connectionType.Type.GetAttributeInfo("outputModule");
            connectionType.outputPinAttribute = connectionType.Type.GetAttributeInfo("outputPin");
            connectionType.inputModuleAttribute = connectionType.Type.GetAttributeInfo("inputModule");
            connectionType.inputPinAttribute = connectionType.Type.GetAttributeInfo("inputPin");

            layerFolderType.Type = getNodeType("http://sony.com/gametech/circuits/1_0", "layerFolderType");
            layerFolderType.nameAttribute = layerFolderType.Type.GetAttributeInfo("name");
            layerFolderType.layerFolderChild = layerFolderType.Type.GetChildInfo("layerFolder");
            layerFolderType.moduleRefChild = layerFolderType.Type.GetChildInfo("moduleRef");

            moduleRefType.Type = getNodeType("http://sony.com/gametech/circuits/1_0", "moduleRefType");
            moduleRefType.refAttribute = moduleRefType.Type.GetAttributeInfo("ref");

            annotationType.Type = getNodeType("http://sony.com/gametech/circuits/1_0", "annotationType");
            annotationType.textAttribute = annotationType.Type.GetAttributeInfo("text");
            annotationType.xAttribute = annotationType.Type.GetAttributeInfo("x");
            annotationType.yAttribute = annotationType.Type.GetAttributeInfo("y");
            annotationType.widthAttribute = annotationType.Type.GetAttributeInfo("width");
            annotationType.heightAttribute = annotationType.Type.GetAttributeInfo("height");
            annotationType.backcolorAttribute = annotationType.Type.GetAttributeInfo("backcolor");

            subCircuitType.Type = getNodeType("http://sony.com/gametech/circuits/1_0", "subCircuitType");
            subCircuitType.nameAttribute = subCircuitType.Type.GetAttributeInfo("name");
            subCircuitType.labelAttribute = subCircuitType.Type.GetAttributeInfo("label");
            subCircuitType.moduleChild = subCircuitType.Type.GetChildInfo("module");
            subCircuitType.connectionChild = subCircuitType.Type.GetChildInfo("connection");
            subCircuitType.layerFolderChild = subCircuitType.Type.GetChildInfo("layerFolder");
            subCircuitType.annotationChild = subCircuitType.Type.GetChildInfo("annotation");
            subCircuitType.inputChild = subCircuitType.Type.GetChildInfo("input");
            subCircuitType.outputChild = subCircuitType.Type.GetChildInfo("output");

            pinType.Type = getNodeType("http://sony.com/gametech/circuits/1_0", "pinType");
            pinType.nameAttribute = pinType.Type.GetAttributeInfo("name");
            pinType.typeAttribute = pinType.Type.GetAttributeInfo("type");

            prototypeFolderType.Type = getNodeType("http://sony.com/gametech/circuits/1_0", "prototypeFolderType");
            prototypeFolderType.nameAttribute = prototypeFolderType.Type.GetAttributeInfo("name");
            prototypeFolderType.prototypeFolderChild = prototypeFolderType.Type.GetChildInfo("prototypeFolder");
            prototypeFolderType.prototypeChild = prototypeFolderType.Type.GetChildInfo("prototype");

            prototypeType.Type = getNodeType("http://sony.com/gametech/circuits/1_0", "prototypeType");
            prototypeType.nameAttribute = prototypeType.Type.GetAttributeInfo("name");
            prototypeType.moduleChild = prototypeType.Type.GetChildInfo("module");
            prototypeType.connectionChild = prototypeType.Type.GetChildInfo("connection");

            templateFolderType.Type = getNodeType("http://sony.com/gametech/circuits/1_0", "templateFolderType");
            templateFolderType.nameAttribute = templateFolderType.Type.GetAttributeInfo("name");
            templateFolderType.referenceFileAttribute = templateFolderType.Type.GetAttributeInfo("referenceFile");
            templateFolderType.templateFolderChild = templateFolderType.Type.GetChildInfo("templateFolder");
            templateFolderType.templateChild = templateFolderType.Type.GetChildInfo("template");

            templateType.Type = getNodeType("http://sony.com/gametech/circuits/1_0", "templateType");
            templateType.guidAttribute = templateType.Type.GetAttributeInfo("guid");
            templateType.labelAttribute = templateType.Type.GetAttributeInfo("label");
            templateType.moduleChild = templateType.Type.GetChildInfo("module");

            groupPinType.Type = getNodeType("http://sony.com/gametech/circuits/1_0", "groupPinType");
            groupPinType.nameAttribute = groupPinType.Type.GetAttributeInfo("name");
            groupPinType.typeAttribute = groupPinType.Type.GetAttributeInfo("type");
            groupPinType.moduleAttribute = groupPinType.Type.GetAttributeInfo("module");
            groupPinType.pinAttribute = groupPinType.Type.GetAttributeInfo("pin");
            groupPinType.visibleAttribute = groupPinType.Type.GetAttributeInfo("visible");
            groupPinType.indexAttribute = groupPinType.Type.GetAttributeInfo("index");
            groupPinType.pinnedAttribute = groupPinType.Type.GetAttributeInfo("pinned");
            groupPinType.pinYAttribute = groupPinType.Type.GetAttributeInfo("pinY");

            groupType.Type = getNodeType("http://sony.com/gametech/circuits/1_0", "groupType");
            groupType.nameAttribute = groupType.Type.GetAttributeInfo("name");
            groupType.labelAttribute = groupType.Type.GetAttributeInfo("label");
            groupType.xAttribute = groupType.Type.GetAttributeInfo("x");
            groupType.yAttribute = groupType.Type.GetAttributeInfo("y");
            groupType.visibleAttribute = groupType.Type.GetAttributeInfo("visible");
            groupType.sourceGuidAttribute = groupType.Type.GetAttributeInfo("sourceGuid");
            groupType.expandedAttribute = groupType.Type.GetAttributeInfo("expanded");
            groupType.showExpandedGroupPinsAttribute = groupType.Type.GetAttributeInfo("showExpandedGroupPins");
            groupType.autosizeAttribute = groupType.Type.GetAttributeInfo("autosize");
            groupType.widthAttribute = groupType.Type.GetAttributeInfo("width");
            groupType.heightAttribute = groupType.Type.GetAttributeInfo("height");
            groupType.minwidthAttribute = groupType.Type.GetAttributeInfo("minwidth");
            groupType.minheightAttribute = groupType.Type.GetAttributeInfo("minheight");
            groupType.inputChild = groupType.Type.GetChildInfo("input");
            groupType.outputChild = groupType.Type.GetChildInfo("output");
            groupType.moduleChild = groupType.Type.GetChildInfo("module");
            groupType.connectionChild = groupType.Type.GetChildInfo("connection");
            groupType.annotationChild = groupType.Type.GetChildInfo("annotation");

            subCircuitInstanceType.Type = getNodeType("http://sony.com/gametech/circuits/1_0", "subCircuitInstanceType");
            subCircuitInstanceType.nameAttribute = subCircuitInstanceType.Type.GetAttributeInfo("name");
            subCircuitInstanceType.labelAttribute = subCircuitInstanceType.Type.GetAttributeInfo("label");
            subCircuitInstanceType.xAttribute = subCircuitInstanceType.Type.GetAttributeInfo("x");
            subCircuitInstanceType.yAttribute = subCircuitInstanceType.Type.GetAttributeInfo("y");
            subCircuitInstanceType.visibleAttribute = subCircuitInstanceType.Type.GetAttributeInfo("visible");
            subCircuitInstanceType.sourceGuidAttribute = subCircuitInstanceType.Type.GetAttributeInfo("sourceGuid");
            subCircuitInstanceType.typeAttribute = subCircuitInstanceType.Type.GetAttributeInfo("type");

            missingTemplateType.Type = getNodeType("http://sony.com/gametech/circuits/1_0", "missingTemplateType");
            missingTemplateType.guidAttribute = missingTemplateType.Type.GetAttributeInfo("guid");
            missingTemplateType.labelAttribute = missingTemplateType.Type.GetAttributeInfo("label");
            missingTemplateType.moduleChild = missingTemplateType.Type.GetChildInfo("module");

            missingModuleType.Type = getNodeType("http://sony.com/gametech/circuits/1_0", "missingModuleType");
            missingModuleType.nameAttribute = missingModuleType.Type.GetAttributeInfo("name");
            missingModuleType.labelAttribute = missingModuleType.Type.GetAttributeInfo("label");
            missingModuleType.xAttribute = missingModuleType.Type.GetAttributeInfo("x");
            missingModuleType.yAttribute = missingModuleType.Type.GetAttributeInfo("y");
            missingModuleType.visibleAttribute = missingModuleType.Type.GetAttributeInfo("visible");
            missingModuleType.sourceGuidAttribute = missingModuleType.Type.GetAttributeInfo("sourceGuid");

            moduleTemplateRefType.Type = getNodeType("http://sony.com/gametech/circuits/1_0", "moduleTemplateRefType");
            moduleTemplateRefType.nameAttribute = moduleTemplateRefType.Type.GetAttributeInfo("name");
            moduleTemplateRefType.labelAttribute = moduleTemplateRefType.Type.GetAttributeInfo("label");
            moduleTemplateRefType.xAttribute = moduleTemplateRefType.Type.GetAttributeInfo("x");
            moduleTemplateRefType.yAttribute = moduleTemplateRefType.Type.GetAttributeInfo("y");
            moduleTemplateRefType.visibleAttribute = moduleTemplateRefType.Type.GetAttributeInfo("visible");
            moduleTemplateRefType.sourceGuidAttribute = moduleTemplateRefType.Type.GetAttributeInfo("sourceGuid");
            moduleTemplateRefType.guidRefAttribute = moduleTemplateRefType.Type.GetAttributeInfo("guidRef");
            moduleTemplateRefType.typeRefAttribute = moduleTemplateRefType.Type.GetAttributeInfo("typeRef");

            groupTemplateRefType.Type = getNodeType("http://sony.com/gametech/circuits/1_0", "groupTemplateRefType");
            groupTemplateRefType.nameAttribute = groupTemplateRefType.Type.GetAttributeInfo("name");
            groupTemplateRefType.labelAttribute = groupTemplateRefType.Type.GetAttributeInfo("label");
            groupTemplateRefType.xAttribute = groupTemplateRefType.Type.GetAttributeInfo("x");
            groupTemplateRefType.yAttribute = groupTemplateRefType.Type.GetAttributeInfo("y");
            groupTemplateRefType.visibleAttribute = groupTemplateRefType.Type.GetAttributeInfo("visible");
            groupTemplateRefType.sourceGuidAttribute = groupTemplateRefType.Type.GetAttributeInfo("sourceGuid");
            groupTemplateRefType.guidRefAttribute = groupTemplateRefType.Type.GetAttributeInfo("guidRef");
            groupTemplateRefType.typeRefAttribute = groupTemplateRefType.Type.GetAttributeInfo("typeRef");
            groupTemplateRefType.refExpandedAttribute = groupTemplateRefType.Type.GetAttributeInfo("refExpanded");
            groupTemplateRefType.refShowExpandedGroupPinsAttribute = groupTemplateRefType.Type.GetAttributeInfo("refShowExpandedGroupPins");

            circuitRootElement = getRootElement(NS, "circuit");
        }

        public static class circuitDocumentType
        {
            public static DomNodeType Type;
            public static ChildInfo moduleChild;
            public static ChildInfo connectionChild;
            public static ChildInfo layerFolderChild;
            public static ChildInfo annotationChild;
            public static ChildInfo subCircuitChild;
            public static ChildInfo prototypeFolderChild;
            public static ChildInfo templateFolderChild;
        }

        public static class circuitType
        {
            public static DomNodeType Type;
            public static ChildInfo moduleChild;
            public static ChildInfo connectionChild;
            public static ChildInfo layerFolderChild;
            public static ChildInfo annotationChild;
        }

        public static class moduleType
        {
            public static DomNodeType Type;
            public static AttributeInfo nameAttribute;
            public static AttributeInfo labelAttribute;
            public static AttributeInfo xAttribute;
            public static AttributeInfo yAttribute;
            public static AttributeInfo visibleAttribute;
            public static AttributeInfo sourceGuidAttribute;
        }

        public static class connectionType
        {
            public static DomNodeType Type;
            public static AttributeInfo labelAttribute;
            public static AttributeInfo outputModuleAttribute;
            public static AttributeInfo outputPinAttribute;
            public static AttributeInfo inputModuleAttribute;
            public static AttributeInfo inputPinAttribute;
        }

        public static class layerFolderType
        {
            public static DomNodeType Type;
            public static AttributeInfo nameAttribute;
            public static ChildInfo layerFolderChild;
            public static ChildInfo moduleRefChild;
        }

        public static class moduleRefType
        {
            public static DomNodeType Type;
            public static AttributeInfo refAttribute;
        }

        public static class annotationType
        {
            public static DomNodeType Type;
            public static AttributeInfo textAttribute;
            public static AttributeInfo xAttribute;
            public static AttributeInfo yAttribute;
            public static AttributeInfo widthAttribute;
            public static AttributeInfo heightAttribute;
            public static AttributeInfo backcolorAttribute;
        }

        public static class subCircuitType
        {
            public static DomNodeType Type;
            public static AttributeInfo nameAttribute;
            public static AttributeInfo labelAttribute;
            public static ChildInfo moduleChild;
            public static ChildInfo connectionChild;
            public static ChildInfo layerFolderChild;
            public static ChildInfo annotationChild;
            public static ChildInfo inputChild;
            public static ChildInfo outputChild;
        }

        public static class pinType
        {
            public static DomNodeType Type;
            public static AttributeInfo nameAttribute;
            public static AttributeInfo typeAttribute;
        }

        public static class prototypeFolderType
        {
            public static DomNodeType Type;
            public static AttributeInfo nameAttribute;
            public static ChildInfo prototypeFolderChild;
            public static ChildInfo prototypeChild;
        }

        public static class prototypeType
        {
            public static DomNodeType Type;
            public static AttributeInfo nameAttribute;
            public static ChildInfo moduleChild;
            public static ChildInfo connectionChild;
        }

        public static class templateFolderType
        {
            public static DomNodeType Type;
            public static AttributeInfo nameAttribute;
            public static AttributeInfo referenceFileAttribute;
            public static ChildInfo templateFolderChild;
            public static ChildInfo templateChild;
        }

        public static class templateType
        {
            public static DomNodeType Type;
            public static AttributeInfo guidAttribute;
            public static AttributeInfo labelAttribute;
            public static ChildInfo moduleChild;
        }

        public static class groupPinType
        {
            public static DomNodeType Type;
            public static AttributeInfo nameAttribute;
            public static AttributeInfo typeAttribute;
            public static AttributeInfo moduleAttribute;
            public static AttributeInfo pinAttribute;
            public static AttributeInfo visibleAttribute;
            public static AttributeInfo indexAttribute;
            public static AttributeInfo pinnedAttribute;
            public static AttributeInfo pinYAttribute;
        }

        public static class groupType
        {
            public static DomNodeType Type;
            public static AttributeInfo nameAttribute;
            public static AttributeInfo labelAttribute;
            public static AttributeInfo xAttribute;
            public static AttributeInfo yAttribute;
            public static AttributeInfo visibleAttribute;
            public static AttributeInfo sourceGuidAttribute;
            public static AttributeInfo expandedAttribute;
            public static AttributeInfo showExpandedGroupPinsAttribute;
            public static AttributeInfo autosizeAttribute;
            public static AttributeInfo widthAttribute;
            public static AttributeInfo heightAttribute;
            public static AttributeInfo minwidthAttribute;
            public static AttributeInfo minheightAttribute;
            public static ChildInfo inputChild;
            public static ChildInfo outputChild;
            public static ChildInfo moduleChild;
            public static ChildInfo connectionChild;
            public static ChildInfo annotationChild;
        }

        public static class subCircuitInstanceType
        {
            public static DomNodeType Type;
            public static AttributeInfo nameAttribute;
            public static AttributeInfo labelAttribute;
            public static AttributeInfo xAttribute;
            public static AttributeInfo yAttribute;
            public static AttributeInfo visibleAttribute;
            public static AttributeInfo sourceGuidAttribute;
            public static AttributeInfo typeAttribute;
        }

        public static class missingTemplateType
        {
            public static DomNodeType Type;
            public static AttributeInfo guidAttribute;
            public static AttributeInfo labelAttribute;
            public static ChildInfo moduleChild;
        }

        public static class missingModuleType
        {
            public static DomNodeType Type;
            public static AttributeInfo nameAttribute;
            public static AttributeInfo labelAttribute;
            public static AttributeInfo xAttribute;
            public static AttributeInfo yAttribute;
            public static AttributeInfo visibleAttribute;
            public static AttributeInfo sourceGuidAttribute;
        }

        public static class moduleTemplateRefType
        {
            public static DomNodeType Type;
            public static AttributeInfo nameAttribute;
            public static AttributeInfo labelAttribute;
            public static AttributeInfo xAttribute;
            public static AttributeInfo yAttribute;
            public static AttributeInfo visibleAttribute;
            public static AttributeInfo sourceGuidAttribute;
            public static AttributeInfo guidRefAttribute;
            public static AttributeInfo typeRefAttribute;
        }

        public static class groupTemplateRefType
        {
            public static DomNodeType Type;
            public static AttributeInfo nameAttribute;
            public static AttributeInfo labelAttribute;
            public static AttributeInfo xAttribute;
            public static AttributeInfo yAttribute;
            public static AttributeInfo visibleAttribute;
            public static AttributeInfo sourceGuidAttribute;
            public static AttributeInfo guidRefAttribute;
            public static AttributeInfo typeRefAttribute;
            public static AttributeInfo refExpandedAttribute;
            public static AttributeInfo refShowExpandedGroupPinsAttribute;
        }

        public static ChildInfo circuitRootElement;
    }
}
