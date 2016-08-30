using Newtonsoft.Json;

namespace VIDEOwnloader.Base.Validation
{
    public class UrlValidationRequest
    {
        [JsonProperty("url")]
        public string[] Urls { get; set; }
    }
}