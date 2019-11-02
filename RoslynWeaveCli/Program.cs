using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using System.Xml.XPath;
using Microsoft.Build.Construction;
using Microsoft.Extensions.Configuration;

namespace RoslynWeave
{
    static class Program
    {
        public class CliConfig
        {
            public string Output { get; set; }
            public string Solution { get; set; } = "../../../../RoslynWeave.sln";
            public bool Inplace { get; set; }
            public bool Replicate { get; set; }
            public string SolutionPath => Path.GetDirectoryName(Path.GetFullPath(Solution));
        }
        static CliConfig config = new CliConfig();
        static void Main(string[] args)
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "appsettings.json"), optional: true, reloadOnChange: true)
                .AddEnvironmentVariables()
                .AddCommandLine(args, new Dictionary<string, string>() { { "-o", "Output" }, { "-i", "Inplace" }, { "-s", "Solution" }, { "-r", "Replicate" } })
                .Build();
            configuration.Bind(config);
            try
            {
                config.Solution = Path.GetFullPath(config.Solution);
                if (config.Replicate)
                {
                    var targetFolder = Path.Combine(Directory.GetParent(config.SolutionPath).FullName, config.Output);
                    CopySolution(config.SolutionPath, targetFolder);
                    config.Solution = Path.Combine(targetFolder, Path.GetFileName(config.Solution));
                    config.Inplace = true;
                }

                var writerconfig = new CodeRewriterConfig() {Track = config.Inplace};
                var rewriter = new CodeRewriter(writerconfig, new TemplateExtractor());
                var solution = SolutionFile.Parse(config.Solution);
                foreach (var project in solution.ProjectsInOrder)
                {
                    if (project.ProjectName == typeof(CodeRewriter).Namespace)
                    {
                        continue;
                    }

                    var newRoot = GetWrappedPath(project);
                    foreach (var document in GetDocuments(project))
                    {
                        var projectRoot = Path.GetDirectoryName(project.AbsolutePath);
                        var relative = Path.GetRelativePath(projectRoot, document);
                        var newPath = Path.Combine(newRoot, relative);
                        Directory.CreateDirectory(Path.GetDirectoryName(newPath));
                        var newCode = rewriter.Wrap(File.ReadAllText(document));
                        File.WriteAllText(newPath, config.Inplace ? newCode : "//RoslynWeave auto generated code." + Environment.NewLine + newCode);
                    }
                }

                foreach (var project in solution.ProjectsInOrder)
                {
                    if (project.ProjectName == typeof(CodeRewriter).Namespace)
                    {
                        continue;
                    }
                    if (!config.Inplace && Directory.Exists(GetWrappedPath(project)))
                    {
                        foreach (var item in Directory.GetFiles(GetWrappedPath(project), "*.cs", SearchOption.AllDirectories))
                        {
                            var newCode = rewriter.UpdateNamespaces(File.ReadAllText(item));
                            File.WriteAllText(item, newCode);
                        }
                    }
                    if (config.Inplace && !string.IsNullOrWhiteSpace(writerconfig.NameSpaceSuffix) && rewriter.NameSpaces.Any())
                    {
                        foreach (var item in GetDocuments(project))
                        {
                            var newCode = rewriter.UpdateNamespaces(File.ReadAllText(item));
                            File.WriteAllText(item, newCode);
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
            return Path.Combine(projectRoot, config.Inplace ? "" : config.Output);
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
                if (!config.Inplace && new Uri(CombindPath(newRoot)).IsBaseOf(new Uri(item)))
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
        public static void CopySolution(string sourceFolder, string targetFolder)
        {
            Directory.CreateDirectory(targetFolder);
            Directory.Delete(targetFolder, recursive: true);
            Directory.CreateDirectory(targetFolder);
            var source = new DirectoryInfo(sourceFolder);
            var target = new DirectoryInfo(targetFolder);
            CopyFilesRecursively(source, target);
        }
        public static void CopyFilesRecursively(DirectoryInfo source, DirectoryInfo target)
        {
            foreach (DirectoryInfo dir in source.GetDirectories())
            {
                if (dir.Name == "bin" || dir.Name == "obj" || dir.Name.StartsWith("."))
                {
                    continue;
                }
                CopyFilesRecursively(dir, target.CreateSubdirectory(dir.Name)); 
            }
            foreach (FileInfo file in source.GetFiles())
            {
                var targetFile = Path.Combine(target.FullName, file.Name);
                Console.WriteLine($"Copying {file.FullName}");
                file.CopyTo(targetFile); 
            }
        }
    }
}