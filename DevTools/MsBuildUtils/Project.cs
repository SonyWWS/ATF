using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;

namespace MsBuildUtils
{
    [DebuggerDisplay("{Name}, {PropertyGroupsCount} PropertyGroups, {ItemGroupsCount} ItemGroups")]
    public class Project
    {
        static readonly Type s_projectRootElement;
        static readonly MethodInfo s_projectRootElementOpen;
        static readonly PropertyInfo s_projectRootElementItemGroups;
        static readonly PropertyInfo s_projectRootElementPropertyGroups;

        static Project()
        {
            s_projectRootElement =
                Type.GetType(
                    "Microsoft.Build.Construction.ProjectRootElement, Microsoft.Build, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a");

            if (s_projectRootElement == null)
                return;

            s_projectRootElementOpen = s_projectRootElement.GetMethod("Open", new [] {typeof(String)});
            s_projectRootElementItemGroups = s_projectRootElement.GetProperty("ItemGroups");
            s_projectRootElementPropertyGroups = s_projectRootElement.GetProperty("PropertyGroups");
        }

        public Project(string fileName)
        {
            if (s_projectRootElement == null)
            {
                throw new InvalidOperationException("Can not find type 'Microsoft.Build.Construction.ProjectRootElement' are you missing a assembly reference to 'Microsoft.Build.dll'?");
            }

            var instance = s_projectRootElementOpen.Invoke(null, new object[] { fileName });
            if (instance == null)
            {
                throw new Exception("Could not open project '" + fileName + "'.");
            }

            FileName = fileName;

            {
                var itemGroups = new List<ItemGroup>();
                var array = ((ICollection)s_projectRootElementItemGroups.GetValue(instance, null)).Cast<object>().ToArray();
                for (var i = 0; i < array.Length; i++)
                {
                    itemGroups.Add(new ItemGroup(array.GetValue(i)));
                }
                ItemGroups = itemGroups;
            }

            {
                var propertyGroups = new List<PropertyGroup>();
                var array = ((ICollection)s_projectRootElementPropertyGroups.GetValue(instance, null)).Cast<object>().ToArray();
                for (var i = 0; i < array.Length; i++)
                {
                    propertyGroups.Add(new PropertyGroup(array.GetValue(i)));
                }
                PropertyGroups = propertyGroups;
            }
        }

        public string FileName { get; private set; }

        public string Name { get { return !string.IsNullOrEmpty(FileName) ? Path.GetFileNameWithoutExtension(FileName) : "<no_file_specified>"; } }

        public List<ItemGroup> ItemGroups { get; private set; }

        public int ItemGroupsCount { get { return ItemGroups != null ? ItemGroups.Count : 0; } }

        public List<PropertyGroup> PropertyGroups { get; private set; }

        public int PropertyGroupsCount { get { return PropertyGroups != null ? PropertyGroups.Count : 0; } }
    }
}