// -------------------------------------------------------------------------------------------------------------------
// Generated code, do not edit
// Command Line:  DomGen "FSM_customized.xsd" "Schema.cs" "http://sony.com/gametech/fsms/1_0" "FsmEditorSample"
// -------------------------------------------------------------------------------------------------------------------

using Sce.Atf.Dom;

namespace FsmEditorSample
{
    public static class Schema
    {
        public const string NS = "http://sony.com/gametech/fsms/1_0";

        public static void Initialize(XmlSchemaTypeCollection typeCollection)
        {
            fsmType.Type = typeCollection.GetNodeType("fsmType");
            fsmType.stateChild = fsmType.Type.GetChildInfo("state");
            fsmType.transitionChild = fsmType.Type.GetChildInfo("transition");
            fsmType.annotationChild = fsmType.Type.GetChildInfo("annotation");
            fsmType.prototypeFolderChild = fsmType.Type.GetChildInfo("prototypeFolder");

            stateType.Type = typeCollection.GetNodeType("stateType");
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

            transitionType.Type = typeCollection.GetNodeType("transitionType");
            transitionType.triggerAttribute = transitionType.Type.GetAttributeInfo("trigger");
            transitionType.actionAttribute = transitionType.Type.GetAttributeInfo("action");
            transitionType.labelAttribute = transitionType.Type.GetAttributeInfo("label");
            transitionType.sourceAttribute = transitionType.Type.GetAttributeInfo("source");
            transitionType.destinationAttribute = transitionType.Type.GetAttributeInfo("destination");

            annotationType.Type = typeCollection.GetNodeType("annotationType");
            annotationType.textAttribute = annotationType.Type.GetAttributeInfo("text");
            annotationType.xAttribute = annotationType.Type.GetAttributeInfo("x");
            annotationType.yAttribute = annotationType.Type.GetAttributeInfo("y");

            prototypeFolderType.Type = typeCollection.GetNodeType("prototypeFolderType");
            prototypeFolderType.nameAttribute = prototypeFolderType.Type.GetAttributeInfo("name");
            prototypeFolderType.prototypeFolderChild = prototypeFolderType.Type.GetChildInfo("prototypeFolder");
            prototypeFolderType.prototypeChild = prototypeFolderType.Type.GetChildInfo("prototype");

            prototypeType.Type = typeCollection.GetNodeType("prototypeType");
            prototypeType.nameAttribute = prototypeType.Type.GetAttributeInfo("name");
            prototypeType.stateChild = prototypeType.Type.GetChildInfo("state");
            prototypeType.transitionChild = prototypeType.Type.GetChildInfo("transition");

            fsmRootElement = typeCollection.GetRootElement("fsm");
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
            public static AttributeInfo triggerAttribute;
            public static AttributeInfo actionAttribute;
            public static AttributeInfo labelAttribute;
            public static AttributeInfo sourceAttribute;
            public static AttributeInfo destinationAttribute;
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
