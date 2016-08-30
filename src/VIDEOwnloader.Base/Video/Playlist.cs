using Newtonsoft.Json;

namespace VIDEOwnloader.Base.Video
{
    public class Playlist : IVideoInfoResultItem
    {
        [JsonProperty("entries")]
        public PlaylistEntry[] Entries { get; set; }

        #region IVideoInfoResultItem Members

        [JsonProperty("extractor")]
        public string Extractor { get; set; }

        [JsonProperty("extractor_key")]
        public string ExtractorKey { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("webpage_url")]
        public string WebpageUrl { get; set; }

        [JsonProperty("webpage_url_basename")]
        public string WebpageUrlBasename { get; set; }

        #endregion
    }
}