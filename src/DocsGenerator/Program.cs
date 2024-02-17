// ReSharper disable PossibleNullReferenceException

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using CodacyCSharp.DocsGenerator.Helpers;
using CodacyCSharp.Analyzer;
using Codacy.Engine.Seed.Patterns;
using Newtonsoft.Json;
using ReverseMarkdown;

namespace CodacyCSharp.DocsGenerator
{
    internal static class Program
    {
        private const string docsFolder = "docs/";
        private const string descriptionFolder = docsFolder + "description/";

        private static void Main()
        {
            var sonarVersion = File.ReadAllText(".SONAR_VERSION")
                .TrimEnd(Environment.NewLine.ToCharArray())
                .Split('.');

            Directory.CreateDirectory(docsFolder);
            Directory.CreateDirectory(descriptionFolder);

            var patternsFile = new CodacyPatterns
            {
                Name = "sonarscharp",
                Version = string.Format("{0}.{1}", sonarVersion),
                Patterns = new List<Pattern>()
            };

            var descriptions = new List<Description>();

            string[] ruleSourceJsonFiles = Directory.GetFiles(".res", "S*.json");
            Array.Sort(ruleSourceJsonFiles);

            string allSourceParametersJsonFileContent = File.ReadAllText(".res/Rules.json");
            ParametersIndex[] allSourceParameters = JsonConvert.DeserializeObject<ParametersIndex[]>(allSourceParametersJsonFileContent);

            foreach (string ruleSourceJsonFile in ruleSourceJsonFiles)
            {
                string ruleSourceJsonFileContent = File.ReadAllText(ruleSourceJsonFile);
                Rule rule = JsonConvert.DeserializeObject<Rule>(ruleSourceJsonFileContent);

                if (CodeAnalyzer.IsInBlacklist(rule.sqKey))
                {
                    continue;
                }

                var lvl = LevelHelper.ToLevel(rule.defaultSeverity);

                ParametersIndex sourceParameters = allSourceParameters.First(x => x.Id == rule.sqKey);
                var patternsParameters = sourceParameters.Parameters.Any() ? sourceParameters.Parameters.Select(param =>
                 new Codacy.Engine.Seed.Patterns.Parameter
                 {
                     Name = param.Key,
                     Default = param.DefaultValue ?? ""
                 }).ToArray() : null;

                var category = CategoryHelper.ToCategory(rule.tags, rule.type, lvl);
                var patternId = rule.sqKey;

                var pattern = new Pattern(
                    patternId,
                    lvl,
                    category,
                    SubcategoryHelper.ToSubcategory(rule.sqKey, rule.tags, category),
                    patternsParameters,
                    DefaultPatterns.defaultPatterns.Contains(patternId));

                var descriptionParameters = sourceParameters.Parameters.Any() ? sourceParameters.Parameters.Select(param => new DescriptionParameter
                {
                    Name = param.Key,
                    Description = param.Description,
                }).ToArray() : null;

                var description = new Description
                {
                    PatternId = pattern.PatternId,
                    Title = rule.title,
                    Parameters = descriptionParameters,
                    TimeToFix = TTFHelper.ToCodacyTimeToFix(rule.remediation?.constantCost ?? "")
                };

                patternsFile.Patterns.Add(pattern);
                descriptions.Add(description);

                string descriptionHTML = File.ReadAllText(".res/" + rule.sqKey + ".html");
                var descriptionMD = new Converter().Convert(descriptionHTML);
                File.WriteAllText(descriptionFolder + pattern.PatternId + ".md", descriptionMD);
            }

            var patternsJson = JsonConvert.SerializeObject(patternsFile,
                Formatting.Indented,
                new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore
                });

            var descriptionJson = JsonConvert.SerializeObject(descriptions,
                Formatting.Indented,
                new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore
                });

            File.WriteAllText("docs/patterns.json", patternsJson + Environment.NewLine);
            File.WriteAllText("docs/description/description.json", descriptionJson + Environment.NewLine);
        }
    }
}
