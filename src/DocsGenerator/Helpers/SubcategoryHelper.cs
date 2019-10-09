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
                        case "owasp-a1":
                            return Subcategory.Injection;
                        case "owasp-a2":
                            return Subcategory.BrokenAuth;
                        case "owasp-a3":
                            return Subcategory.SensitiveData;
                        case "owasp-a4":
                            return Subcategory.XXE;
                        case "owasp-a5":
                            return Subcategory.BrokenAccess;
                        case "owasp-a6":
                            return Subcategory.Misconfiguration;
                        case "owasp-a7":
                            return Subcategory.XSS;
                        case "owasp-a8":
                            return Subcategory.BadDeserialization;
                        case "owasp-a9":
                            return Subcategory.VulnerableComponent;
                        case "owasp-a10":
                            return Subcategory.NoLogging;
                        default:
                            continue;
                    }
                }
            }

            return null;
        }
    }
}
