using Newtonsoft.Json;

namespace CodacyCSharp.DocsGenerator.Results
{
    public sealed class DescriptionParameter
    {
        [JsonProperty(PropertyName = "name")] public string Name { get; set; }

        [JsonProperty(PropertyName = "description")] public string Description { get; set; }
    }
}
