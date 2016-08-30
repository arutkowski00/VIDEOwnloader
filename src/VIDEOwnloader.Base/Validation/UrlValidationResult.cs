using Newtonsoft.Json;

namespace VIDEOwnloader.Base.Validation
{
    public class UrlValidationResult
    {
        [JsonProperty("is_valid")]
        public bool IsValid { get; set; }

        [JsonProperty("url")]
        public string Url { get; set; }
    }
}