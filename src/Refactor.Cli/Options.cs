using CommandLine;

namespace Codibex.Refactor.Cli;

internal class Options
{
    [Option('s', Required = true, HelpText = "Solution to analyze")]
    public string Solution { get; }

    [Option('f', Required = true, HelpText = "Fixer to use")]
    public CodeFixer CodeFixer { get; }

    [Option('r', Required = false, HelpText = "Project to reference")]
    public string? ProjectToReference { get; }

    [Option('o', Required = false, HelpText = "Old using or name to replace")]
    public string OldSymbol { get; }

    [Option('n', Required = false, HelpText = "New using or name")]
    public string NewSymbol { get; }

    public Options(string solution, CodeFixer codeFixer, string? projectToReference, string oldSymbol, string newSymbol)
    {
        Solution = solution;
        CodeFixer = codeFixer;
        ProjectToReference = projectToReference;
        OldSymbol = oldSymbol;
        NewSymbol = newSymbol;
    }
}
