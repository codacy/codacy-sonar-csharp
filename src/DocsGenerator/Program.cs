using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using CodacyCSharp.Seed.Patterns;
using Newtonsoft.Json;

namespace CodacyCSharp.DocsGenerator
{
	using Helpers;

	static class Program
	{
		private const string docsFolder = "docs/";
		private const string descriptionFolder = docsFolder + "description/";

		static void Main (string[] args)
		{
			string sonarVersion = File.ReadAllText (".SONAR_VERSION").TrimEnd (Environment.NewLine.ToCharArray ());

			Directory.CreateDirectory (docsFolder);
			Directory.CreateDirectory (descriptionFolder);

			CodacyPatterns patternsFile = new CodacyPatterns
			{
				Name = "Sonar C#",
					Version = sonarVersion,
					Patterns = new List<Pattern> ()
			};

			List<Results.Description> descriptions = new List<Results.Description> ();

			var doc = XDocument.Load (@".res/sonar-csharp-rules.xml");
			foreach (var rule in doc.Root.Elements ())
			{
				var lvl = LevelHelper.ToLevel ((rule.Element ("severity") ?? new XElement ("undefined")).Value);
				Pattern pattern = new Pattern
				{
					PatternId = rule.Element ("key").Value,
						Level = lvl,
						Category = CategoryHelper.ToCategory (rule, lvl),
						Parameters = (from param in rule.Elements () where param.Name == "param"
							select new Parameter
							{
								Name = param.Element ("key").Value,
									Default = param.Element ("defaultValue").Value ?? ""
							}).ToArray ()
				};

				Results.Description description = new Results.Description
				{
					PatternId = pattern.PatternId,
					Title = rule.Element ("name").Value
				};

				patternsFile.Patterns.Add (pattern);
				descriptions.Add (description);

				var descriptionHTML = rule.Element ("description").Value;
				var descriptionMD = new ReverseMarkdown.Converter ().Convert (descriptionHTML);
				File.WriteAllText (descriptionFolder + pattern.PatternId + ".md", descriptionMD);
			}

			string patternsJson = JsonConvert.SerializeObject (patternsFile,
				Newtonsoft.Json.Formatting.Indented,
				new JsonSerializerSettings
				{
					NullValueHandling = NullValueHandling.Ignore
				});

			string descriptionJson = JsonConvert.SerializeObject (descriptions,
				Newtonsoft.Json.Formatting.Indented,
				new JsonSerializerSettings
				{
					NullValueHandling = NullValueHandling.Ignore
				});

			File.WriteAllText ("docs/patterns.json", patternsJson);
			File.WriteAllText ("docs/description/description.json", descriptionJson);
		}
	}
}
