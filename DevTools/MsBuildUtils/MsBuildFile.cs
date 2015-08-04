using System;
using System.Collections.Generic;
using System.Linq;

namespace MsBuildUtils
{
    public static class MsBuildFile
    {
        public static IEnumerable<string> GetXamlFilenames(string pathName)
        {
            return (from p in GetProjects(pathName)
                    from g in p.ItemGroups
                    from i in g.Items
                    where i.ItemType == "Page" && i.Include.EndsWith(".xaml", kCompareType)
                    select i.IncludeUri.AbsolutePath.Replace("%20", " ")
                ).ToList();
        }

        public static IEnumerable<string> GetCsharpFilenames(string pathName)
        {
            return (from p in GetProjects(pathName)
                    from g in p.ItemGroups
                    from i in g.Items
                    where i.ItemType == "Compile" && i.Include.EndsWith(".cs", kCompareType)
                    select i.IncludeUri.AbsolutePath.Replace("%20", " ")
                ).ToList();
        }

        public static IEnumerable<string> GetProjectFilenames(string pathName)
        {
            return (from p in GetProjects(pathName)
                    from g in p.ItemGroups
                    from i in g.Items
                    where 
                        i.ItemType == "Page" && i.Include.EndsWith(".xaml", kCompareType) ||
                        i.ItemType == "Compile" && i.Include.EndsWith(".cs", kCompareType)
                    select i.IncludeUri.AbsolutePath.Replace("%20", " ")
                ).ToList();
        }

        public static IEnumerable<Project> GetProjects(string pathName)
        {
            IEnumerable<Project> result = new Project[] {};

            if (pathName.EndsWith(".sln", kCompareType))
            {
                var solution = new Solution(pathName);
                result = solution.ProjectRefs.Where(pr => pr.AbsolutePath.EndsWith(".csproj")).Select(sp => new Project(sp.AbsolutePath));
            }
            else if (pathName.EndsWith(".csproj", kCompareType))
                result = new List<Project> { new Project(pathName) };

            return result;
        }

        private const StringComparison kCompareType = StringComparison.InvariantCultureIgnoreCase;
    }
}