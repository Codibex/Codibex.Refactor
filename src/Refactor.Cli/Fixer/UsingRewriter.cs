using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Codibex.Refactor.Cli.Fixer;
internal class UsingRewriter : CSharpSyntaxRewriter
{
    private readonly string _oldUsing;

    private readonly NameSyntax _nameSyntax;

    public UsingRewriter(string oldUsing, string newUsing)
    {
        _oldUsing = oldUsing;
        _nameSyntax = SyntaxFactory.ParseName(newUsing);
    }

    public override SyntaxNode VisitUsingDirective(UsingDirectiveSyntax node)
    {
        if (node.Name is QualifiedNameSyntax qualifiedNameSyntax)
        {
            return _oldUsing == qualifiedNameSyntax.ToFullString()
                ? node.ReplaceNode(node, _nameSyntax)
                : node;
        }

        if (node.Name is IdentifierNameSyntax identifierNameSyntax)
        {
            return _oldUsing == identifierNameSyntax.ToFullString()
                ? node.ReplaceNode(node.Name, _nameSyntax)
                : node;
        }

        return node;
    }
}
