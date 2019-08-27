using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using RoslynWeaveTemplate;

namespace RoslynWeave
{
    public class TemplateExtractor : CSharpSyntaxRewriter
    {
        public class MethodTemplate
        {
            public BlockSyntax Body { get; set; }
            public IList<SyntaxNode> OriginalBodies { get; } = new List<SyntaxNode>();
            public IList<SyntaxNode> DefaultReturns { get; } = new List<SyntaxNode>();
            public ObjectCreationExpressionSyntax MetaData {get;set;}
        }

        public MethodTemplate Sync { get; } = new MethodTemplate();
        public MethodTemplate Async { get; } = new MethodTemplate();
        public IList<string> Usings = new List<string>();
        private readonly Type _metadataType;

        public TemplateExtractor(Func<string> templateSource = null, Type metadataType = null)
        {
            _metadataType = metadataType ?? typeof(MethodMetadata);
            string sourceCode = null;
            if (templateSource == null)
            {
                using (var sr = new StreamReader(typeof(TemplateExtractor).Assembly
                    .GetManifestResourceStream("RoslynWeave.CodeRewriter.WrapperTemplate.cs")))
                {
                    sourceCode = sr.ReadToEnd();
                    
                }
            }
            else
            {
                sourceCode = templateSource();
            }
            var tree = CSharpSyntaxTree.ParseText(sourceCode);
            var root = tree.GetRoot();
            Visit(root);
            foreach (var item in ((CompilationUnitSyntax)root).Usings)
            {
                Usings.Add(item.Name.ToString());
            }
        }

        public override SyntaxNode VisitMethodDeclaration(MethodDeclarationSyntax node)
        {
            switch (node.Identifier.ValueText)
            {
                case nameof(WrapperTemplate.SyncMethod):
                    Sync.Body = node.Body;
                    AddPlaceHolderStatements(Sync, node);
                    break;
                case nameof(WrapperTemplate.AsyncMethod):
                    Async.Body = node.Body;
                    AddPlaceHolderStatements(Async, node);
                    break;
                default:
                    break;
            }

            return base.VisitMethodDeclaration(node);
        }

        private void AddPlaceHolderStatements(MethodTemplate template, MethodDeclarationSyntax node)
        {
            foreach (var item in node.DescendantNodes().OfType<InvocationExpressionSyntax>())
            {
                switch (item.Expression.ToString())
                {
                    case nameof(WrapperTemplate.Body):
                        template.OriginalBodies.Add(item.Parent);
                        break;
                    case nameof(WrapperTemplate.Default):
                        template.DefaultReturns.Add(item.Parent);
                        break;
                }
            }
            foreach (var item in node.DescendantNodes().OfType<ObjectCreationExpressionSyntax>())
            {
                if (item.Type.ToString() == _metadataType.Name)
                {
                    template.MetaData = item;
                    break;
                }
            }
        }
    }
}
