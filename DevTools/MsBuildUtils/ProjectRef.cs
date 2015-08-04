using System;
using System.Diagnostics;
using System.Reflection;

namespace MsBuildUtils
{
    [DebuggerDisplay("{ProjectName}, {RelativePath}, {ProjectGuid}")]
    public class ProjectRef
    {
        static readonly PropertyInfo s_projectInSolutionProjectName;
        static readonly PropertyInfo s_projectInSolutionRelativePath;
        static readonly PropertyInfo s_projectInSolutionProjectGuid;
        static readonly PropertyInfo s_projectInSolutionProjectType;

        static ProjectRef()
        {
            var projectInSolution = Type.GetType(
                "Microsoft.Build.Construction.ProjectInSolution, Microsoft.Build, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", 
                false, 
                false);

            if (projectInSolution == null)
                return;

            s_projectInSolutionProjectName = projectInSolution.GetProperty("ProjectName", BindingFlags.NonPublic | BindingFlags.Instance);
            s_projectInSolutionRelativePath = projectInSolution.GetProperty("RelativePath", BindingFlags.NonPublic | BindingFlags.Instance);
            s_projectInSolutionProjectGuid = projectInSolution.GetProperty("ProjectGuid", BindingFlags.NonPublic | BindingFlags.Instance);
            s_projectInSolutionProjectType = projectInSolution.GetProperty("ProjectType", BindingFlags.NonPublic | BindingFlags.Instance);
        }

        public string ProjectName { get; private set; }
        public string RelativePath { get; private set; }
        public string ProjectGuid { get; private set; }
        public string ProjectType { get; private set; }

        public string AbsolutePath { get; private set; }

        public ProjectRef(object solutionProject, string solutionPath)
        {
            ProjectName = s_projectInSolutionProjectName.GetValue(solutionProject, null) as string;
            RelativePath = s_projectInSolutionRelativePath.GetValue(solutionProject, null) as string;
            ProjectGuid = s_projectInSolutionProjectGuid.GetValue(solutionProject, null) as string;
            ProjectType = s_projectInSolutionProjectType.GetValue(solutionProject, null).ToString();

            if (!solutionPath.EndsWith("\\") && !solutionPath.EndsWith("/"))
                solutionPath += "\\";

            Uri projectUri;
            if (!Uri.TryCreate(new Uri(solutionPath), RelativePath, out projectUri))
                throw new ArgumentException("Could not create an absolute path for project '" + RelativePath + "', under base path '" + solutionPath + "'.");

            AbsolutePath = projectUri.AbsolutePath;
        }
    }
}