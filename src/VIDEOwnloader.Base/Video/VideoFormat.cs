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
    public class VideoFormat
    {
        [JsonProperty("abr")]
        public int Abr { get; set; }

        [JsonProperty("acodec")]
        public string Acodec { get; set; }

        [JsonProperty("asr")]
        public int? Asr { get; set; }

        [JsonIgnore]
        public bool AudioOnly => Vcodec == "none";

        [JsonProperty("container")]
        public string Container { get; set; }

        [JsonIgnore]
        public string Description
        {
            get
            {
                if (Vcodec == "none")
                    return Abr > 0 ? $"{Extension} (audio only, {Abr} kbps)" : $"{Extension} (audio only, {FormatId})";
                if ((Acodec == "none") || (Abr <= 0))
                    return $"{FormatNote} {Extension} ({Width}x{Height})";
                return $"{FormatNote} {Extension} ({Width}x{Height}, {Abr} kbps)";
            }
        }

        [JsonProperty("ext")]
        public string Extension { get; set; }

        [JsonProperty("filesize")]
        public int Filesize { get; set; }

        [JsonProperty("format")]
        public string Format { get; set; }

        [JsonProperty("format_id")]
        public string FormatId { get; set; }

        [JsonProperty("format_note")]
        public string FormatNote { get; set; }

        [JsonProperty("fps")]
        public int? Fps { get; set; }

        [JsonIgnore]
        public bool HasAudioAndVideo => (Vcodec != "none") && (Acodec != "none");

        [JsonProperty("height")]
        public int? Height { get; set; }

        [JsonProperty("language")]
        public string Language { get; set; }

        [JsonProperty("player_url")]
        public string PlayerUrl { get; set; }

        [JsonProperty("preference")]
        public int Preference { get; set; }

        [JsonProperty("protocol")]
        public string Protocol { get; set; }

        [JsonProperty("resolution")]
        public string Resolution { get; set; }

        [JsonProperty("tbr")]
        public double Tbr { get; set; }

        [JsonProperty("url")]
        public string Url { get; set; }

        [JsonProperty("vcodec")]
        public string Vcodec { get; set; }

        [JsonProperty("width")]
        public int? Width { get; set; }
    }
}