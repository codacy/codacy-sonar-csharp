using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using SonarAnalyzer.Common;
using SonarAnalyzer.Rules.CSharp;
using SonarAnalyzer.Rules.Common;
using SonarAnalyzer.Rules;

namespace CodacyCSharp.Analyzer.Utilities
{
	public class RuleFinder
	{
		private readonly IReadOnlyList<Type> diagnosticAnalyzers;

		public static IEnumerable<Assembly> PackagedRuleAssemblies { get; } =
			new[]
			{
				Assembly.Load(typeof(FlagsEnumZeroMember).Assembly.GetName()),
				Assembly.Load(typeof(FlagsEnumZeroMemberBase).Assembly.GetName())
			};

		public RuleFinder()
		{
			this.diagnosticAnalyzers = PackagedRuleAssemblies
				.SelectMany(assembly => assembly.GetTypes())
				.Where(t => t.IsSubclassOf(typeof(DiagnosticAnalyzer)))
				.Where(t => t.GetCustomAttributes<RuleAttribute>().Any())
				.ToList();
		}

		public IEnumerable<Type> GetParameterlessAnalyzerTypes()
		{
			return this.diagnosticAnalyzers
				.Where(analyzerType => !IsParameterized(analyzerType))
				.Where(type => GetTargetLanguages(type).IsAlso(AnalyzerLanguage.CSharp));
		}

		public static bool IsParameterized(Type analyzerType)
		{
			return analyzerType.GetProperties()
				.Any(p => p.GetCustomAttributes<RuleParameterAttribute>().Any());
		}

		public IEnumerable<Type> AllAnalyzerTypes => diagnosticAnalyzers;

		public IEnumerable<Type> GetAnalyzerTypes()
		{
			return this.diagnosticAnalyzers
				.Where(type => GetTargetLanguages(type).IsAlso(AnalyzerLanguage.CSharp));
		}

		public static IEnumerable<Type> GetUtilityAnalyzerTypes()
		{
			return PackagedRuleAssemblies
				.SelectMany(assembly => assembly.GetTypes())
				.Where(t => !t.IsAbstract && t.IsSubclassOf(typeof(UtilityAnalyzerBase)))
				.Where(type => GetTargetLanguages(type).IsAlso(AnalyzerLanguage.CSharp));
		}

		public static AnalyzerLanguage GetTargetLanguages(Type analyzerType)
		{
			var attribute = analyzerType.GetCustomAttributes<DiagnosticAnalyzerAttribute>().FirstOrDefault();
			if (attribute == null)
			{
				return null;
			}

			var language = AnalyzerLanguage.None;
			foreach (var lang in attribute.Languages)
			{
				if (lang == LanguageNames.CSharp)
				{
					language = language.AddLanguage(AnalyzerLanguage.CSharp);
				}
			}

			return language;
		}
	}
}
