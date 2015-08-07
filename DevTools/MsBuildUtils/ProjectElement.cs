//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Diagnostics;
using System.Reflection;

namespace MsBuildUtils
{
    [DebuggerDisplay("{LocationString}")]
    public class ProjectElement
    {
        static readonly Type s_projectElement;
        static readonly PropertyInfo s_projectElementLocation;

        static readonly Type s_elementLocation;
        static readonly PropertyInfo s_elementLocationFile;
        static readonly PropertyInfo s_elementLocationLine;
        static readonly PropertyInfo s_elementLocationColumn;
        static readonly PropertyInfo s_elementLocationLocationString;

        static ProjectElement()
        {
            s_projectElement =
                Type.GetType(
                    "Microsoft.Build.Construction.ProjectElement, Microsoft.Build, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a",
                    false,
                    false);

            if (s_projectElement != null)
            {
                s_projectElementLocation = s_projectElement.GetProperty("Location");
            }

            s_elementLocation =
                Type.GetType(
                    "Microsoft.Build.Construction.ElementLocation, Microsoft.Build, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a",
                    false,
                    false);

            if (s_elementLocation != null)
            {
                s_elementLocationFile = s_elementLocation.GetProperty("File");
                s_elementLocationLine= s_elementLocation.GetProperty("Line");
                s_elementLocationColumn = s_elementLocation.GetProperty("Column");
                s_elementLocationLocationString = s_elementLocation.GetProperty("LocationString");
            }
        }

        public ProjectElement(object projectElement)
        {
            if (s_projectElement == null || s_elementLocation == null)
            {
                throw new InvalidOperationException("Can not find type 'Microsoft.Build.Construction.ProjectElement' or 'Microsoft.Build.Construction.ElementLocation' are you missing a assembly reference to 'Microsoft.Build.dll'?");
            }

            var location = s_projectElementLocation.GetValue(projectElement, null);

            File = s_elementLocationFile.GetValue(location, null) as string;
            Line = (int)s_elementLocationLine.GetValue(location, null);
            Column = (int)s_elementLocationColumn.GetValue(location, null);
            LocationString = s_elementLocationLocationString.GetValue(location, null) as string;
        }

        public string File { get; private set; }

        public int Line { get; private set; }

        public int Column { get; private set; }

        public string LocationString { get; private set; }
    }
}