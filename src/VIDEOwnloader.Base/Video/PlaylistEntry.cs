using Newtonsoft.Json;

namespace VIDEOwnloader.Base.Video
{
    public class PlaylistEntry
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("ie_key")]
        public string IeKey { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("_type")]
        public string Type { get; set; }

        [JsonProperty("url")]
        public string Url { get; set; }
    }
}