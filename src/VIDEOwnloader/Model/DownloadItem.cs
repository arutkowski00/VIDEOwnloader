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
using System.Net;
using System.Xml.Serialization;
using GalaSoft.MvvmLight;
using VIDEOwnloader.Base.Video;
using VIDEOwnloader.Common;

namespace VIDEOwnloader.Model
{
    public interface IDownloadable
    {
        string Title { get; set; }
        string Url { get; set; }
    }

    [Serializable]
    public class PlaylistDownloadItem : DownloadItem
    {
        public override string DownloadCompletedStatusText => $"Saved in {Path.GetFullPath(SavePath)}";

        public Playlist Playlist { get; set; }

        public string SavePath { get; set; }
    }

    [Serializable]
    public class VideoDownloadItem : DownloadItem
    {
        public override string DownloadCompletedStatusText => $"Saved as {Path.GetFileName(Filename)}";

        public string Filename { get; set; }

        public Video Video { get; set; }

        public VideoFormat VideoFormat { get; set; }
    }

    [XmlInclude(typeof(PlaylistDownloadItem))]
    [XmlInclude(typeof(VideoDownloadItem))]
    [Serializable]
    public abstract class DownloadItem : ObservableObject
    {
        private long _downloadedBytes;
        private bool _isCanceled;
        private bool _isDownloaded;
        private bool _isDownloading;
        private bool _isPaused;
        private float _progressValue;
        private string _statusText;
        private long _totalSize;

        [XmlIgnore]
        public bool CanBeCancelled => IsDownloading || IsPaused;

        [XmlIgnore]
        public bool CanBePaused => false; // TODO: add pause support

        [XmlIgnore]
        public bool CanBeRemoved => IsCanceled || IsDownloaded;

        public abstract string DownloadCompletedStatusText { get; }

        public long DownloadedBytes
        {
            get { return _downloadedBytes; }
            set { Set(ref _downloadedBytes, value); }
        }

        [XmlIgnore]
        public IEtaCalculator EtaCalculator { get; set; }

        public string ErrorText { get; set; }

        public bool IsCanceled
        {
            get { return _isCanceled; }
            set
            {
                Set(ref _isCanceled, value);
                RaisePropertyChanged(() => CanBeRemoved);
            }
        }

        public bool IsDownloaded
        {
            get { return _isDownloaded; }
            set
            {
                Set(ref _isDownloaded, value);
                RaisePropertyChanged(() => CanBeRemoved);
            }
        }

        public bool IsDownloading
        {
            get { return _isDownloading; }
            set
            {
                Set(ref _isDownloading, value);
                RaisePropertyChanged(() => CanBeCancelled);
            }
        }

        public bool IsPaused
        {
            get { return _isPaused; }
            set
            {
                Set(ref _isPaused, value);
                RaisePropertyChanged(() => CanBeCancelled);
            }
        }

        public DateTime LastStatusUpdateTime { get; set; }

        public float ProgressValue
        {
            get { return _progressValue; }
            set { Set(ref _progressValue, value); }
        }

        public string StatusText
        {
            get { return _statusText; }
            set { Set(ref _statusText, value); }
        }

        public long TotalBytes
        {
            get { return _totalSize; }
            set { Set(ref _totalSize, value); }
        }

        [XmlIgnore]
        public WebClient WebClient { get; set; } = new WebClient();
    }
}