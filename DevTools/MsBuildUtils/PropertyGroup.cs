//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace MsBuildUtils
{
    [DebuggerDisplay("Condition: {Condition}, {PropertyCount} properties")]
    public class PropertyGroup : Group
    {
        static readonly Type s_projectPropertyGroupElement;
        static readonly PropertyInfo s_projectPropertyGroupElementProperties;

        static PropertyGroup()
        {
            s_projectPropertyGroupElement =
                Type.GetType(
                    "Microsoft.Build.Construction.ProjectPropertyGroupElement, Microsoft.Build, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a",
                    false,
                    false);

            if (s_projectPropertyGroupElement == null)
                return;

            s_projectPropertyGroupElementProperties = s_projectPropertyGroupElement.GetProperty("Properties");
        }

        public PropertyGroup(object propertyGroup)
            : base(propertyGroup)
        {
            if (s_projectPropertyGroupElement == null)
            {
                throw new InvalidOperationException("Can not find type 'Microsoft.Build.Construction.ProjectPropertyGroupElement' are you missing a assembly reference to 'Microsoft.Build.dll'?");
            }

            var properties = new List<ProjectProperty>();
            var array = ((ICollection)s_projectPropertyGroupElementProperties.GetValue(propertyGroup, null)).Cast<object>().ToArray();
            for (var i = 0; i < array.Length; i++)
            {
                properties.Add(new ProjectProperty(array.GetValue(i)));
            }
            Properties = properties;
        }

        public List<ProjectProperty> Properties { get; private set; }

        public int PropertyCount { get { return Properties != null ? Properties.Count : 0; } }
    }
}