using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace RoslynWeave.CodeReWriter
{
    [Generator]
    public class Intercepter : ISourceGenerator
    {
        int index = 0;
        string DiagnoseLogPath;
        CodeRewriterConfig config;
        CodeRewriter rewriter;
        GeneratorExecutionContext currentContext;
        IList<string> newSourceCode = new List<string>();
        public void Execute(GeneratorExecutionContext context)
        {
            currentContext = context;
            try
            {
                if (context.Compilation.SyntaxTrees.Any())
                {
                    DiagnoseLogPath = Path.Combine(Path.GetDirectoryName(context.Compilation.SyntaxTrees.First().FilePath), "../aop/");
                }

                var syntaxReceiver = (RoslynWeaveSyntaxReceiver)context.SyntaxReceiver;
                foreach (var rootNode in syntaxReceiver.RootNodes)
                {
                    var beginning = rootNode.DescendantNodes().FirstOrDefault()?.ToFullString();
                    if (beginning.Contains("<auto") && beginning.Contains("generated"))
                    {
                        File.WriteAllText($"{DiagnoseLogPath}unmapped_{index++}.txt", rootNode.ToFullString());
                        continue;
                    }
                    var newCode = rewriter.Wrap(rootNode);
                    newSourceCode.Add(newCode);

                }
                foreach (var item in newSourceCode)
                {
                    var newCode = rewriter.UpdateNamespaces(item);
                    var sourceText = SourceText.From(newCode, Encoding.UTF8);
                    context.AddSource($"AopManaged_{index++}.cs", sourceText);
                    File.WriteAllText($"{DiagnoseLogPath}output_{index++}.cs", newCode);
                }
            }
            catch (Exception ex)
            {
                context.ReportDiagnostic(Diagnostic.Create((new DiagnosticDescriptor(id: "ROSLYNWEAVEEXECUTE",
                                            title: "Unexpected error while wrapping AOP code",
                                            messageFormat: "Error during execution '{0}': '{1}'.",
                                            category: "RoslynWeave CodeGenerator",
                                            DiagnosticSeverity.Error,
                                            isEnabledByDefault: true)
                                        ), Location.None, typeof(Intercepter).FullName, ex.ToString()));
            }
        }

        public void Initialize(GeneratorInitializationContext context)
        {
            try
            {
                config = new CodeRewriterConfig() { Logger = Log};
                rewriter = new CodeRewriter(config, new TemplateExtractor());
                context.RegisterForSyntaxNotifications(() => new RoslynWeaveSyntaxReceiver());

            }
            catch (Exception ex)
            {
                //context.ReportDiagnostic(Diagnostic.Create((new DiagnosticDescriptor(id: "ROSLYNWEAVEINIT",
                //                            title: "Unexpected error while wrapping AOP code",
                //                            messageFormat: "Error during execution '{0}': '{1}'.",
                //                            category: "RoslynWeave CodeGenerator",
                //                            DiagnosticSeverity.Warning,
                //                            isEnabledByDefault: true)
                //                        ), Location.None, typeof(Intercepter).FullName, ex.ToString()));
                //Seems no where to report so:
                File.AppendAllLines($"error.txt", new[] {"Error initializing code generator", ex.ToString() });
            }
        }

        private void Log(string message)
        {
            currentContext.ReportDiagnostic(Diagnostic.Create((new DiagnosticDescriptor(id: "ROSLYNWEAVEREWRITER",
                                            title: "Information while wrapping AOP code",
                                            messageFormat: "'{0}': '{1}'.",
                                            category: "RoslynWeave Code rewriter",
                                            DiagnosticSeverity.Info,
                                            isEnabledByDefault: true)
                                        ), Location.None, typeof(Intercepter).FullName, message));
        }
    }

    class RoslynWeaveSyntaxReceiver : ISyntaxReceiver
    {
        public IList<SyntaxNode> RootNodes { get; private set; } = new List<SyntaxNode>();

        public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
        {
            if (syntaxNode.Kind() == SyntaxKind.CompilationUnit)
            {
                RootNodes.Add(syntaxNode);
            }
        }
    }
}
