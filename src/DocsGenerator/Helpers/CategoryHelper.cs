using System.Collections.Generic;
using Codacy.Engine.Seed.Patterns;

namespace CodacyCSharp.DocsGenerator.Helpers
{
    public static class CategoryHelper
    {
        public static Category ToCategory(Rule rule, Level lvl)
        {
            if (rule.type == "VULNERABILITY" || rule.type == "SECURITY_HOTSPOT")
            {
                return Category.Security;
            }
            else if (rule.tags.Count > 0)
            {
                switch (rule.tags[0])
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

                    // These are skipped to avoid non-security issues being classified as so.
                    // case "cert":
                    // case "cwe":
                    // case "denial-of-service":
                    // case "overflow":
                    // case "owasp-a1":
                    // case "owasp-a2":
                    // case "owasp-a3":
                    // case "owasp-a4":
                    // case "owasp-a5":
                    // case "owasp-a6":
                    // case "owasp-a7":
                    // case "owasp-a8":
                    // case "owasp-a9":
                    // case "owasp-a10":
                    // case "security":
                    // case "leak":
                    // case "sans-top25-risky":
                    // case "sans-top25-porous":
                    // case "sans-top25-insecure":
                    //     return Category.Security;

                    case "multi-threading":
                    case "performance":
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
                        return DefaultCaseWhenNoTags(rule.type, lvl);
                }
            }
            else
            {
                return DefaultCaseWhenNoTags(rule.type, lvl);
            }

            static Category DefaultCaseWhenNoTags(string type, Level lvl)
            {
                switch (type)
                {
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
