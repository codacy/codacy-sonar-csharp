using Newtonsoft.Json;

namespace CodacyCSharp.Seed.Configuration
{
    public sealed class Pattern
    {
        [JsonProperty(PropertyName = "patternId")]
        public string PatternId { get; set; }

        [JsonProperty(PropertyName = "parameters")]
        public Parameter[] Parameters { get; set; }
    }
}
