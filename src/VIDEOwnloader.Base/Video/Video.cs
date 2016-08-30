using Newtonsoft.Json;

namespace VIDEOwnloader.Base.Video
{
    public class Video : IVideoInfoResultItem
    {
        [JsonProperty("abr")]
        public int Abr { get; set; }

        [JsonProperty("acodec")]
        public string Acodec { get; set; }

        [JsonProperty("age_limit")]
        public int AgeLimit { get; set; }

        [JsonProperty("alt_title")]
        public string AltTitle { get; set; }

        [JsonProperty("annotations")]
        public object Annotations { get; set; }

        [JsonProperty("automatic_captions")]
        public AutomaticCaptions AutomaticCaptions { get; set; }

        [JsonProperty("average_rating")]
        public double AverageRating { get; set; }

        [JsonProperty("categories")]
        public string[] Categories { get; set; }

        [JsonProperty("creator")]
        public object Creator { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("dislike_count")]
        public int DislikeCount { get; set; }

        [JsonProperty("display_id")]
        public string DisplayId { get; set; }

        [JsonProperty("duration")]
        public int Duration { get; set; }

        [JsonProperty("end_time")]
        public object EndTime { get; set; }

        [JsonProperty("ext")]
        public string Ext { get; set; }

        [JsonProperty("format")]
        public string Format { get; set; }

        [JsonProperty("format_id")]
        public string FormatId { get; set; }

        [JsonProperty("format_note")]
        public string FormatNote { get; set; }

        [JsonProperty("formats")]
        public VideoFormat[] Formats { get; set; }

        [JsonProperty("height")]
        public int Height { get; set; }

        [JsonProperty("is_live")]
        public bool? IsLive { get; set; }

        [JsonProperty("license")]
        public string License { get; set; }

        [JsonProperty("like_count")]
        public int LikeCount { get; set; }

        [JsonProperty("player_url")]
        public string PlayerUrl { get; set; }

        [JsonProperty("playlist")]
        public object Playlist { get; set; }

        [JsonProperty("playlist_index")]
        public object PlaylistIndex { get; set; }

        [JsonProperty("protocol")]
        public string Protocol { get; set; }

        [JsonProperty("requested_subtitles")]
        public object RequestedSubtitles { get; set; }

        [JsonProperty("resolution")]
        public string Resolution { get; set; }

        [JsonProperty("start_time")]
        public object StartTime { get; set; }

        [JsonProperty("subtitles")]
        public Subtitles Subtitles { get; set; }

        [JsonProperty("tags")]
        public string[] Tags { get; set; }

        [JsonProperty("thumbnail")]
        public string Thumbnail { get; set; }

        [JsonProperty("thumbnails")]
        public Thumbnail[] Thumbnails { get; set; }

        [JsonProperty("upload_date")]
        public string UploadDate { get; set; }

        [JsonProperty("uploader")]
        public string Uploader { get; set; }

        [JsonProperty("uploader_id")]
        public string UploaderId { get; set; }

        [JsonProperty("uploader_url")]
        public string UploaderUrl { get; set; }

        [JsonProperty("url")]
        public string Url { get; set; }

        [JsonProperty("vcodec")]
        public string Vcodec { get; set; }

        [JsonProperty("view_count")]
        public int ViewCount { get; set; }

        [JsonProperty("width")]
        public int Width { get; set; }

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