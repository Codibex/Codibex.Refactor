using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Formatting;

namespace Codibex.Refactor.Cli;

internal class LineEndingsDocumentUpdater : DocumentUpdater
{
    internal override async Task<Document> UpdateAsync(Document originalDocument)
    {
        if (originalDocument.SupportsSyntaxTree == false)
        {
            return originalDocument;
        }

        var root = await originalDocument.GetSyntaxRootAsync();
        var options = await originalDocument.GetOptionsAsync();

        root = root!.ReplaceTrivia(root!.DescendantTrivia().Where(t => t.IsKind(SyntaxKind.EndOfLineTrivia)),
            (o, r) => SyntaxFactory.ElasticMarker);

        var update = Formatter.Format(root, originalDocument.Project.Solution.Workspace, options);
        return originalDocument.WithSyntaxRoot(update);

    }
}