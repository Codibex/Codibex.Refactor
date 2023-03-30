using CommandLine;

namespace Codibex.Refactor.Cli;

internal enum CodeFixer
{
    Usings,
    LineEndings
}

internal class Options
{
    [Option('s', Required = true, HelpText = "Solution to analyze")]
    public string Solution { get; }

    [Option('f', Required = true, HelpText = "Fixer to use")]
    public CodeFixer CodeFixer { get; }

    [Option('r', Required = false, HelpText = "Project to reference")]
    public string? ProjectToReference { get; }

    [Option('o', Required = true, HelpText = "Old using to replace")]
    public string OldUsing { get; }

    [Option('n', Required = true, HelpText = "New using")]
    public string NewUsing { get; }

    public Options(string solution, CodeFixer codeFixer, string? projectToReference, string oldUsing, string newUsing)
    {
        Solution = solution;
        CodeFixer = codeFixer;
        ProjectToReference = projectToReference;
        OldUsing = oldUsing;
        NewUsing = newUsing;
    }
}
