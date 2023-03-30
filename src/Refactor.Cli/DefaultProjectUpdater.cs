using Microsoft.CodeAnalysis;

namespace Codibex.Refactor.Cli;

internal class DefaultProjectUpdater : ProjectUpdater
{
    public DefaultProjectUpdater(Project originalProject, DocumentUpdater documentUpdater) : base(originalProject, documentUpdater)
    {
    }

    internal override async Task<Project> UpdateAsync()
    {
        foreach (DocumentId documentId in UpdatedProject.DocumentIds)
        {
            var projectDocument = UpdatedProject.GetDocument(documentId);
            if (projectDocument is null)
            {
                continue;
            }

            var updatedDocument = await DocumentUpdater.UpdateAsync(projectDocument);
            if (projectDocument == updatedDocument)
            {
                continue;
            }

            UpdatedProject = updatedDocument.Project;
        }

        return UpdatedProject;
    }
}