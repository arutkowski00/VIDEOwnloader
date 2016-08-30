using System.Collections.Generic;
using Newtonsoft.Json;

namespace VIDEOwnloader.Base.Validation
{
    public class UrlValidationResponse
    {
        [JsonProperty("message", NullValueHandling = NullValueHandling.Ignore)]
        public Dictionary<string, string> MessageDictionary { get; set; }

        [JsonProperty("valid")]
        public UrlValidationResult[] ValidationResults { get; set; }
    }
}