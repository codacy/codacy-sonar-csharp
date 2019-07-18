using Newtonsoft.Json;

namespace CodacyCSharp.Seed.Results
{
	public sealed class CodacyResult
	{
		[JsonProperty (PropertyName = "filename")]
		public string Filename { get; set; }

		[JsonProperty (PropertyName = "message")]
		public string Message { get; set; }

		[JsonProperty (PropertyName = "patternId")]
		public string PatternId { get; set; }

		[JsonProperty (PropertyName = "line")]
		public long? Line { get; set; }

		public override string ToString ()
		{
			return JsonConvert.SerializeObject (this,
				Newtonsoft.Json.Formatting.None,
				new JsonSerializerSettings
				{
					NullValueHandling = NullValueHandling.Ignore
				});
		}
	}
}
