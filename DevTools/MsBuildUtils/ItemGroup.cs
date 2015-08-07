//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace MsBuildUtils
{
    [DebuggerDisplay("Condition: {Condition}, {ItemCount} items")]
    public class ItemGroup : Group
    {
        static readonly Type s_projectItemGroupElement;
        static readonly PropertyInfo s_projectItemGroupElementItems;

        static ItemGroup()
        {
            s_projectItemGroupElement =
                Type.GetType(
                    "Microsoft.Build.Construction.ProjectItemGroupElement, Microsoft.Build, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a",
                    false,
                    false);

            if (s_projectItemGroupElement == null)
                return;

            s_projectItemGroupElementItems = s_projectItemGroupElement.GetProperty("Items");
        }

        public ItemGroup(object itemGroup)
            : base(itemGroup)
        {
            if (s_projectItemGroupElement == null)
            {
                throw new InvalidOperationException("Can not find type 'Microsoft.Build.Construction.ProjectItemGroupElement' are you missing a assembly reference to 'Microsoft.Build.dll'?");
            }

            var items = new List<ProjectItem>();
            var array = ((ICollection)s_projectItemGroupElementItems.GetValue(itemGroup, null)).Cast<object>().ToArray();
            for (var i = 0; i < array.Length; i++)
            {
                items.Add(new ProjectItem(array.GetValue(i)));
            }
            Items = items;
        }

        public List<ProjectItem> Items { get; private set; }

        public int ItemCount { get { return Items != null ? Items.Count : 0; } }  
    }
}