using System.Collections.Generic;
using Newtonsoft.Json;

namespace CodacyCSharp.Seed.Patterns
{
    public sealed class CodacyPatterns
    {
        [JsonProperty(PropertyName = "name")] public string Name { get; set; }

        [JsonProperty(PropertyName = "version")]
        public string Version { get; set; }

        [JsonProperty(PropertyName = "patterns")]
        public List<Pattern> Patterns { get; set; }
    }
}
