using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace RoslynWeave.CodeReWriter
{
    public class CodeRewriter : CSharpSyntaxRewriter
    {
        public string ComponnetName { get; private set; }
        private readonly TemplateExtractor Templates = new TemplateExtractor();
        private readonly CodeRewriterConfig _config;
        private readonly RoslynWeaveLogger _logger;

        public IDictionary<string, string> NameSpaces = new Dictionary<string, string>();

        public CodeRewriter(CodeRewriterConfig config, TemplateExtractor extractor)
        {
            _config = config;
            _logger = new RoslynWeaveLogger(_config.Logger);
            Templates = extractor;
        }

        public string Wrap(string code)
        {
            var tree = CSharpSyntaxTree.ParseText(code);
            var root = tree.GetRoot();
            return Wrap(root);
        }

        public string Wrap(SyntaxNode root)
        {
            foreach (var item in Templates.Usings)
            {
                root = ((CompilationUnitSyntax)root).AddUsingIfNotExist(item);
            }
            var newNode = Visit(root);
            return newNode.NormalizeWhitespace().ToFullString();
        }

        public string UpdateNamespaces(string code)
        {
            var tree = CSharpSyntaxTree.ParseText(code);
            var root = tree.GetRoot();
            var unit = ((CompilationUnitSyntax)root);
            unit = unit
                .WithUsings(
                    new SyntaxList<UsingDirectiveSyntax>(
                        unit.Usings.Select(
                            u => u.WithName(
                                    NameSpaces.ContainsKey(u.Name.ToString()) ? IdentifierName(NameSpaces[u.Name.ToString()]) : u.Name))));
            return unit.NormalizeWhitespace().ToFullString();
        }

        public override SyntaxNode VisitNamespaceDeclaration(NamespaceDeclarationSyntax node)
        {
            var newNode = node;
            var identifier = node.Name.DescendantNodes().OfType<IdentifierNameSyntax>().First().Identifier;
            var newIdentifier = CreateAopIdentifier(identifier, _config.NameSpaceSuffix);

            NameSpaces[identifier.ValueText] = newIdentifier.ValueText;

            newNode = node.WithName(node.Name.ReplaceToken(identifier, newIdentifier));
            return base.VisitNamespaceDeclaration(newNode);
        }

        public override SyntaxNode VisitClassDeclaration(ClassDeclarationSyntax node)
        {
            ComponnetName = ComponnetName ?? node.Identifier.ToFullString();
            if (node.IsIgnored())
            {
                _logger.WriteLine($"Node {node.Identifier.ToFullString()} is skipped due to attribute ignore");
                return node;
            }
            var newIdentifier = CreateAopIdentifier(node.Identifier);
            var newNode = node.ReplaceToken(node.Identifier, newIdentifier);
            return base.VisitClassDeclaration(newNode);
        }

        public override SyntaxNode VisitStructDeclaration(StructDeclarationSyntax node)
        {
            ComponnetName = ComponnetName ?? node.Identifier.ToFullString();
            if (node.IsIgnored())
            {
                _logger.WriteLine($"Node {node.Identifier.ToFullString()} is skipped due to attribute ignore");
                return node;
            }
            var newIdentifier = CreateAopIdentifier(node.Identifier);
            var newNode = node.ReplaceToken(node.Identifier, newIdentifier);
            return base.VisitStructDeclaration(newNode);
        }

        public override SyntaxNode VisitConstructorDeclaration(ConstructorDeclarationSyntax node)
        {
            if (!_config.IncludeConstructors)
            {
                return node;
            }
            if (node.IsIgnored())
            {
                _logger.WriteLine($"Node {node.Identifier.ToFullString()} is skipped due to attribute ignore");
                return node;
            }
            var arguments = node.ParameterList.Parameters.Select(p => p.Identifier.ValueText);
            var newIdentifier = CreateAopIdentifier(node.Identifier);
            var newNode = node.ReplaceToken(node.Identifier, newIdentifier)
                .WithBody(WrapMethodBody(node.Body, node.ParameterList, false, defaultReturn: "return;"));
            return base.VisitConstructorDeclaration(newNode);
        }

        public override SyntaxNode VisitMethodDeclaration(MethodDeclarationSyntax node)
        {
            if (node.IsIgnored())
            {
                _logger.WriteLine($"Node {node.Identifier.ToFullString()} is skipped due to attribute ignore");
                return node;
            }
            if (node.Body == null)
            {
                return node;
            }
            if (node.DescendantNodes().OfType<YieldStatementSyntax>().Any())
            {
                return node;
            }
            var methodName = node.Identifier.ValueText;
            var returnType = node.ReturnType.ToString();
            var genericReturnType = node.ReturnType as GenericNameSyntax;
            var isAsync = node.Modifiers.Any(x => x.IsKind(SyntaxKind.AsyncKeyword));
            var isVoid = returnType == "void";
            var isTask = returnType == "Task";
            var isGenericTask = genericReturnType?.Identifier.ValueText == "Task";
            string returnStatement = "return;";
            switch((isVoid, isTask, isAsync, isGenericTask))
            {
                case (false, false, true, true):
                case (false, false, false, false):
                    returnStatement = $"return default;";
                    break;
                case (false, false, false, true):
                    returnStatement = $"return Task.FromResult(default({genericReturnType.TypeArgumentList.Arguments[0].GetText().ToString().Trim()}));";
                    break;
                case (false, true, false, false):
                    returnStatement = $"return Task.CompletedTask;";
                    break;
                default:
                    break;
            }
            var newNode = node.WithBody(WrapMethodBody(node.Body, node.ParameterList, isAsync, returnStatement));
            return base.VisitMethodDeclaration(newNode).NormalizeWhitespace();
        }

        private BlockSyntax WrapMethodBody(BlockSyntax original, ParameterListSyntax parameters, bool async, string defaultReturn)
        {
            if (!original.Statements.Any())
            {
                return original;
            }
            if (!_config.UseAsyncIntercepters)
            {
                async = false;
            }
            var template = (async ? Templates.Async : Templates.Sync);
            var ps = string.Join(',', parameters.Parameters
                .Where(p => !p.Modifiers.Any(m => m.IsKind(SyntaxKind.OutKeyword)))
                .Select(p => $"({($"\"{p.Identifier.ValueText}\"",$"(object){p.Identifier.ValueText} ?? typeof({p.Type})")})"));
            var outs = string.Join(' ', parameters.Parameters.Where(p => p.Modifiers.Any(m => m.IsKind(SyntaxKind.OutKeyword)))
                .Select(p => $"{p.Identifier.ValueText} = default;"));
            var defaultStatement = ParseStatement($"{outs}{defaultReturn}");
            var nodes = template.OriginalBodies.Concat(template.DefaultReturns);
            if (template.MetaData != null)
            {
                nodes = nodes.Prepend(template.MetaData);
            }
            var ret = template.Body.TrackNodes(nodes);
            ret = ret.ReplaceNode(ret.GetCurrentNode(template.MetaData), template.MetaData.AddArgumentListArguments(ParseArgumentList(ps).Arguments.ToArray()));
            foreach (var item in template.OriginalBodies)
            {
                ret = ret.ReplaceNode(ret.GetCurrentNode(item), original.Statements);
            }
            foreach (var item in template.DefaultReturns)
            {
                ret = ret.ReplaceNode(ret.GetCurrentNode(item), defaultStatement);
            }
            return ret;
        }

        private static SyntaxToken CreateAopIdentifier(SyntaxToken originalIdentifier, string suffix = null)
        {
            return Identifier(originalIdentifier.ValueText + suffix);
        }
    }
}
