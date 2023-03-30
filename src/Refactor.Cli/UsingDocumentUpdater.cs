using Microsoft.CodeAnalysis;

namespace Codibex.Refactor.Cli;

internal class UsingDocumentUpdater : DocumentUpdater
{
    private readonly string _oldUsing;
    private readonly string _newUsing;

    public UsingDocumentUpdater(string oldUsing, string newUsing)
    {
        _oldUsing = oldUsing;
        _newUsing = newUsing;
    }

    internal override async Task<Document> UpdateAsync(Document originalDocument)
    {
        var source = await originalDocument.GetSyntaxRootAsync();

        var rewriter = new UsingRewriter(_oldUsing, _newUsing);
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