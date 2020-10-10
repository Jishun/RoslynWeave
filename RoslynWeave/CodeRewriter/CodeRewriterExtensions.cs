using System;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace RoslynWeave.CodeReWriter
{
    public static class CodeRewriterExtensions
    {
        public static CompilationUnitSyntax AddUsingIfNotExist(this CompilationUnitSyntax node, string name)
        {
            if (node.Usings.Any(u => u.Name.ToString() == name))
            {
                return node;
            }
            return node.AddUsings(UsingDirective(IdentifierName(name)));
        }

        public static bool IsIgnored(this MemberDeclarationSyntax node)
        {
            if (node.AttributeLists.Count > 0 &&
                node.AttributeLists.First().Attributes.Count > 0)
            {
                if (node.AttributeLists.First().Attributes.Any(a => a.Name.ToFullString().StartsWith(nameof(AopIgnoreAttribute).Replace("Attribute", ""))))
                {
                    return true;
                }
            }
            return false;
        }

    }
}
