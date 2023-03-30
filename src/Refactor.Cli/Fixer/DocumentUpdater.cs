using Microsoft.CodeAnalysis;

namespace Codibex.Refactor.Cli.Fixer;

internal abstract class DocumentUpdater
{
    internal abstract Task<Document> UpdateAsync(Document originalDocument);
}