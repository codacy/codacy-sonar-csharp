using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using SonarAnalyzer.Common;

namespace CodacyCSharp.Analyzer.Runner
{
    public class DiagnosticsRunner
    {
        private readonly AnalyzerOptions options;
        private readonly ImmutableArray<DiagnosticAnalyzer> usedAnalyzers;
        private readonly ImmutableArray<DiagnosticAnalyzer> utilityAnalyzers;

        public DiagnosticsRunner(
            ImmutableArray<DiagnosticAnalyzer> usedAnalyzers,
            ImmutableArray<DiagnosticAnalyzer> utilityAnalyzers,
            AnalyzerAdditionalFile[] additionalFiles)
        {
            this.usedAnalyzers = usedAnalyzers;
            this.utilityAnalyzers = utilityAnalyzers;

            if (!(additionalFiles is null) && additionalFiles.Any())
            {
                options = new AnalyzerOptions(additionalFiles.OfType<AdditionalText>().ToImmutableArray());
            }
        }

        public Task<ImmutableArray<Diagnostic>> GetDiagnostics(Compilation compilation,
            CancellationToken cancellationToken)
        {
            var diagnosticsAnalyzers = usedAnalyzers.Union(utilityAnalyzers).ToImmutableArray();

            if (diagnosticsAnalyzers.IsDefaultOrEmpty)
            {
                return Task.FromResult(new Diagnostic[0].ToImmutableArray());
            }

            var compilationOptions =
                (CompilationOptions) new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary);
            compilationOptions = compilationOptions.WithSpecificDiagnosticOptions(
                diagnosticsAnalyzers.SelectMany(analyzer => analyzer.SupportedDiagnostics)
                    .Select(diagnostic =>
                        new KeyValuePair<string, ReportDiagnostic>(diagnostic.Id, ReportDiagnostic.Warn)));

            var modifiedCompilation = compilation.WithOptions(compilationOptions);
            var compilationWithAnalyzer = modifiedCompilation.WithAnalyzers(
                diagnosticsAnalyzers,
                options,
                cancellationToken);

            return compilationWithAnalyzer.GetAnalyzerDiagnosticsAsync(cancellationToken);
        }
    }
}
