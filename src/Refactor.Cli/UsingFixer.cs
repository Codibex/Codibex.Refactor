using Microsoft.CodeAnalysis;

namespace Codibex.Refactor.Cli;
internal class UsingFixer
{
    private readonly Solution _solution;

    internal UsingFixer(Solution solution)
    {
        _solution = solution;
    }
    internal async Task FixAsync(string? assemblyToReference, string oldUsing, string newUsing)
    {
        Project? projectToReference = GetProjectToReference(assemblyToReference);

        Action commit = () => { };

        commit += await AnalyzeProjectsAsync(projectToReference, oldUsing, newUsing);

        commit();
    }

    private async Task<Action> AnalyzeProjectsAsync(Project? projectToReference, string oldUsing, string newUsing)
    {
        Action commit = () => { };
        var documentAnalyzer = new DocumentAnalyzer(oldUsing, newUsing);
        foreach (Project solutionProject in _solution.Projects)
        {
            var projectAnalyzer = new ProjectAnalyzer(solutionProject, documentAnalyzer);
            var commitAction = await projectAnalyzer.AnalyzeAsync(projectToReference);
            if (commitAction is null)
            {
                continue;
            }

            commit += commitAction;
        }

        return commit;
    }

    private Project? GetProjectToReference(string? assemblyToReference) =>
        assemblyToReference is null
            ? null
            : _solution.Projects.FirstOrDefault(p => p.AssemblyName == assemblyToReference);

    private class ProjectAnalyzer
    {
        private readonly Project _project;
        private readonly DocumentAnalyzer _documentAnalyzer;

        public ProjectAnalyzer(Project project, DocumentAnalyzer documentAnalyzer)
        {
            _project = project;
            _documentAnalyzer = documentAnalyzer;
        }

        public async Task<Action?> AnalyzeAsync(Project? projectToReference)
        {
            var result = await AnalyzeDocumentsAsync();
            if (result.anyDocumentUpdated == false)
            {
                return null;
            }

            if (projectToReference is null)
            {
                return result.updateAction;
            }

            if (_project.AssemblyName == projectToReference.AssemblyName)
            {
                return result.updateAction;
            }

            if (result.anyDocumentUpdated == false)
            {
                return result.updateAction;
            }

            var addAction = AddProjectReference(projectToReference);
            if (addAction is null)
            {
                return result.updateAction;
            }

            Action commit = () => { };
            commit += result.updateAction;
            commit += addAction;

            return commit;
        }

        private Action? AddProjectReference(Project projectToReference)
        {
            var existingReference = _project.AllProjectReferences
                .FirstOrDefault(r => r.ProjectId == projectToReference.Id);

            if (existingReference != null)
            {
                return null;
            }

            var updatedProject = _project.AddProjectReference(new ProjectReference(projectToReference.Id));
            var newSolution = updatedProject.Solution;
            return () => newSolution.Workspace.TryApplyChanges(newSolution);
        }

        private async Task<(bool anyDocumentUpdated, Action updateAction)> AnalyzeDocumentsAsync()
        {
            Action commit = () => { };
            bool anyDocumentUpdated = false;
            foreach (Document projectDocument in _project.Documents)
            {
                var updateAction = await _documentAnalyzer.AnalyzeAsync(projectDocument);
                if (updateAction is null)
                {
                    continue;
                }

                commit += updateAction;
                anyDocumentUpdated = true;
            }

            return (anyDocumentUpdated, commit);
        }
    }

    private class DocumentAnalyzer
    {
        private readonly string _oldUsing;
        private readonly string _newUsing;

        public DocumentAnalyzer(string oldUsing, string newUsing)
        {
            _oldUsing = oldUsing;
            _newUsing = newUsing;
        }

        public async Task<Action?> AnalyzeAsync(Document document)
        {
            var source = await document.GetSyntaxRootAsync();

            var rewriter = new UsingRewriter(_oldUsing, _newUsing);
            var newSource = rewriter.Visit(source);

            if (newSource == source)
            {
                return null;
            }

            return () => File.WriteAllText(document.FilePath, newSource.ToFullString());
        }
    }
}
