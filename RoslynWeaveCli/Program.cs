using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using System.Xml.XPath;
using Microsoft.Build.Construction;

namespace RoslynWeave
{
    static class Program
    {
        const string WrapInFolder = "AopManaged";
        static bool Inplace = true;
        static void Main(string[] args)
        {
            try
            {
                var solutionPath = "../../../../RoslynWeave.sln";
                if (args.Length > 0)
                {
                    solutionPath = args[0];
                }

                if (!Path.IsPathRooted(solutionPath))
                {
                    solutionPath = Path.Combine(Directory.GetCurrentDirectory(), solutionPath);
                }

                var config = new CodeRewriterConfig()
                {Track = Inplace};
                var rewriter = new CodeRewriter(config, new TemplateExtractor());
                var solution = SolutionFile.Parse(solutionPath);
                foreach (var project in solution.ProjectsInOrder)
                {
                    if (project.ProjectName == typeof(CodeRewriter).Namespace)
                    {
                        continue;
                    }

                    var newRoot = GetWrappedPath(project);
                    foreach (var document in GetDocuments(project))
                    {
                        string newCode = null;
                        using (var sr = new StreamReader(document))
                        {
                            newCode = rewriter.Wrap(sr.ReadToEnd());
                        }

                        var projectRoot = Path.GetDirectoryName(project.AbsolutePath);
                        var relative = Path.GetRelativePath(projectRoot, document);
                        var newPath = Path.Combine(newRoot, relative);
                        Directory.CreateDirectory(Path.GetDirectoryName(newPath));
                        using (var sw = new StreamWriter(newPath))
                        {
                            if (!Inplace)
                            {
                                sw.WriteLine("//RoslynWeave auto generated code.");
                            }
                            sw.Write(newCode);
                        }
                    }
                }

                foreach (var project in solution.ProjectsInOrder)
                {
                    if (project.ProjectName == typeof(CodeRewriter).Namespace)
                    {
                        continue;
                    }
                    if (!Inplace && Directory.Exists(GetWrappedPath(project)))
                    {
                        foreach (var item in Directory.GetFiles(GetWrappedPath(project), "*.cs", SearchOption.AllDirectories))
                        {
                            string newCode = null;
                            using (var sr = new StreamReader(item))
                            {
                                newCode = rewriter.UpdateNamespaces(sr.ReadToEnd());
                            }

                            using (var sw = new StreamWriter(item))
                            {
                                sw.Write(newCode);
                            }
                        }
                    }
                    if (Inplace && !string.IsNullOrWhiteSpace(config.NameSpaceSuffix) && rewriter.NameSpaces.Any())
                    {
                        foreach (var item in GetDocuments(project))
                        {
                            string newCode = null;
                            using (var sr = new StreamReader(item))
                            {
                                newCode = rewriter.UpdateNamespaces(sr.ReadToEnd());
                            }

                            using (var sw = new StreamWriter(item))
                            {
                                sw.Write(newCode);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.Write(ex);
            }
        }

        private static string CombindPath(params string[] pathParts)
        {
            return Path.Combine(pathParts).TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
        }

        private static string GetWrappedPath(ProjectInSolution project)
        {
            var projectRoot = Path.GetDirectoryName(project.AbsolutePath);
            return Path.Combine(projectRoot, Inplace ? "" : WrapInFolder);
        }

        public static IEnumerable<string> GetDocuments(ProjectInSolution project)
        {
            var projectRoot = Path.GetDirectoryName(project.AbsolutePath);
            var newRoot = GetWrappedPath(project);
            var xml = XElement.Load(project.AbsolutePath);
            var removes = xml.XPathSelectElements("//Compile[@Remove]").Select(x => Path.Combine(projectRoot, x.Attributes("Remove").First().Value).NormalizePath()).ToList();
            foreach (var file in Directory.GetFiles(projectRoot, "*.cs", SearchOption.AllDirectories))
            {
                var item = file.NormalizePath();
                if (!Inplace && new Uri(CombindPath(newRoot)).IsBaseOf(new Uri(item)))
                {
                    continue;
                }

                if (new Uri(CombindPath(projectRoot, "bin")).IsBaseOf(new Uri(item)))
                {
                    continue;
                }

                if (new Uri(CombindPath(projectRoot, "obj")).IsBaseOf(new Uri(item)))
                {
                    continue;
                }

                if (Path.GetFileName(item).ToLower() == "program.cs")
                {
                    continue;
                }

                if (removes.Contains(item))
                {
                    continue;
                }

                yield return item;
            }
        }

        public static string NormalizePath(this string path)
        {
            return Path.GetFullPath(path).Replace("\\", "/");
        }
    }
}