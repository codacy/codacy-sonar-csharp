using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using CodacyCSharp.Analyzer.Runner;
using CodacyCSharp.Analyzer.Utilities;
using Codacy.Engine.Seed;
using Codacy.Engine.Seed.Results;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using SonarAnalyzer.Common;
using SonarAnalyzer.Helpers;

namespace CodacyCSharp.Analyzer
{
    public class CodeAnalyzer : Codacy.Engine.Seed.CodeAnalyzer, IDisposable
    {
        private const string csharpExtension = ".cs";
        private const string defaultSonarConfiguration = "SonarLint.xml";
        private readonly string sonarConfigurationPath;
        private readonly ImmutableArray<DiagnosticAnalyzer> availableAnalyzers;
        private readonly DiagnosticsRunner diagnosticsRunner;
        private readonly string tmpSonarLintFolder;
        private static HashSet<string> blacklist = new HashSet<string> { "S1144", "S2325", "S2077" };

        public static bool IsInBlacklist(string id)
        {
            return blacklist.Contains(id);
        }

        public CodeAnalyzer() : base(csharpExtension)
        {
            sonarConfigurationPath = Path.Combine(DefaultSourceFolder, defaultSonarConfiguration);
            availableAnalyzers = ImmutableArray.Create(
                new RuleFinder()
                    .GetAnalyzerTypes()
                    .Select(type => (DiagnosticAnalyzer)Activator.CreateInstance(type))
                    .ToArray());

            var additionalFiles = new List<AnalyzerAdditionalFile>();

            if (!(PatternIds is null) && PatternIds.Any())
            {
                // create temporary directory only if needed
                this.tmpSonarLintFolder = Path.Combine(Path.GetTempPath(), "sonarlint_" + Guid.NewGuid());
                Directory.CreateDirectory(tmpSonarLintFolder);
                var tmpSonarLintPath = Path.Combine(tmpSonarLintFolder, defaultSonarConfiguration);

                var rules = new XElement("Rules");
                var patterns = CurrentTool.Patterns.Where(pattern => !IsInBlacklist(pattern.PatternId)).ToArray();

                foreach (var pattern in patterns)
                {
                    var parameters = new XElement("Parameters");
                    if (!(pattern.Parameters is null))
                    {
                        foreach (var parameter in pattern.Parameters)
                        {
                            var key = new XElement("Key", parameter.Name);
                            var value = new XElement("Value", parameter.Value);
                            parameters.Add(new XElement("Parameter", key, value));
                        }
                    }

                    rules.Add(new XElement("Rule", new XElement("Key", pattern.PatternId), parameters));
                }

                new XDocument(new XElement("AnalysisInput", rules)).Save(tmpSonarLintPath);
                additionalFiles.Add(new AnalyzerAdditionalFile(tmpSonarLintPath));
            }
            else if (File.Exists(sonarConfigurationPath))
            {
                var configurationFile = XDocument.Load(sonarConfigurationPath).Element("AnalysisInput");
                additionalFiles.Add(new AnalyzerAdditionalFile(sonarConfigurationPath));
                
                // Load patterns from cached configuration file
                var rules = configurationFile.Element("Rules");
                if (rules != null)
                {
                    PatternIds = rules.Elements("Rule").Select(e => e.Elements("Key").Single().Value).ToImmutableList();
                }
            }

            diagnosticsRunner = new DiagnosticsRunner(GetAnalyzers(), GetUtilityAnalyzers(), additionalFiles.ToArray(), PatternIds);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~CodeAnalyzer()
        {
            Dispose(false);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (tmpSonarLintFolder != null)
            {
                Directory.Delete(tmpSonarLintFolder, true);
            }
        }

        protected override async Task Analyze(CancellationToken cancellationToken)
        {
            // Process files concurrently for better performance
            var fileAnalysisTasks = Config.Files
                .Where(file => file.EndsWith(".cs")) // only analyze C# files
                .Select(file => Analyze(file, cancellationToken));

            await Task.WhenAll(fileAnalysisTasks);
        }

        public async Task Analyze(string file, CancellationToken cancellationToken)
        {
            try
            {
                var solution = CompilationHelper.GetSolutionFromFile(DefaultSourceFolder + file);
                var compilation = await solution.Projects.First().GetCompilationAsync();

                // Parallelize diagnostics fetching
                var diagnostics = await diagnosticsRunner.GetDiagnostics(compilation, cancellationToken);

                Parallel.ForEach(diagnostics, diagnostic =>
                {
                    var result = new CodacyResult
                    {
                        Filename = file,
                        PatternId = diagnostic.Id,
                        Message = diagnostic.GetMessage(),
                        Line = diagnostic.Location != Location.None ? diagnostic.Location.GetLineNumberToReport() : 1
                    };

                    if (!IsInBlacklist(diagnostic.Id))
                    {
                        Console.WriteLine(result);
                    }
                });
            }
            catch (Exception e)
            {
                Logger.Send(e);

                Console.WriteLine(new CodacyResult
                {
                    Filename = file,
                    Message = "could not parse the file"
                });
            }
        }

        public ImmutableArray<DiagnosticAnalyzer> GetAnalyzers()
        {
            if (PatternIds is null)
            {
                return availableAnalyzers;
            }

            // Initialize only relevant analyzers
            var relevantAnalyzers = availableAnalyzers
                .Where(analyzer => analyzer.SupportedDiagnostics.Any(diagnostic => PatternIds.Contains(diagnostic.Id)))
                .ToImmutableArray();

            return relevantAnalyzers;
        }

        public static ImmutableArray<DiagnosticAnalyzer> GetUtilityAnalyzers()
        {
            var utilityAnalyzerTypes = RuleFinder.GetUtilityAnalyzerTypes()
                .Where(t => !t.IsAbstract)
                .ToList();

            var utilityAnalyzers = utilityAnalyzerTypes
                .Select(type => (DiagnosticAnalyzer)Activator.CreateInstance(type))
                .ToImmutableArray();

            return utilityAnalyzers;
        }
    }
}
