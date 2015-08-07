//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Diagnostics;
using System.Reflection;

namespace MsBuildUtils
{
    [DebuggerDisplay("{Name} = {Value}")]
    public class ProjectProperty
    {
        static readonly Type s_projectPropertyElement;
        static readonly PropertyInfo s_projectPropertyElementName;
        static readonly PropertyInfo s_projectPropertyElementValue;

        static ProjectProperty()
        {
            s_projectPropertyElement =
                Type.GetType(
                    "Microsoft.Build.Construction.ProjectPropertyElement, Microsoft.Build, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a",
                    false,
                    false);

            if (s_projectPropertyElement == null)
                return;

            s_projectPropertyElementName = s_projectPropertyElement.GetProperty("Name");
            s_projectPropertyElementValue = s_projectPropertyElement.GetProperty("Value");
        }

        public ProjectProperty(object projectProperty)
        {
            if (s_projectPropertyElement == null)
            {
                throw new InvalidOperationException("Can not find type 'Microsoft.Build.Construction.ProjectPropertyElement' are you missing a assembly reference to 'Microsoft.Build.dll'?");
            }

            Name = s_projectPropertyElementName.GetValue(projectProperty, null) as string;
            Value = s_projectPropertyElementValue.GetValue(projectProperty, null) as string;
        }

        public string Name { get; private set; }

        public string Value { get; private set; }
    }
}