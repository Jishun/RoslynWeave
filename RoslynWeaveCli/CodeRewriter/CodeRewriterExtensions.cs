using System;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace RoslynWeave
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
    }
}
