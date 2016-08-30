using System.Collections.Generic;
using Newtonsoft.Json;

namespace VIDEOwnloader.Base.Video
{
    public class VideoInfoResponse
    {
        [JsonProperty("message", NullValueHandling = NullValueHandling.Ignore)]
        public Dictionary<string, string> MessageDictionary { get; set; }

        [JsonProperty("playlists")]
        public Playlist[] Playlists { get; set; }

        [JsonProperty("videos")]
        public Video[] Videos { get; set; }
    }
}