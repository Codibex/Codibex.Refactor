using Microsoft.CodeAnalysis;

namespace Codibex.Refactor.Cli.Fixer;

internal abstract class SolutionFixer
{
    protected Solution OriginalSolution { get; }
    protected Solution UpdatedSolution { get; set; }

    protected SolutionFixer(Solution originalSolution)
    {
        OriginalSolution = originalSolution;
        UpdatedSolution = OriginalSolution;
    }

    internal async Task<Solution> FixAsync()
    {
        await UpdateProjectsAsync();
        return UpdatedSolution;
    }

    protected abstract Task UpdateProjectsAsync();
}