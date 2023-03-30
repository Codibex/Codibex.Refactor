using Microsoft.CodeAnalysis;

namespace Codibex.Refactor.Cli.Fixer;

internal class LineEndingFixer : SolutionFixer
{
    public LineEndingFixer(Solution originalSolution) : base(originalSolution)
    {
    }

    protected override async Task UpdateProjectsAsync()
    {

        var documentAnalyzer = new LineEndingsDocumentUpdater();
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