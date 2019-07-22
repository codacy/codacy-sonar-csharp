using System.Xml.Linq;
using CodacyCSharp.Seed.Patterns;

namespace CodacyCSharp.DocsGenerator.Helpers
{
	public static class CategoryHelper
	{
		public static Category ToCategory (XElement elem, Level lvl)
		{
			var tag = elem.Element ("tag") ?? new XElement ("undefined");

			switch (tag.Value)
			{
				case "api-design":
				case "bad-practice":
				case "bug":
				case "confusing":
				case "serialization":
				case "design":
				case "error-handling":
				case "finding":
				case "suspicious":
				case "unpredictable":
				case "deadlock":
				case "pitfall":
				case "localisation":
				case "tests":
				case "pinvoke":
					return Category.ErrorProne;

				case "cert":
				case "cwe":
				case "overflow":
				case "owasp-a1":
				case "owasp-a2":
				case "owasp-a3":
				case "owasp-a4":
				case "owasp-a5":
				case "owasp-a6":
				case "owasp-a7":
				case "owasp-a8":
				case "owasp-a9":
				case "owasp-a10":
				case "security":
				case "leak":
				case "sans-top25-risky":
				case "sans-top25-porous":
				case "sans-top25-insecure":
					return Category.Security;

				case "multi-threading":
				case "performance":
				case "denial-of-service":
					return Category.Performance;

				case "brain-overload":
					return Category.Complexity;

				case "redundant":
				case "unused":
					return Category.UnusedCode;

				case "clumsy":
				case "convention":
				case "misra":
				case "style":
					return Category.CodeStyle;

				default:
					var type = elem.Element ("type") ?? new XElement ("undefined");
					switch (type.Value)
					{
						case "VULNERABILITY":
							return Category.Security;
						case "CODE_SMELL":
							if (lvl == Level.Info)
							{
								return Category.CodeStyle;
							}
							else
							{
								return Category.ErrorProne;
							}
						case "BUG":
							return Category.ErrorProne;
						default:
							return Category.CodeStyle;
					}
			}
		}
	}
}
