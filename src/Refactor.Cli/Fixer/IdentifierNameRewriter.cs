using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Codibex.Refactor.Cli.Fixer;

internal class IdentifierNameRewriter : CSharpSyntaxRewriter
{
    private readonly string _oldIdentifierName;

    private readonly IdentifierNameSyntax _identifierNameSyntax;

    public IdentifierNameRewriter(string oldIdentifierName, string newIdentifierName)
    {
        _oldIdentifierName = oldIdentifierName;
        _identifierNameSyntax = SyntaxFactory.IdentifierName(newIdentifierName);
    }

    public override SyntaxNode? VisitIdentifierName(IdentifierNameSyntax node)
    {
        return _oldIdentifierName == node.Identifier.ToFullString()
            ? node.ReplaceNode(node, _identifierNameSyntax)
            : node;
    }
}