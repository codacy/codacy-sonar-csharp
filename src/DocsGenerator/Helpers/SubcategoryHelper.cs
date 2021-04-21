using System.Linq;
using System.Xml.Linq;
using Codacy.Engine.Seed.Patterns;

namespace CodacyCSharp.DocsGenerator.Helpers
{
    public static class SubcategoryHelper
    {
        public static Subcategory? ToSubcategory(XElement elem, Category category)
        {
            if(category == Category.Security)
            {
                var tags = elem.Elements("tag").Select((tag, index) => tag.Value);
                if (tags.Any(tag => tag == "sql")) return Subcategory.SQLInjection;
                if (elem.Element("key").Value == "S5042" || tags.Any(tag => tag == "denial-of-service")) return Subcategory.DoS;
                if (tags.Any(tag => tag == "owasp-a1" || tag == "owasp-a8")) return Subcategory.CommandInjection;
                if (tags.Any(tag => tag == "owasp-a2" || tag == "owasp-a5" || tag == "owasp-a10")) return Subcategory.Auth;
                if (tags.Any(tag => tag == "owasp-a3")) return Subcategory.Cryptography;
                if (tags.Any(tag => tag == "owasp-a4")) return Subcategory.InputValidation;
                if (tags.Any(tag => tag == "owasp-a6")) return Subcategory.FileAccess;
                if (tags.Any(tag => tag == "owasp-a7")) return Subcategory.XSS;
                if (tags.Any(tag => tag == "owasp-a9")) return Subcategory.InsecureModulesLibraries;
            }

            return null;
        }
    }
}
