using Newtonsoft.Json;

namespace VIDEOwnloader.Base.Video
{
    public class VideoInfoRequest
    {
        [JsonProperty("url")]
        public string[] Urls { get; set; }
    }
}