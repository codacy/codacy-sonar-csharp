using System.IO;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace CodacyCSharp.Analyzer.Runner
{
    public static class CompilationHelper
    {
        private static readonly MetadataReference systemMetadataReference =
            MetadataReference.CreateFromFile(typeof(object).Assembly.Location);

        public static Solution GetSolutionFromFile(string filePath)
        {
            using (var workspace = new AdhocWorkspace())
            {
                var file = new FileInfo(filePath);
                var project = workspace.CurrentSolution
                    .AddProject("srcassembly", "srcassembly.dll", LanguageNames.CSharp)
                    .AddMetadataReference(systemMetadataReference);

                using (var fileStream = File.Open(file.FullName, FileMode.Open, FileAccess.Read))
                {
                    var document = project.AddDocument(file.Name, SourceText.From(fileStream));
                    project = document.Project;
                    return project.Solution;
                }
            }
        }
    }
}
