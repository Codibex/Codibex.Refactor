using Microsoft.CodeAnalysis;

namespace Codibex.Refactor.Cli.Fixer;

internal abstract class ProjectUpdater
{
    protected Project OriginalProject { get; }
    protected Project UpdatedProject { get; set; }
    protected DocumentUpdater DocumentUpdater { get; }


    protected ProjectUpdater(Project originalProject, DocumentUpdater documentUpdater)
    {
        OriginalProject = originalProject;
        UpdatedProject = OriginalProject;

        DocumentUpdater = documentUpdater;
    }

    internal abstract Task<Project> UpdateAsync();
}