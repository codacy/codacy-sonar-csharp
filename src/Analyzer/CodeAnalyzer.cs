using System;
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
        private static List<string> blacklist = new List<string> { "S1144", "S2325", "S2077"};

        public static bool IsInBlacklist(string id) {
            return blacklist.Contains(id);
        }

        public CodeAnalyzer() : base(csharpExtension)
        {
            sonarConfigurationPath = Path.Combine(DefaultSourceFolder, defaultSonarConfiguration);
            availableAnalyzers = ImmutableArray.Create(
                new RuleFinder()
                    .GetAnalyzerTypes()
                    .Select(type => (DiagnosticAnalyzer) Activator.CreateInstance(type))
                    .ToArray());

            var additionalFiles = new List<AnalyzerAdditionalFile>();

            if (!(PatternIds is null) && PatternIds.Any())
            {
                // create temporary directory
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
            } else if (File.Exists(sonarConfigurationPath)) {
                additionalFiles.Add(new AnalyzerAdditionalFile(sonarConfigurationPath));
            }

            // if we don't have patterns let's get them from the config file
            if (PatternIds is null && File.Exists(sonarConfigurationPath))
            {
                var xmlDoc = XDocument.Load(sonarConfigurationPath);
                var analysisInput = xmlDoc.Element("AnalysisInput");
                var rules = analysisInput.Element("Rules");
                if (rules != null)
                {
                    PatternIds = rules.Elements("Rule").Select(e => e.Elements("Key").Single().Value)
                        .ToImmutableList();
                }
            }

            diagnosticsRunner = new DiagnosticsRunner(GetAnalyzers(), GetUtilityAnalyzers(), additionalFiles.ToArray(), PatternIds);
        }

        public void Dispose()
        {
            Dispose(true);
        }

        /// <summary>
        ///     Free temporary resources created on object construction.
        /// </summary>
        ~CodeAnalyzer()
        {
            Dispose(false);
        }

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                GC.SuppressFinalize(this);
            }

            // delete created temporary directory
            if(tmpSonarLintFolder != null)
            {
                Directory.Delete(tmpSonarLintFolder, true);
            }
        }

        protected override async Task Analyze(CancellationToken cancellationToken)
        {
            foreach (var file in Config.Files) await Analyze(file, cancellationToken).ConfigureAwait(false);
        }

        public async Task Analyze(string file, CancellationToken cancellationToken)
        {
            try
            {
                var solution = CompilationHelper.GetSolutionFromFile(DefaultSourceFolder + file);
                var compilation = await solution.Projects.First().GetCompilationAsync();

                foreach (var diagnostic in await diagnosticsRunner.GetDiagnostics(compilation, cancellationToken))
                {
                    var result = new CodacyResult
                    {
                        Filename = file,
                        PatternId = diagnostic.Id,
                        Message = diagnostic.GetMessage()
                    };

                    if (diagnostic.Location != Location.None)
                    {
                        result.Line = diagnostic.Location.GetLineNumberToReport();
                    }
                    else
                    {
                        result.Line = 1;
                    }

                    if(!IsInBlacklist(diagnostic.Id)) {
                        Console.WriteLine(result);
                    }
                }
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
            var builder = ImmutableArray.CreateBuilder<DiagnosticAnalyzer>();

            foreach (var analyzer in availableAnalyzers
                .Where(analyzer => analyzer.SupportedDiagnostics.Any(diagnostic => PatternIds.Contains(diagnostic.Id))))
                builder.Add(analyzer);

            return builder.ToImmutable();
        }

        public static ImmutableArray<DiagnosticAnalyzer> GetUtilityAnalyzers()
        {
            var builder = ImmutableArray.CreateBuilder<DiagnosticAnalyzer>();
            var utilityAnalyzerTypes = RuleFinder.GetUtilityAnalyzerTypes()
                .Where(t => !t.IsAbstract)
                .ToList();

            foreach (var analyzer in utilityAnalyzerTypes
                .Select(type => (DiagnosticAnalyzer) Activator.CreateInstance(type)))
                builder.Add(analyzer);

            return builder.ToImmutable();
        }
    }
}
