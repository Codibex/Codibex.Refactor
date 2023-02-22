using CommandLine;

namespace Codibex.Refactor.Cli;
internal class Options
{
    [Option('s', Required = true, HelpText = "Solution to analyze")]
    public string Solution { get; }

    [Option('r', Required = false, HelpText = "Project to reference")]
    public string? ProjectToReference { get; }

    [Option('o', Required = true, HelpText = "Old using to replace")]
    public string OldUsing { get; }

    [Option('n', Required = true, HelpText = "New using")]
    public string NewUsing { get; }

    public Options(string solution, string? projectToReference, string oldUsing, string newUsing)
    {
        Solution = solution;
        ProjectToReference = projectToReference;
        OldUsing = oldUsing;
        NewUsing = newUsing;
    }
}
