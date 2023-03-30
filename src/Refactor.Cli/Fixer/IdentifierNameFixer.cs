using Microsoft.CodeAnalysis;

namespace Codibex.Refactor.Cli.Fixer;

internal class IdentifierNameFixer : SolutionFixer
{
    private readonly string _oldIdentifierName;
    private readonly string _newIdentifierName;

    public IdentifierNameFixer(Solution originalSolution, string oldIdentifierName, string newIdentifierName) : base(originalSolution)
    {
        _oldIdentifierName = oldIdentifierName;
        _newIdentifierName = newIdentifierName;
    }

    protected override async Task UpdateProjectsAsync()
    {
        var documentAnalyzer = new IdentifierNameDocumentUpdater(_oldIdentifierName, _newIdentifierName);
        foreach (ProjectId projectId in UpdatedSolution.ProjectIds)
        {
            Project solutionProject = UpdatedSolution.GetProject(projectId)!;

            var projectAnalyzer = new DefaultProjectUpdater(solutionProject, documentAnalyzer);
            var updatedProject = await projectAnalyzer.UpdateAsync();

            if (solutionProject == updatedProject)
            {
                continue;
            }

            UpdatedSolution = updatedProject.Solution;
        }
    }
}