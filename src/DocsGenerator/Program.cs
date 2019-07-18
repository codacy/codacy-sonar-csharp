// ReSharper disable PossibleNullReferenceException

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using CodacyCSharp.DocsGenerator.Helpers;
using CodacyCSharp.DocsGenerator.Results;
using CodacyCSharp.Seed.Patterns;
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
            var sonarVersion = File.ReadAllText(".SONAR_VERSION").TrimEnd(Environment.NewLine.ToCharArray());

            Directory.CreateDirectory(docsFolder);
            Directory.CreateDirectory(descriptionFolder);

            var patternsFile = new CodacyPatterns
            {
                Name = "Sonar C#",
                Version = sonarVersion,
                Patterns = new List<Pattern>()
            };

            var descriptions = new List<Description>();

            var doc = XDocument.Load(@".res/sonar-csharp-rules.xml");
            foreach (var rule in doc.Root.Elements())
            {
                var lvl = LevelHelper.ToLevel((rule.Element("severity") ?? new XElement("undefined")).Value);
                var pattern = new Pattern
                {
                    PatternId = rule.Element("key").Value,
                    Level = lvl,
                    Category = CategoryHelper.ToCategory(rule, lvl),
                    Parameters = (from param in rule.Elements()
                        where param.Name == "param"
                        select new Parameter
                        {
                            Name = param.Element("key").Value,
                            Default = param.Element("defaultValue")?.Value ?? ""
                        }).ToArray()
                };

                var description = new Description
                {
                    PatternId = pattern.PatternId,
                    Title = rule.Element("name").Value
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

            File.WriteAllText("docs/patterns.json", patternsJson);
            File.WriteAllText("docs/description/description.json", descriptionJson);
        }
    }
}
