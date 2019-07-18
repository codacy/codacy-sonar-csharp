using CodacyCSharp.Seed.Patterns;
using System.Xml.Linq;

namespace CodacyCSharp.DocsGenerator.Helpers
{
	public static class CategoryHelper {
		public static Category ToCategory(XElement elem)
		{
			var tag = elem ?? new XElement("undefined");

			switch(tag.Value)
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
				case "pitfall":
					return Category.ErrorProne;

				case "cert":
				case "cwe":
				case "overflow":
				case "owasp-a6":
				case "sans-top25-porous":
				case "security":
				case "leak":
				case "sans-top25-risky":
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
				default:
					return Category.CodeStyle;
			}
		}
	}
}
