using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;

namespace MsBuildUtils
{
    [DebuggerDisplay("{ProjectRefs}")]
    public class Solution
    {
        //internal class SolutionParser
        //Name: Microsoft.Build.Construction.SolutionParser
        //Assembly: Microsoft.Build, Version=4.0.0.0

        static readonly Type s_solutionParser;
        static readonly PropertyInfo s_solutionParserSolutionReader;
        static readonly MethodInfo s_solutionParserParseSolution;
        static readonly PropertyInfo s_solutionParserProjects;

        static Solution()
        {
            s_solutionParser = Type.GetType("Microsoft.Build.Construction.SolutionParser, Microsoft.Build, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", false, false);

            if (s_solutionParser == null)
                return;

            s_solutionParserSolutionReader = s_solutionParser.GetProperty("SolutionReader", BindingFlags.NonPublic | BindingFlags.Instance);
            s_solutionParserProjects = s_solutionParser.GetProperty("Projects", BindingFlags.NonPublic | BindingFlags.Instance);
            s_solutionParserParseSolution = s_solutionParser.GetMethod("ParseSolution", BindingFlags.NonPublic | BindingFlags.Instance);
        }

        public List<ProjectRef> ProjectRefs
        { get; private set; }

        public Solution(string solutionFileName)
        {
            if (s_solutionParser == null)
            {
                throw new InvalidOperationException("Can not find type 'Microsoft.Build.Construction.SolutionParser' are you missing a assembly reference to 'Microsoft.Build.dll'?");
            }
            var solutionParser = s_solutionParser.GetConstructors(BindingFlags.Instance | BindingFlags.NonPublic).First().Invoke(null);
            using (var streamReader = new StreamReader(solutionFileName))
            {
                s_solutionParserSolutionReader.SetValue(solutionParser, streamReader, null);
                s_solutionParserParseSolution.Invoke(solutionParser, null);
            }
            var projectRefs = new List<ProjectRef>();
            var array = (Array)s_solutionParserProjects.GetValue(solutionParser, null);
            for (var i = 0; i < array.Length; i++)
                projectRefs.Add(new ProjectRef(array.GetValue(i), Path.GetDirectoryName(solutionFileName)));

            ProjectRefs = projectRefs;
        }
    }
}