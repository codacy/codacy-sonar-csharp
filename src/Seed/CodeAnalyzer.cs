using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace CodacyCSharp.Seed
{
	using Configuration;
	using Patterns;

	public abstract class CodeAnalyzer
	{
		protected readonly CodacyConfiguration Config;
		protected readonly Tool currentTool;
		protected ImmutableList<string> PatternIds;

		protected const string DefaultConfigFile = ".codacyrc";
		protected const string DefaultPatternsFile = "docs/patterns.json";
		protected const string DefaultSourceFolder = "src/";

		protected CodeAnalyzer (string fileExtension = ".*")
		{
			string patternsJSON = File.ReadAllText (DefaultPatternsFile);
			CodacyPatterns patterns = JsonConvert.DeserializeObject<CodacyPatterns> (patternsJSON);

			if (File.Exists (DefaultConfigFile))
			{
				string configJSON = File.ReadAllText (DefaultConfigFile);
				Config = JsonConvert.DeserializeObject<CodacyConfiguration> (configJSON);

				currentTool = Array.Find (Config.Tools, tool => tool.Name == patterns.Name);

				if (Config.Files is null)
				{
					Config.Files = GetSourceFiles (DefaultSourceFolder, fileExtension);
				}

				if (!(currentTool.Patterns is null))
				{
					var patternIds = new List<string>();
					foreach (Configuration.Pattern pattern in currentTool.Patterns)
					{
						patternIds.Add (pattern.PatternId);
					}

					this.PatternIds = patternIds.ToImmutableList ();
				}

			} else
			{
				Config = new CodacyConfiguration
				{
					Files = GetSourceFiles (DefaultSourceFolder, fileExtension),
						Tools = new []
						{
							new Tool
							{
								Name = patterns.Name
							}
						}
				};
			}
		}

		protected abstract Task Analyze (CancellationToken cancellationToken);

		public async Task Run ()
		{
			string timeoutEnv = Environment.GetEnvironmentVariable ("TIMEOUT");

			if (timeoutEnv is null)
			{
				await Analyze (CancellationToken.None).ConfigureAwait(false);
			} else
			{
				try
				{
					TimeSpan timeout = TimeSpanHelper.Parse (timeoutEnv);
					using (var cancellationTokenSource = new CancellationTokenSource ())
					{
						var cancellationToken = cancellationTokenSource.Token;
						var task = Analyze (cancellationToken);
						if (await Task.WhenAny (task, Task.Delay (timeout)).ConfigureAwait(false) != task)
						{
							cancellationTokenSource.Cancel ();
							Environment.Exit (2);
						}
					}
				} catch (Exception e) {
					Console.Error.WriteLine ($"can't parse 'TIMEOUT' environment variable ({timeoutEnv})");
					Logger.Send (e.StackTrace);
					Environment.Exit (1);
				}
			}
		}

		private static string[] GetSourceFiles (string folder, string fileExtension)
		{
			return (from string entry in Directory.GetFiles (folder, "*" + fileExtension, SearchOption.AllDirectories) select entry.Substring (entry.IndexOf ("/") + 1)).ToArray ();
		}
	}
}
