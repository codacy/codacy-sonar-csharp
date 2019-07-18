using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CodacyCSharp.Seed.Configuration;
using CodacyCSharp.Seed.Patterns;
using Newtonsoft.Json;

namespace CodacyCSharp.Seed
{
    public abstract class CodeAnalyzer
    {
        protected const string DefaultConfigFile = ".codacyrc";
        protected const string DefaultPatternsFile = "docs/patterns.json";
        protected const string DefaultSourceFolder = "src/";
        protected readonly CodacyConfiguration Config;
        protected readonly Tool CurrentTool;
        protected ImmutableList<string> PatternIds;

        protected CodeAnalyzer(string fileExtension = ".*")
        {
            var patternsJSON = File.ReadAllText(DefaultPatternsFile);
            var patterns = JsonConvert.DeserializeObject<CodacyPatterns>(patternsJSON);

            if (File.Exists(DefaultConfigFile))
            {
                var configJSON = File.ReadAllText(DefaultConfigFile);
                Config = JsonConvert.DeserializeObject<CodacyConfiguration>(configJSON);

                CurrentTool = Array.Find(Config.Tools, tool => tool.Name == patterns.Name);

                if (Config.Files is null)
                {
                    Config.Files = GetSourceFiles(DefaultSourceFolder, fileExtension);
                }

                if (!(CurrentTool.Patterns is null))
                {
                    var patternIds = new List<string>();
                    foreach (var pattern in CurrentTool.Patterns) patternIds.Add(pattern.PatternId);

                    PatternIds = patternIds.ToImmutableList();
                }
            }
            else
            {
                Config = new CodacyConfiguration
                {
                    Files = GetSourceFiles(DefaultSourceFolder, fileExtension),
                    Tools = new[]
                    {
                        new Tool
                        {
                            Name = patterns.Name
                        }
                    }
                };
            }
        }

        protected abstract Task Analyze(CancellationToken cancellationToken);

        public async Task Run()
        {
            var timeoutEnv = Environment.GetEnvironmentVariable("TIMEOUT");

            if (timeoutEnv is null)
            {
                await Analyze(CancellationToken.None).ConfigureAwait(false);
            }
            else
            {
                try
                {
                    var timeout = TimeSpanHelper.Parse(timeoutEnv);
                    using (var cancellationTokenSource = new CancellationTokenSource())
                    {
                        var cancellationToken = cancellationTokenSource.Token;
                        var task = Analyze(cancellationToken);
                        if (await Task.WhenAny(task, Task.Delay(timeout)).ConfigureAwait(false) != task)
                        {
                            cancellationTokenSource.Cancel();
                            Environment.Exit(2);
                        }
                    }
                }
                catch (Exception e)
                {
                    Console.Error.WriteLine($"can't parse 'TIMEOUT' environment variable ({timeoutEnv})");
                    Logger.Send(e.StackTrace);
                    Environment.Exit(1);
                }
            }
        }

        private static string[] GetSourceFiles(string folder, string fileExtension)
        {
            return (from string entry in Directory.GetFiles(folder, "*" + fileExtension, SearchOption.AllDirectories)
                select entry.Substring(entry.IndexOf("/", StringComparison.InvariantCulture) + 1)).ToArray();
        }
    }
}
