using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.MSBuild;
namespace RoslynWeave
{
    class Program
    {
        const string WrapInFolder = "AopManaged";
        static async Task Main(string[] args)
        {
            try
            {
                var solutionPath = "../../../../RoslynWeave.sln";
                if (args.Length > 0)
                {
                    solutionPath = args[0];
                }
                using (var workspace = MSBuildWorkspace.Create())
                {
                    var config = new CodeRewriterConfig();
                    var rewriter = new CodeRewriter(config, new TemplateExtractor());
                    var solution = await workspace.OpenSolutionAsync(solutionPath);
                    foreach (var project in solution.Projects)
                    {
                        if (project.Name ==typeof(CodeRewriter).Namespace)
                        {
                            continue;
                        }
                        var projectRoot = Path.GetDirectoryName(project.FilePath);
                        var newRoot = GetWrappedPath(project);
                        foreach (var document in Directory.GetFiles(projectRoot, "*.cs", SearchOption.AllDirectories))
                        {
                            if (new Uri(CombindPath(newRoot)).IsBaseOf(new Uri(document)))
                            {
                                continue;
                            }
                            if (new Uri(CombindPath(projectRoot, "bin")).IsBaseOf(new Uri(document)))
                            {
                                continue;
                            }
                            if (new Uri(CombindPath(projectRoot, "obj")).IsBaseOf(new Uri(document)))
                            {
                                continue;
                            }
                            if (Path.GetFileName(document).ToLower() == "program.cs")
                            {
                                continue;
                            }
                            string newCode = null;
                            using (var sr = new StreamReader(document))
                            {
                                newCode = rewriter.Wrap(sr.ReadToEnd());
                            }
                            var relative = Path.GetRelativePath(projectRoot, document);
                            var newPath = Path.Combine(newRoot, relative);
                            Directory.CreateDirectory(Path.GetDirectoryName(newPath));
                            using (var sw = new StreamWriter(newPath))
                            {
                                sw.WriteLine("//RoslynWeave auto generated code.");
                                sw.Write(newCode);
                            }
                        }
                    }
                    foreach (var project in solution.Projects)
                    {
                        if (Directory.Exists(GetWrappedPath(project)))
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

        private static string GetWrappedPath(Project project)
        {
            var projectRoot = Path.GetDirectoryName(project.FilePath);
            return Path.Combine(projectRoot, WrapInFolder);
        }
    }

    public class MyContext: DefaultAopContext
    {
        public override void ExitFrame()
        {
            base.ExitFrame();
        }
    }
}
