using Microsoft.CodeAnalysis;

namespace Codibex.Refactor.Cli;

internal abstract class DocumentUpdater
{
    internal abstract Task<Document> UpdateAsync(Document originalDocument);
}