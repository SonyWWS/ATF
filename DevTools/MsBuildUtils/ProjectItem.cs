using System;
using System.Diagnostics;
using System.Reflection;

namespace MsBuildUtils
{
    [DebuggerDisplay("{ItemType}, {Include}, {IncludeUri}")]
    public class ProjectItem : ProjectElement
    {
        static readonly Type s_projectItemElement;
        static readonly PropertyInfo s_projectItemElementItemType;
        static readonly PropertyInfo s_projectItemElementInclude;

        static ProjectItem()
        {
            s_projectItemElement =
                Type.GetType(
                    "Microsoft.Build.Construction.ProjectItemElement, Microsoft.Build, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a",
                    false,
                    false);

            if (s_projectItemElement == null)
                return;

            s_projectItemElementItemType = s_projectItemElement.GetProperty("ItemType");
            s_projectItemElementInclude = s_projectItemElement.GetProperty("Include");
        }

        public ProjectItem(object projectItem)
            : base(projectItem)
        {
            if (s_projectItemElement == null)
            {
                throw new InvalidOperationException("Can not find type 'Microsoft.Build.Construction.ProjectItemElement' are you missing a assembly reference to 'Microsoft.Build.dll'?");
            }

            ItemType = s_projectItemElementItemType.GetValue(projectItem, null) as string;
            Include = s_projectItemElementInclude.GetValue(projectItem, null) as string;

            var baseUri = new Uri(File);
            Uri includeUri;
            if (!Uri.TryCreate(baseUri, Include, out includeUri))
            {
                throw new Exception("Failed to create absolute URI for project item include reference");
            }
            IncludeUri = includeUri;
        }

        public string ItemType { get; private set; }

        public string Include { get; private set; }

        public Uri IncludeUri { get; private set; }
    }
}