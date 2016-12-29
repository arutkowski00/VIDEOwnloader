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

using System;
using System.IO;
using System.Xml.Serialization;
using VIDEOwnloader.Base.Video;
using VIDEOwnloader.Common.Downloader;

namespace VIDEOwnloader.Model
{
    //[Serializable]
    //public class PlaylistDownloadItem : DownloadItem
    //{
    //    public override string DownloadCompletedStatusText => $"Saved in {Path.GetFullPath(SavePath)}";

    //    public Playlist Playlist { get; set; }

    //    public string SavePath { get; set; }
    //}

    public class DesignVideoDownloadItem : VideoDownloadItem
    {
        public DesignVideoDownloadItem()
            : base(null, null, null)
        {
        }

        public string DestinationPath { get; set; }
        public new long DownloadedBytes { get; set; }
        public new long TotalBytes { get; set; }
        public new DownloadState State { get; set; }
    }

    [Serializable]
    public class VideoDownloadItem : DownloadItem
    {
        public VideoDownloadItem(Video video, VideoFormat videoFormat, string targetFileName)
            : base(new Uri(videoFormat.Url), targetFileName)
        {
            Video = video;
            VideoFormat = videoFormat;
        }

        public override string DownloadCompletedStatusText => $"Saved as {Path.GetFileName(TargetFileName)}";

        public Video Video { get; set; }

        public VideoFormat VideoFormat { get; set; }
    }

    //[XmlInclude(typeof(PlaylistDownloadItem))]
    [XmlInclude(typeof(VideoDownloadItem))]
    [Serializable]
    public class DownloadItem : DownloadSession
    {
        private DownloadState _state;
        private string _statusText;

        protected DownloadItem(Uri source, string targetFileName) : base(source, targetFileName)
        {
            StateChanged += OnStateChanged;
        }

        [XmlIgnore]
        public bool CanBeRemoved => (State == DownloadState.Cancelled) || (State == DownloadState.Success);

        public virtual string DownloadCompletedStatusText => $"Saved in {Path.GetFullPath(TargetFileName)}";

        public string ErrorText { get; set; }

        public bool IsCanceled => State == DownloadState.Cancelled;
        public bool IsDownloaded => State == DownloadState.Success;
        public bool IsDownloading => State == DownloadState.Downloading;
        public bool IsPaused => State == DownloadState.Paused;

        public DateTime LastStatusUpdateTime { get; set; }

        public string StatusText
        {
            get { return _statusText; }
            set { Set(ref _statusText, value); }
        }

        private void OnStateChanged(object sender, DownloadSessionStateChangedEventArgs args)
        {
            RaisePropertyChanged(() => CanBeRemoved);
        }
    }
}