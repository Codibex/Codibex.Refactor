using Microsoft.CodeAnalysis;

namespace Codibex.Refactor.Cli.Fixer;
internal class IdentifierNameDocumentUpdater : DocumentUpdater
{
    private readonly string _oldSymbol;
    private readonly string _newSymbol;

    public IdentifierNameDocumentUpdater(string oldSymbol, string newSymbol)
    {
        _oldSymbol = oldSymbol;
        _newSymbol = newSymbol;
    }
    internal override async Task<Document> UpdateAsync(Document originalDocument)
    {
        var source = await originalDocument.GetSyntaxRootAsync();

        var rewriter = new IdentifierNameRewriter(_oldSymbol, _newSymbol);
        var newSource = rewriter.Visit(source);

        if (newSource is null)
        {
            return originalDocument;
        }

        return newSource == source
            ? originalDocument
            : originalDocument.WithSyntaxRoot(newSource);
    }
}
