#region License

// VIDEOwnloader
// Copyright (C) 2016 Adam Rutkowski
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.

#endregion

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