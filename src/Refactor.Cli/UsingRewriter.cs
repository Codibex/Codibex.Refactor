using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Codibex.Refactor.Cli;
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
        if (node.Name is QualifiedNameSyntax qualifiedNameSyntax == false)
        {
            return node;
        }

        if (_oldUsing != qualifiedNameSyntax.ToFullString())
        {
            return node;
        }

        return node.ReplaceNode(node, _nameSyntax);
    }
}
