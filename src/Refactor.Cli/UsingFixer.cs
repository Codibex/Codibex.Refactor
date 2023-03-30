using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Formatting;

namespace Codibex.Refactor.Cli;
internal class UsingFixer : SolutionFixer
{
    internal UsingFixer(Solution originalSolution) : base(originalSolution)
    {;
    }
    
    internal async Task FixAsync(string? assemblyToReference, string oldUsing, string newUsing)
    {
        Project? projectToReference = GetProjectToReference(assemblyToReference);
        
        await UpdateProjectsAsync(projectToReference, oldUsing, newUsing);
        OriginalSolution.Workspace.TryApplyChanges(UpdatedSolution);
    }

    private async Task UpdateProjectsAsync(Project? projectToReference, string oldUsing, string newUsing)
    {
        var documentAnalyzer = new DocumentUpdater(oldUsing, newUsing);
        foreach (ProjectId projectId in UpdatedSolution.ProjectIds)
        {
            Project solutionProject = UpdatedSolution.GetProject(projectId)!;

            var projectAnalyzer = new ProjectUpdater(solutionProject, documentAnalyzer);
            var updatedProject = await projectAnalyzer.UpdateAsync(projectToReference);

            if (updatedProject is null)
            {
                continue;
            }

            UpdatedSolution = updatedProject.Solution;
        }
    }

    private Project? GetProjectToReference(string? assemblyToReference) =>
        assemblyToReference is null
            ? null
            : OriginalSolution.Projects.FirstOrDefault(p => p.AssemblyName == assemblyToReference);

    private class ProjectUpdater
    {
        private readonly Project _originalProject;
        private Project _updatedProject;
        private readonly DocumentUpdater _documentUpdater;

        public ProjectUpdater(Project originalProject, DocumentUpdater documentUpdater)
        {
            _originalProject = originalProject;
            _updatedProject = originalProject;
            _documentUpdater = documentUpdater;
        }

        /// <summary>
        /// Updates all documents and the project references in a project
        /// </summary>
        /// <param name="projectToReference">Project to reference for the using update</param>
        /// <returns>A new project instance with the changes or null if no changes are made</returns>
        public async Task<Project?> UpdateAsync(Project? projectToReference)
        {
            await UpdateDocumentsAsync();

            var hasDocumentChanges = _updatedProject.GetChanges(_originalProject).GetChangedDocuments().Any();

            // A reference update is not needed if no documents are changed
            if (hasDocumentChanges == false)
            {
                return null;
            }

            if (projectToReference is null
                || _originalProject.AssemblyName == projectToReference.AssemblyName)
            {
                return _updatedProject;
            }

            AddProjectReference(projectToReference);
            return _updatedProject;
        }

        private async Task UpdateDocumentsAsync()
        {
            foreach (DocumentId documentId in _updatedProject.DocumentIds)
            {
                var projectDocument = _updatedProject.GetDocument(documentId)!;
                var updatedDocument = await _documentUpdater.UpdateAsync(projectDocument);
                
                if (updatedDocument is null)
                {
                    continue;
                }

                if (updatedDocument.SupportsSyntaxTree)
                {
                    var root = await updatedDocument.GetSyntaxRootAsync();
                    var options = await updatedDocument.GetOptionsAsync();

                    root = root!.ReplaceTrivia(root!.DescendantTrivia().Where(t => t.IsKind(SyntaxKind.EndOfLineTrivia)),
                        (o, r) => SyntaxFactory.ElasticMarker);

                    var update = Formatter.Format(root, updatedDocument.Project.Solution.Workspace, options);
                    updatedDocument = updatedDocument.WithSyntaxRoot(update);
                }
                _updatedProject = updatedDocument.Project;
            }
        }

        private void AddProjectReference(Project projectToReference)
        {
            var existingReference = _updatedProject!.AllProjectReferences
                .FirstOrDefault(r => r.ProjectId == projectToReference.Id);

            if (existingReference is null)
            {
                _updatedProject = _updatedProject.AddProjectReference(new ProjectReference(projectToReference.Id));
            }
        }
    }

    private class DocumentUpdater
    {
        private readonly string _oldUsing;
        private readonly string _newUsing;

        public DocumentUpdater(string oldUsing, string newUsing)
        {
            _oldUsing = oldUsing;
            _newUsing = newUsing;
        }

        /// <summary>
        /// Updates the using statements within the document 
        /// </summary>
        /// <param name="originalDocument">Original originalDocument</param>
        /// <returns>A new document if the using is updated</returns>
        public async Task<Document?> UpdateAsync(Document originalDocument)
        {
            var source = await originalDocument.GetSyntaxRootAsync();

            var rewriter = new UsingRewriter(_oldUsing, _newUsing);
            var newSource = rewriter.Visit(source);

            if (newSource is null)
            {
                return null;
            }

            return newSource == source 
                ? null 
                : originalDocument.WithSyntaxRoot(newSource);
        }
    }
}
