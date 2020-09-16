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

            var doc = XDocument.Load(@".res/sonar-csharp-rules.xml");

            var elements = doc.Root.Elements().Where(rule => !CodeAnalyzer.IsInBlacklist(rule.Element("key").Value));
            
            foreach (var rule in elements)
            {
                var lvl = LevelHelper.ToLevel((rule.Element("severity") ?? new XElement("undefined")).Value);

                var parameters = rule.Elements().Where(e => e.Name == "param");

                var patternsParameters = parameters.Any() ? parameters.Select(param => new Parameter
                {
                    Name = param.Element("key").Value,
                    Default = param.Element("defaultValue")?.Value ?? ""
                }).ToArray() : null;

                var category = CategoryHelper.ToCategory(rule, lvl);
                var patternId = rule.Element("key").Value;

                var pattern = new Pattern(
                    patternId,
                    lvl,
                    category,
                    SubcategoryHelper.ToSubcategory(rule, category),
                    patternsParameters,
                    DefaultPatterns.defaultPatterns.Contains(patternId));

                var descriptionParameters = parameters.Any() ? parameters.Select(param => new DescriptionParameter
                {
                    Name = param.Element("key").Value,
                    Description = param.Element("description").Value
                }).ToArray() : null;

                var description = new Description
                {
                    PatternId = pattern.PatternId,
                    Title = rule.Element("name").Value,
                    Parameters = descriptionParameters,
                    TimeToFix = TTFHelper.ToCodacyTimeToFix(rule.Element("remediationFunctionBaseEffort")?.Value ?? "")
                };

                patternsFile.Patterns.Add(pattern);
                descriptions.Add(description);

                var descriptionHTML = rule.Element("description").Value;
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
