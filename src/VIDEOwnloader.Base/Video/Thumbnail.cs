using Newtonsoft.Json;

namespace VIDEOwnloader.Base.Video
{
    public class Thumbnail
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("url")]
        public string Url { get; set; }
    }
}