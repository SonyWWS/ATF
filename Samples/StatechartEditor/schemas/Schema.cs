// -------------------------------------------------------------------------------------------------------------------
// Generated code, do not edit
// Command Line:  DomGen "Statechart.xsd" "Schema.cs" "http://sony.com/gametech/statecharts/1_0" "StatechartEditorSample"
// -------------------------------------------------------------------------------------------------------------------

using Sce.Atf.Dom;

namespace StatechartEditorSample
{
    public static class Schema
    {
        public const string NS = "http://sony.com/gametech/statecharts/1_0";

        public static void Initialize(XmlSchemaTypeCollection typeCollection)
        {
            statechartDocumentType.Type = typeCollection.GetNodeType("statechartDocumentType");
            statechartDocumentType.stateChild = statechartDocumentType.Type.GetChildInfo("state");
            statechartDocumentType.transitionChild = statechartDocumentType.Type.GetChildInfo("transition");
            statechartDocumentType.annotationChild = statechartDocumentType.Type.GetChildInfo("annotation");
            statechartDocumentType.prototypeFolderChild = statechartDocumentType.Type.GetChildInfo("prototypeFolder");

            statechartType.Type = typeCollection.GetNodeType("statechartType");
            statechartType.stateChild = statechartType.Type.GetChildInfo("state");

            stateBaseType.Type = typeCollection.GetNodeType("stateBaseType");
            stateBaseType.nameAttribute = stateBaseType.Type.GetAttributeInfo("name");
            stateBaseType.xAttribute = stateBaseType.Type.GetAttributeInfo("x");
            stateBaseType.yAttribute = stateBaseType.Type.GetAttributeInfo("y");

            transitionType.Type = typeCollection.GetNodeType("transitionType");
            transitionType.eventAttribute = transitionType.Type.GetAttributeInfo("event");
            transitionType.guardAttribute = transitionType.Type.GetAttributeInfo("guard");
            transitionType.actionAttribute = transitionType.Type.GetAttributeInfo("action");
            transitionType.fromStateAttribute = transitionType.Type.GetAttributeInfo("fromState");
            transitionType.fromPositionAttribute = transitionType.Type.GetAttributeInfo("fromPosition");
            transitionType.toStateAttribute = transitionType.Type.GetAttributeInfo("toState");
            transitionType.toPositionAttribute = transitionType.Type.GetAttributeInfo("toPosition");

            reactionType.Type = typeCollection.GetNodeType("reactionType");
            reactionType.eventAttribute = reactionType.Type.GetAttributeInfo("event");
            reactionType.guardAttribute = reactionType.Type.GetAttributeInfo("guard");
            reactionType.actionAttribute = reactionType.Type.GetAttributeInfo("action");

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

            stateType.Type = typeCollection.GetNodeType("stateType");
            stateType.nameAttribute = stateType.Type.GetAttributeInfo("name");
            stateType.xAttribute = stateType.Type.GetAttributeInfo("x");
            stateType.yAttribute = stateType.Type.GetAttributeInfo("y");
            stateType.labelAttribute = stateType.Type.GetAttributeInfo("label");
            stateType.widthAttribute = stateType.Type.GetAttributeInfo("width");
            stateType.heightAttribute = stateType.Type.GetAttributeInfo("height");
            stateType.entryActionAttribute = stateType.Type.GetAttributeInfo("entryAction");
            stateType.exitActionAttribute = stateType.Type.GetAttributeInfo("exitAction");
            stateType.reactionChild = stateType.Type.GetChildInfo("reaction");
            stateType.statechartChild = stateType.Type.GetChildInfo("statechart");

            startStateType.Type = typeCollection.GetNodeType("startStateType");
            startStateType.nameAttribute = startStateType.Type.GetAttributeInfo("name");
            startStateType.xAttribute = startStateType.Type.GetAttributeInfo("x");
            startStateType.yAttribute = startStateType.Type.GetAttributeInfo("y");

            finalStateType.Type = typeCollection.GetNodeType("finalStateType");
            finalStateType.nameAttribute = finalStateType.Type.GetAttributeInfo("name");
            finalStateType.xAttribute = finalStateType.Type.GetAttributeInfo("x");
            finalStateType.yAttribute = finalStateType.Type.GetAttributeInfo("y");

            historyStateType.Type = typeCollection.GetNodeType("historyStateType");
            historyStateType.nameAttribute = historyStateType.Type.GetAttributeInfo("name");
            historyStateType.xAttribute = historyStateType.Type.GetAttributeInfo("x");
            historyStateType.yAttribute = historyStateType.Type.GetAttributeInfo("y");
            historyStateType.typeAttribute = historyStateType.Type.GetAttributeInfo("type");

            conditionalStateType.Type = typeCollection.GetNodeType("conditionalStateType");
            conditionalStateType.nameAttribute = conditionalStateType.Type.GetAttributeInfo("name");
            conditionalStateType.xAttribute = conditionalStateType.Type.GetAttributeInfo("x");
            conditionalStateType.yAttribute = conditionalStateType.Type.GetAttributeInfo("y");

            statechartRootElement = typeCollection.GetRootElement("statechart");
        }

        public static class statechartDocumentType
        {
            public static DomNodeType Type;
            public static ChildInfo stateChild;
            public static ChildInfo transitionChild;
            public static ChildInfo annotationChild;
            public static ChildInfo prototypeFolderChild;
        }

        public static class statechartType
        {
            public static DomNodeType Type;
            public static ChildInfo stateChild;
        }

        public static class stateBaseType
        {
            public static DomNodeType Type;
            public static AttributeInfo nameAttribute;
            public static AttributeInfo xAttribute;
            public static AttributeInfo yAttribute;
        }

        public static class transitionType
        {
            public static DomNodeType Type;
            public static AttributeInfo eventAttribute;
            public static AttributeInfo guardAttribute;
            public static AttributeInfo actionAttribute;
            public static AttributeInfo fromStateAttribute;
            public static AttributeInfo fromPositionAttribute;
            public static AttributeInfo toStateAttribute;
            public static AttributeInfo toPositionAttribute;
        }

        public static class reactionType
        {
            public static DomNodeType Type;
            public static AttributeInfo eventAttribute;
            public static AttributeInfo guardAttribute;
            public static AttributeInfo actionAttribute;
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

        public static class stateType
        {
            public static DomNodeType Type;
            public static AttributeInfo nameAttribute;
            public static AttributeInfo xAttribute;
            public static AttributeInfo yAttribute;
            public static AttributeInfo labelAttribute;
            public static AttributeInfo widthAttribute;
            public static AttributeInfo heightAttribute;
            public static AttributeInfo entryActionAttribute;
            public static AttributeInfo exitActionAttribute;
            public static ChildInfo reactionChild;
            public static ChildInfo statechartChild;
        }

        public static class startStateType
        {
            public static DomNodeType Type;
            public static AttributeInfo nameAttribute;
            public static AttributeInfo xAttribute;
            public static AttributeInfo yAttribute;
        }

        public static class finalStateType
        {
            public static DomNodeType Type;
            public static AttributeInfo nameAttribute;
            public static AttributeInfo xAttribute;
            public static AttributeInfo yAttribute;
        }

        public static class historyStateType
        {
            public static DomNodeType Type;
            public static AttributeInfo nameAttribute;
            public static AttributeInfo xAttribute;
            public static AttributeInfo yAttribute;
            public static AttributeInfo typeAttribute;
        }

        public static class conditionalStateType
        {
            public static DomNodeType Type;
            public static AttributeInfo nameAttribute;
            public static AttributeInfo xAttribute;
            public static AttributeInfo yAttribute;
        }

        public static ChildInfo statechartRootElement;
    }
}
