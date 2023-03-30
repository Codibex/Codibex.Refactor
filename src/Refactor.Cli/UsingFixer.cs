using Microsoft.CodeAnalysis;

namespace Codibex.Refactor.Cli;

internal class UsingFixer : SolutionFixer
{
    private readonly string? _assemblyToReference;
    private readonly string _oldUsing;
    private readonly string _newUsing;

    internal UsingFixer(Solution originalSolution, string? assemblyToReference, string oldUsing, string newUsing) : base(originalSolution)
    {
        _assemblyToReference = assemblyToReference;
        _oldUsing = oldUsing;
        _newUsing = newUsing;
    }
    
    protected override async Task UpdateProjectsAsync()
    {
        Project? projectToReference = GetProjectToReference();

        var documentAnalyzer = new UsingDocumentUpdater(_oldUsing, _newUsing);
        foreach (ProjectId projectId in UpdatedSolution.ProjectIds)
        {
            Project solutionProject = UpdatedSolution.GetProject(projectId)!;

            var projectAnalyzer = new UsingProjectUpdater(solutionProject, documentAnalyzer, projectToReference);
            var updatedProject = await projectAnalyzer.UpdateAsync();

            if (solutionProject == updatedProject)
            {
                continue;
            }

            UpdatedSolution = updatedProject.Solution;
        }
    }

    private Project? GetProjectToReference() =>
        _assemblyToReference is null
            ? null
            : OriginalSolution.Projects.FirstOrDefault(p => p.AssemblyName == _assemblyToReference);

    
}
