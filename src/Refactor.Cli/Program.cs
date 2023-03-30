using CommandLine;
using Microsoft.Build.Locator;
using Microsoft.CodeAnalysis.MSBuild;

namespace Codibex.Refactor.Cli;

internal class Program
{
    private static string[] OverrideArgs =>
        new[]
        {
            "-s..\\..\\..\\..\\..\\test\\RefactorTest\\RefactorTest.sln",
            "-rRefactorTest.AnotherProj",
            "-oRefactorTest",
            "-nRefactorTest.AnotherProj"
        };

    public static async Task Main(string[] args)
    {
        //comment out this line for test
        args = OverrideArgs;
        
        // Attempt to set the version of MSBuild.
        var visualStudioInstances = MSBuildLocator.QueryVisualStudioInstances().ToArray();
        var instance = visualStudioInstances.Length == 1
            // If there is only one instance of MSBuild on this machine, set that as the one to use.
            ? visualStudioInstances[0]
            // Handle selecting the version of MSBuild you want to use.
            : SelectVisualStudioInstance(visualStudioInstances);

        Console.WriteLine($"Using MSBuild at '{instance.MSBuildPath}' to load projects.");

        // NOTE: Be sure to register an instance with the MSBuildLocator 
        //       before calling MSBuildWorkspace.Create()
        //       otherwise, MSBuildWorkspace won't MEF compose.
        MSBuildLocator.RegisterInstance(instance);

        using var workspace = MSBuildWorkspace.Create();
        // Print message for WorkspaceFailed event to help diagnosing project load failures.
        workspace.WorkspaceFailed += (o, e) => Console.WriteLine(e.Diagnostic.Message);

        var result = await Parser.Default.ParseArguments<Options>(args)
            .WithParsedAsync(async o => {
                var solutionPath = o.Solution;
                Console.WriteLine($"Loading solution '{solutionPath}'");

                // Attach progress reporter so we print projects as they are loaded.
                var solution = await workspace.OpenSolutionAsync(solutionPath, new ConsoleProgressReporter());
            
                Console.WriteLine($"Finished loading solution '{solutionPath}'");
                var updatedSolution = await new LineEndingFixer(solution).FixAsync();
                solution.Workspace.TryApplyChanges(updatedSolution);

                await new UsingFixer(solution).FixAsync(o.ProjectToReference, o.OldUsing, o.NewUsing);
            });
    }

    private static VisualStudioInstance SelectVisualStudioInstance(IReadOnlyList<VisualStudioInstance> visualStudioInstances)
    {
        Console.WriteLine("Multiple installs of MSBuild detected please select one:");
        for (int i = 0; i < visualStudioInstances.Count; i++)
        {
            Console.WriteLine($"Instance {i + 1}");
            Console.WriteLine($"    Name: {visualStudioInstances[i].Name}");
            Console.WriteLine($"    Version: {visualStudioInstances[i].Version}");
            Console.WriteLine($"    MSBuild Path: {visualStudioInstances[i].MSBuildPath}");
        }

        while (true)
        {
            var userResponse = Console.ReadLine();
            if (int.TryParse(userResponse, out int instanceNumber) &&
                instanceNumber > 0 &&
                instanceNumber <= visualStudioInstances.Count)
            {
                return visualStudioInstances[instanceNumber - 1];
            }
            Console.WriteLine("Input not accepted, try again.");
        }
    }

    private class ConsoleProgressReporter : IProgress<ProjectLoadProgress>
    {
        public void Report(ProjectLoadProgress loadProgress)
        {
            var projectDisplay = Path.GetFileName(loadProgress.FilePath);
            if (loadProgress.TargetFramework != null)
            {
                projectDisplay += $" ({loadProgress.TargetFramework})";
            }

            Console.WriteLine($"{loadProgress.Operation,-15} {loadProgress.ElapsedTime,-15:m\\:ss\\.fffffff} {projectDisplay}");
        }
    }
}