// -------------------------------------------------------------------------------------------------------------------
// Generated code, do not edit
// Command Line:  DomGen "FSM_customized.xsd" "Schema.cs" "http://sony.com/gametech/fsms/1_0" "FsmEditorSample"
// -------------------------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;

using Sce.Atf.Dom;

namespace FsmEditorSample
{
    public static class Schema
    {
        public const string NS = "http://sony.com/gametech/fsms/1_0";

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
            fsmType.Type = getNodeType("http://sony.com/gametech/fsms/1_0", "fsmType");
            fsmType.stateChild = fsmType.Type.GetChildInfo("state");
            fsmType.transitionChild = fsmType.Type.GetChildInfo("transition");
            fsmType.annotationChild = fsmType.Type.GetChildInfo("annotation");
            fsmType.prototypeFolderChild = fsmType.Type.GetChildInfo("prototypeFolder");

            stateType.Type = getNodeType("http://sony.com/gametech/fsms/1_0", "stateType");
            stateType.entryActionAttribute = stateType.Type.GetAttributeInfo("entryAction");
            stateType.actionAttribute = stateType.Type.GetAttributeInfo("action");
            stateType.exitActionAttribute = stateType.Type.GetAttributeInfo("exitAction");
            stateType.nameAttribute = stateType.Type.GetAttributeInfo("name");
            stateType.labelAttribute = stateType.Type.GetAttributeInfo("label");
            stateType.xAttribute = stateType.Type.GetAttributeInfo("x");
            stateType.yAttribute = stateType.Type.GetAttributeInfo("y");
            stateType.sizeAttribute = stateType.Type.GetAttributeInfo("size");
            stateType.hiddenAttribute = stateType.Type.GetAttributeInfo("hidden");
            stateType.startAttribute = stateType.Type.GetAttributeInfo("start");

            transitionType.Type = getNodeType("http://sony.com/gametech/fsms/1_0", "transitionType");
            transitionType.actionAttribute = transitionType.Type.GetAttributeInfo("action");
            transitionType.labelAttribute = transitionType.Type.GetAttributeInfo("label");
            transitionType.sourceAttribute = transitionType.Type.GetAttributeInfo("source");
            transitionType.destinationAttribute = transitionType.Type.GetAttributeInfo("destination");
            transitionType.triggerChild = transitionType.Type.GetChildInfo("trigger");

            triggerType.Type = getNodeType("http://sony.com/gametech/fsms/1_0", "triggerType");
            triggerType.labelAttribute = triggerType.Type.GetAttributeInfo("label");
            triggerType.idAttribute = triggerType.Type.GetAttributeInfo("id");
            triggerType.activeAttribute = triggerType.Type.GetAttributeInfo("active");

            annotationType.Type = getNodeType("http://sony.com/gametech/fsms/1_0", "annotationType");
            annotationType.textAttribute = annotationType.Type.GetAttributeInfo("text");
            annotationType.xAttribute = annotationType.Type.GetAttributeInfo("x");
            annotationType.yAttribute = annotationType.Type.GetAttributeInfo("y");

            prototypeFolderType.Type = getNodeType("http://sony.com/gametech/fsms/1_0", "prototypeFolderType");
            prototypeFolderType.nameAttribute = prototypeFolderType.Type.GetAttributeInfo("name");
            prototypeFolderType.prototypeFolderChild = prototypeFolderType.Type.GetChildInfo("prototypeFolder");
            prototypeFolderType.prototypeChild = prototypeFolderType.Type.GetChildInfo("prototype");

            prototypeType.Type = getNodeType("http://sony.com/gametech/fsms/1_0", "prototypeType");
            prototypeType.nameAttribute = prototypeType.Type.GetAttributeInfo("name");
            prototypeType.stateChild = prototypeType.Type.GetChildInfo("state");
            prototypeType.transitionChild = prototypeType.Type.GetChildInfo("transition");

            fsmRootElement = getRootElement(NS, "fsm");
        }

        public static class fsmType
        {
            public static DomNodeType Type;
            public static ChildInfo stateChild;
            public static ChildInfo transitionChild;
            public static ChildInfo annotationChild;
            public static ChildInfo prototypeFolderChild;
        }

        public static class stateType
        {
            public static DomNodeType Type;
            public static AttributeInfo entryActionAttribute;
            public static AttributeInfo actionAttribute;
            public static AttributeInfo exitActionAttribute;
            public static AttributeInfo nameAttribute;
            public static AttributeInfo labelAttribute;
            public static AttributeInfo xAttribute;
            public static AttributeInfo yAttribute;
            public static AttributeInfo sizeAttribute;
            public static AttributeInfo hiddenAttribute;
            public static AttributeInfo startAttribute;
        }

        public static class transitionType
        {
            public static DomNodeType Type;
            public static AttributeInfo actionAttribute;
            public static AttributeInfo labelAttribute;
            public static AttributeInfo sourceAttribute;
            public static AttributeInfo destinationAttribute;
            public static ChildInfo triggerChild;
        }

        public static class triggerType
        {
            public static DomNodeType Type;
            public static AttributeInfo labelAttribute;
            public static AttributeInfo idAttribute;
            public static AttributeInfo activeAttribute;
        }

        public static class annotationType
        {
            public static DomNodeType Type;
            public static AttributeInfo textAttribute;
            public static AttributeInfo xAttribute;
            public static AttributeInfo yAttribute;
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
            public static ChildInfo stateChild;
            public static ChildInfo transitionChild;
        }

        public static ChildInfo fsmRootElement;
    }
}
