using Microsoft.CodeAnalysis;

namespace Codibex.Refactor.Cli;

internal abstract class SolutionFixer
{
    protected Solution OriginalSolution { get; }
    protected Solution UpdatedSolution { get; set; }

    protected SolutionFixer(Solution originalSolution)
    {
        OriginalSolution = originalSolution;
        UpdatedSolution = OriginalSolution;
    }
}