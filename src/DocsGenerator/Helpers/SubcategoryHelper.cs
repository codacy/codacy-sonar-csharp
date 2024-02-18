using System.Collections.Generic;
using Codacy.Engine.Seed.Patterns;

namespace CodacyCSharp.DocsGenerator.Helpers
{
    public static class SubcategoryHelper
    {
        public static Subcategory? ToSubcategory(string key, List<string> tags, Category category)
        {
            if (category == Category.Security)
            {
                if (tags.Contains("sql"))
                {
                    return Subcategory.SQLInjection;
                }
                if (key == "S5042" || tags.Contains("denial-of-service"))
                {
                    return Subcategory.DoS;
                }
                if (tags.Contains("owasp-a1") || tags.Contains("owasp-a8"))
                {
                    return Subcategory.CommandInjection;
                }
                if (tags.Contains("owasp-a2") || tags.Contains("owasp-a5") || tags.Contains("owasp-a10"))
                {
                    return Subcategory.Auth;
                }
                if (tags.Contains("owasp-a3"))
                {
                    return Subcategory.Cryptography;
                }
                if (tags.Contains("owasp-a4"))
                {
                    return Subcategory.InputValidation;
                }
                if (tags.Contains("owasp-a6"))
                {
                    return Subcategory.FileAccess;
                }
                if (tags.Contains("owasp-a7"))
                {
                    return Subcategory.XSS;
                }
                if (tags.Contains("owasp-a9"))
                {
                    return Subcategory.InsecureModulesLibraries;
                }
            }

            return null;
        }
    }
}
