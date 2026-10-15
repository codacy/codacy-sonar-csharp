using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace CodacyCSharp.Analyzer.Runner
{
    public static class CompilationHelper
    {
        // Reference the whole runtime framework (not only System.Private.CoreLib),
        // so types like System.Net.HttpStatusCode resolve and rules relying on
        // the semantic model (e.g. S1128) don't report false positives.
        private static readonly IReadOnlyList<MetadataReference> frameworkMetadataReferences =
            GetFrameworkMetadataReferences();

        private static IReadOnlyList<MetadataReference> GetFrameworkMetadataReferences()
        {
            var trustedAssemblies = AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES") as string;
            var paths = string.IsNullOrEmpty(trustedAssemblies)
                ? new[] { typeof(object).Assembly.Location }
                : trustedAssemblies.Split(Path.PathSeparator);

            var frameworkDirectory = Path.GetDirectoryName(typeof(object).Assembly.Location);

            return paths
                // keep only framework assemblies, not the analyzer's own dependencies
                .Where(path => Path.GetDirectoryName(path) == frameworkDirectory)
                .Select(path => (MetadataReference)MetadataReference.CreateFromFile(path))
                .ToList();
        }

        public static Solution GetSolutionFromFile(string filePath)
        {
            using (var workspace = new AdhocWorkspace())
            {
                var file = new FileInfo(filePath);
                var project = workspace.CurrentSolution
                    .AddProject("srcassembly", "srcassembly.dll", LanguageNames.CSharp)
                    .AddMetadataReferences(frameworkMetadataReferences);

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
