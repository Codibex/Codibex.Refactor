using Microsoft.CodeAnalysis;

namespace Codibex.Refactor.Cli;

internal class UsingProjectUpdater : DefaultProjectUpdater
{
    private readonly Project? _projectToReference;

    public UsingProjectUpdater(Project originalProject, DocumentUpdater documentUpdater, Project? projectToReference) : base(originalProject, documentUpdater)
    {
        _projectToReference = projectToReference;
    }

    internal override async Task<Project> UpdateAsync()
    {
        await base.UpdateAsync();

        var hasDocumentChanges = UpdatedProject.GetChanges(OriginalProject).GetChangedDocuments().Any();

        // A reference update is not needed if no documents are changed
        if (hasDocumentChanges == false)
        {
            return UpdatedProject;
        }

        if (_projectToReference is null
            || OriginalProject.AssemblyName == _projectToReference.AssemblyName)
        {
            return UpdatedProject;
        }

        AddProjectReference(_projectToReference);
        return UpdatedProject;
    }

    private void AddProjectReference(Project projectToReference)
    {
        var existingReference = UpdatedProject!.AllProjectReferences
            .FirstOrDefault(r => r.ProjectId == projectToReference.Id);

        if (existingReference is null)
        {
            UpdatedProject = UpdatedProject.AddProjectReference(new ProjectReference(projectToReference.Id));
        }
    }
}