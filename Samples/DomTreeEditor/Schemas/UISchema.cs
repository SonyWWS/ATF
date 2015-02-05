// -------------------------------------------------------------------------------------------------------------------
// Generated code, do not edit
// Command Line:  DomGen "UISchema.xsd" "UISchema.cs" "IdolMinds.UI" "DomTreeEditorSample"
// -------------------------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;

using Sce.Atf.Dom;

namespace DomTreeEditorSample
{
    public static class UISchema
    {
        public const string NS = "IdolMinds.UI";

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
            UIType.Type = getNodeType("IdolMinds.UI", "UIType");
            UIType.nameAttribute = UIType.Type.GetAttributeInfo("name");
            UIType.PackageChild = UIType.Type.GetChildInfo("Package");

            UIObjectType.Type = getNodeType("IdolMinds.UI", "UIObjectType");
            UIObjectType.nameAttribute = UIObjectType.Type.GetAttributeInfo("name");

            UIPackageType.Type = getNodeType("IdolMinds.UI", "UIPackageType");
            UIPackageType.nameAttribute = UIPackageType.Type.GetAttributeInfo("name");
            UIPackageType.FormChild = UIPackageType.Type.GetChildInfo("Form");
            UIPackageType.ShaderChild = UIPackageType.Type.GetChildInfo("Shader");
            UIPackageType.TextureChild = UIPackageType.Type.GetChildInfo("Texture");
            UIPackageType.FontChild = UIPackageType.Type.GetChildInfo("Font");

            UIFormType.Type = getNodeType("IdolMinds.UI", "UIFormType");
            UIFormType.nameAttribute = UIFormType.Type.GetAttributeInfo("name");
            UIFormType.ControlChild = UIFormType.Type.GetChildInfo("Control");
            UIFormType.AnimationChild = UIFormType.Type.GetChildInfo("Animation");

            UIControlType.Type = getNodeType("IdolMinds.UI", "UIControlType");
            UIControlType.nameAttribute = UIControlType.Type.GetAttributeInfo("name");
            UIControlType.TransformChild = UIControlType.Type.GetChildInfo("Transform");
            UIControlType.AnimationChild = UIControlType.Type.GetChildInfo("Animation");
            UIControlType.ControlChild = UIControlType.Type.GetChildInfo("Control");

            UITransformType.Type = getNodeType("IdolMinds.UI", "UITransformType");
            UITransformType.RotateAttribute = UITransformType.Type.GetAttributeInfo("Rotate");
            UITransformType.ScaleAttribute = UITransformType.Type.GetAttributeInfo("Scale");
            UITransformType.TranslateAttribute = UITransformType.Type.GetAttributeInfo("Translate");

            UIAnimationType.Type = getNodeType("IdolMinds.UI", "UIAnimationType");
            UIAnimationType.nameAttribute = UIAnimationType.Type.GetAttributeInfo("name");
            UIAnimationType.AnimationTransformAttribute = UIAnimationType.Type.GetAttributeInfo("AnimationTransform");
            UIAnimationType.AnimalKindsAttribute = UIAnimationType.Type.GetAttributeInfo("AnimalKinds");
            UIAnimationType.curveChild = UIAnimationType.Type.GetChildInfo("curve");

            curveType.Type = getNodeType("IdolMinds.UI", "curveType");
            curveType.nameAttribute = curveType.Type.GetAttributeInfo("name");
            curveType.displayNameAttribute = curveType.Type.GetAttributeInfo("displayName");
            curveType.minXAttribute = curveType.Type.GetAttributeInfo("minX");
            curveType.maxXAttribute = curveType.Type.GetAttributeInfo("maxX");
            curveType.minYAttribute = curveType.Type.GetAttributeInfo("minY");
            curveType.maxYAttribute = curveType.Type.GetAttributeInfo("maxY");
            curveType.preInfinityAttribute = curveType.Type.GetAttributeInfo("preInfinity");
            curveType.postInfinityAttribute = curveType.Type.GetAttributeInfo("postInfinity");
            curveType.colorAttribute = curveType.Type.GetAttributeInfo("color");
            curveType.xLabelAttribute = curveType.Type.GetAttributeInfo("xLabel");
            curveType.yLabelAttribute = curveType.Type.GetAttributeInfo("yLabel");
            curveType.controlPointChild = curveType.Type.GetChildInfo("controlPoint");

            controlPointType.Type = getNodeType("IdolMinds.UI", "controlPointType");
            controlPointType.xAttribute = controlPointType.Type.GetAttributeInfo("x");
            controlPointType.yAttribute = controlPointType.Type.GetAttributeInfo("y");
            controlPointType.tangentInAttribute = controlPointType.Type.GetAttributeInfo("tangentIn");
            controlPointType.tangentInTypeAttribute = controlPointType.Type.GetAttributeInfo("tangentInType");
            controlPointType.tangentOutAttribute = controlPointType.Type.GetAttributeInfo("tangentOut");
            controlPointType.tangentOutTypeAttribute = controlPointType.Type.GetAttributeInfo("tangentOutType");
            controlPointType.brokenTangentsAttribute = controlPointType.Type.GetAttributeInfo("brokenTangents");

            UIShaderType.Type = getNodeType("IdolMinds.UI", "UIShaderType");
            UIShaderType.nameAttribute = UIShaderType.Type.GetAttributeInfo("name");
            UIShaderType.FxFileAttribute = UIShaderType.Type.GetAttributeInfo("FxFile");
            UIShaderType.ShaderIDAttribute = UIShaderType.Type.GetAttributeInfo("ShaderID");
            UIShaderType.ShaderParamAttribute = UIShaderType.Type.GetAttributeInfo("ShaderParam");

            UITextureType.Type = getNodeType("IdolMinds.UI", "UITextureType");
            UITextureType.nameAttribute = UITextureType.Type.GetAttributeInfo("name");
            UITextureType.TextureFileAttribute = UITextureType.Type.GetAttributeInfo("TextureFile");
            UITextureType.TextureFolderAttribute = UITextureType.Type.GetAttributeInfo("TextureFolder");
            UITextureType.TextureArrayAttribute = UITextureType.Type.GetAttributeInfo("TextureArray");
            UITextureType.TextureRevDateAttribute = UITextureType.Type.GetAttributeInfo("TextureRevDate");

            UIFontType.Type = getNodeType("IdolMinds.UI", "UIFontType");
            UIFontType.nameAttribute = UIFontType.Type.GetAttributeInfo("name");
            UIFontType.FontFileAttribute = UIFontType.Type.GetAttributeInfo("FontFile");
            UIFontType.FontFolderAttribute = UIFontType.Type.GetAttributeInfo("FontFolder");
            UIFontType.FontSizeAttribute = UIFontType.Type.GetAttributeInfo("FontSize");
            UIFontType.FontParamsAttribute = UIFontType.Type.GetAttributeInfo("FontParams");
            UIFontType.FontColorAttribute = UIFontType.Type.GetAttributeInfo("FontColor");

            UIRefType.Type = getNodeType("IdolMinds.UI", "UIRefType");
            UIRefType.refAttribute = UIRefType.Type.GetAttributeInfo("ref");

            UISpriteType.Type = getNodeType("IdolMinds.UI", "UISpriteType");
            UISpriteType.nameAttribute = UISpriteType.Type.GetAttributeInfo("name");
            UISpriteType.SpriteTypeAttribute = UISpriteType.Type.GetAttributeInfo("SpriteType");
            UISpriteType.TransformChild = UISpriteType.Type.GetChildInfo("Transform");
            UISpriteType.AnimationChild = UISpriteType.Type.GetChildInfo("Animation");
            UISpriteType.ControlChild = UISpriteType.Type.GetChildInfo("Control");
            UISpriteType.ShaderChild = UISpriteType.Type.GetChildInfo("Shader");

            UITextItemType.Type = getNodeType("IdolMinds.UI", "UITextItemType");
            UITextItemType.nameAttribute = UITextItemType.Type.GetAttributeInfo("name");
            UITextItemType.textAttribute = UITextItemType.Type.GetAttributeInfo("text");
            UITextItemType.TransformChild = UITextItemType.Type.GetChildInfo("Transform");
            UITextItemType.AnimationChild = UITextItemType.Type.GetChildInfo("Animation");
            UITextItemType.ControlChild = UITextItemType.Type.GetChildInfo("Control");
            UITextItemType.FontChild = UITextItemType.Type.GetChildInfo("Font");

            UIRootElement = getRootElement(NS, "UI");
        }

        public static class UIType
        {
            public static DomNodeType Type;
            public static AttributeInfo nameAttribute;
            public static ChildInfo PackageChild;
        }

        public static class UIObjectType
        {
            public static DomNodeType Type;
            public static AttributeInfo nameAttribute;
        }

        public static class UIPackageType
        {
            public static DomNodeType Type;
            public static AttributeInfo nameAttribute;
            public static ChildInfo FormChild;
            public static ChildInfo ShaderChild;
            public static ChildInfo TextureChild;
            public static ChildInfo FontChild;
        }

        public static class UIFormType
        {
            public static DomNodeType Type;
            public static AttributeInfo nameAttribute;
            public static ChildInfo ControlChild;
            public static ChildInfo AnimationChild;
        }

        public static class UIControlType
        {
            public static DomNodeType Type;
            public static AttributeInfo nameAttribute;
            public static ChildInfo TransformChild;
            public static ChildInfo AnimationChild;
            public static ChildInfo ControlChild;
        }

        public static class UITransformType
        {
            public static DomNodeType Type;
            public static AttributeInfo RotateAttribute;
            public static AttributeInfo ScaleAttribute;
            public static AttributeInfo TranslateAttribute;
        }

        public static class UIAnimationType
        {
            public static DomNodeType Type;
            public static AttributeInfo nameAttribute;
            public static AttributeInfo AnimationTransformAttribute;
            public static AttributeInfo AnimalKindsAttribute;
            public static ChildInfo curveChild;
        }

        public static class curveType
        {
            public static DomNodeType Type;
            public static AttributeInfo nameAttribute;
            public static AttributeInfo displayNameAttribute;
            public static AttributeInfo minXAttribute;
            public static AttributeInfo maxXAttribute;
            public static AttributeInfo minYAttribute;
            public static AttributeInfo maxYAttribute;
            public static AttributeInfo preInfinityAttribute;
            public static AttributeInfo postInfinityAttribute;
            public static AttributeInfo colorAttribute;
            public static AttributeInfo xLabelAttribute;
            public static AttributeInfo yLabelAttribute;
            public static ChildInfo controlPointChild;
        }

        public static class controlPointType
        {
            public static DomNodeType Type;
            public static AttributeInfo xAttribute;
            public static AttributeInfo yAttribute;
            public static AttributeInfo tangentInAttribute;
            public static AttributeInfo tangentInTypeAttribute;
            public static AttributeInfo tangentOutAttribute;
            public static AttributeInfo tangentOutTypeAttribute;
            public static AttributeInfo brokenTangentsAttribute;
        }

        public static class UIShaderType
        {
            public static DomNodeType Type;
            public static AttributeInfo nameAttribute;
            public static AttributeInfo FxFileAttribute;
            public static AttributeInfo ShaderIDAttribute;
            public static AttributeInfo ShaderParamAttribute;
        }

        public static class UITextureType
        {
            public static DomNodeType Type;
            public static AttributeInfo nameAttribute;
            public static AttributeInfo TextureFileAttribute;
            public static AttributeInfo TextureFolderAttribute;
            public static AttributeInfo TextureArrayAttribute;
            public static AttributeInfo TextureRevDateAttribute;
        }

        public static class UIFontType
        {
            public static DomNodeType Type;
            public static AttributeInfo nameAttribute;
            public static AttributeInfo FontFileAttribute;
            public static AttributeInfo FontFolderAttribute;
            public static AttributeInfo FontSizeAttribute;
            public static AttributeInfo FontParamsAttribute;
            public static AttributeInfo FontColorAttribute;
        }

        public static class UIRefType
        {
            public static DomNodeType Type;
            public static AttributeInfo refAttribute;
        }

        public static class UISpriteType
        {
            public static DomNodeType Type;
            public static AttributeInfo nameAttribute;
            public static AttributeInfo SpriteTypeAttribute;
            public static ChildInfo TransformChild;
            public static ChildInfo AnimationChild;
            public static ChildInfo ControlChild;
            public static ChildInfo ShaderChild;
        }

        public static class UITextItemType
        {
            public static DomNodeType Type;
            public static AttributeInfo nameAttribute;
            public static AttributeInfo textAttribute;
            public static ChildInfo TransformChild;
            public static ChildInfo AnimationChild;
            public static ChildInfo ControlChild;
            public static ChildInfo FontChild;
        }

        public static ChildInfo UIRootElement;
    }
}
