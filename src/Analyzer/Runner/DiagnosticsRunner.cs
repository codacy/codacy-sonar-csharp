using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using SonarAnalyzer.Common;

namespace CodacyCSharp.Analyzer.Runner
{
    public class DiagnosticsRunner
    {
        private readonly AnalyzerOptions options;
        private readonly ImmutableArray<DiagnosticAnalyzer> usedAnalyzers;
        private readonly ImmutableArray<DiagnosticAnalyzer> utilityAnalyzers;
        private readonly ImmutableList<string> patternIds;

        public DiagnosticsRunner(
            ImmutableArray<DiagnosticAnalyzer> usedAnalyzers,
            ImmutableArray<DiagnosticAnalyzer> utilityAnalyzers,
            AnalyzerAdditionalFile[] additionalFiles,
            ImmutableList<string> patternIds)
        {
            this.usedAnalyzers = usedAnalyzers;
            this.utilityAnalyzers = utilityAnalyzers;
            this.patternIds = patternIds;

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
                        new KeyValuePair<string, ReportDiagnostic>(diagnostic.Id, GetReportDiagnosticForPattern(diagnostic.Id))));

            var modifiedCompilation = compilation.WithOptions(compilationOptions);
            var compilationWithAnalyzer = modifiedCompilation.WithAnalyzers(
                diagnosticsAnalyzers,
                options,
                cancellationToken);

            return FilterDiagnostics(modifiedCompilation, compilationWithAnalyzer, cancellationToken);
        }

        private static async Task<ImmutableArray<Diagnostic>> FilterDiagnostics(
            Compilation compilation,
            CompilationWithAnalyzers compilationWithAnalyzer,
            CancellationToken cancellationToken)
        {
            var diagnostics = await compilationWithAnalyzer.GetAnalyzerDiagnosticsAsync(cancellationToken);

            return diagnostics
                .Where(diagnostic => !IsS1125OnUnresolvedType(compilation, diagnostic, cancellationToken))
                .ToImmutableArray();
        }

        // S1125 can only tell `x == true` apart from a valid `bool?` comparison when the
        // type of `x` is known. Files are compiled in isolation, so types declared in other
        // files or packages are unresolved and the rule reports false positives on them.
        private static bool IsS1125OnUnresolvedType(
            Compilation compilation,
            Diagnostic diagnostic,
            CancellationToken cancellationToken)
        {
            var tree = diagnostic.Location.SourceTree;
            if (diagnostic.Id != "S1125" || tree is null)
            {
                return false;
            }

            var binaryExpression = tree.GetRoot(cancellationToken)
                .FindNode(diagnostic.Location.SourceSpan, getInnermostNodeForTie: true)
                .AncestorsAndSelf()
                .OfType<BinaryExpressionSyntax>()
                .FirstOrDefault();

            if (binaryExpression is null)
            {
                return false;
            }

            var semanticModel = compilation.GetSemanticModel(tree);

            return new[] { binaryExpression.Left, binaryExpression.Right }
                .Select(operand => semanticModel.GetTypeInfo(operand, cancellationToken).Type)
                .Any(type => type is null || type.TypeKind == TypeKind.Error);
        }

        private ReportDiagnostic GetReportDiagnosticForPattern(string diagnosticId)
        {
            // ignore diagnostic if user defined patterns and this is not part of them
            if (patternIds is null || patternIds.Contains(diagnosticId) || utilityAnalyzers.SelectMany(a => a.SupportedDiagnostics).Any(d => d.Id == diagnosticId))
            {
                return ReportDiagnostic.Warn;
            } 
            else
            {
                return ReportDiagnostic.Suppress;
            }
        }
    }
}