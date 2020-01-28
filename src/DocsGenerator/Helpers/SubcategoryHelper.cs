using System.Xml.Linq;
using Codacy.Engine.Seed.Patterns;

namespace CodacyCSharp.DocsGenerator.Helpers
{
    public static class SubcategoryHelper
    {
        public static Subcategory? ToSubcategory(XElement elem, Category category)
        {
            var tags = elem.Elements("tag");
            foreach(var tag in tags)
            {
                if(category == Category.Security)
                {
                    switch (tag.Value)
                    {
                        case "denial-of-service":
                            return Subcategory.DoS;
                        case "owasp-a1":
                            return Subcategory.CommandInjection;
                        case "owasp-a2":
                            return Subcategory.Auth;
                        case "owasp-a3":
                            return Subcategory.Cryptography;
                        case "owasp-a4":
                            return Subcategory.InputValidation;
                        case "owasp-a5":
                            return Subcategory.Auth;
                        case "owasp-a6":
                            return Subcategory.FileAccess;
                        case "owasp-a7":
                            return Subcategory.XSS;
                        case "owasp-a8":
                            return Subcategory.CommandInjection;
                        case "owasp-a9":
                            return Subcategory.InsecureModulesLibraries;
                        case "owasp-a10":
                            return Subcategory.Auth;
                        default:
                            continue;
                    }
                }
            }

            return null;
        }
    }
}
