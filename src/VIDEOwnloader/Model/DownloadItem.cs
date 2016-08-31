using System;
using System.IO;
using System.Net;
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

    public class PlaylistDownloadItem : DownloadItem
    {
        public PlaylistDownloadItem(string savePath, Playlist playlist)
        {
            Playlist = playlist;
            SavePath = savePath;
        }

        public override string DownloadCompletedStatusText => $"Saved in {Path.GetFullPath(SavePath)}";

        public Playlist Playlist { get; }

        public string SavePath { get; }
    }

    public class VideoDownloadItem : DownloadItem
    {
        public VideoDownloadItem(Video video, VideoFormat videoFormat, string filename)
        {
            Filename = Path.GetFullPath(filename);
            Video = video;
            VideoFormat = videoFormat;
        }

        public override string DownloadCompletedStatusText => $"Saved as {Path.GetFileName(Filename)}";

        public string Filename { get; }

        public Video Video { get; }
        public VideoFormat VideoFormat { get; }
    }

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

        public bool CanBeCancelled => IsDownloading || IsPaused;

        public bool CanBeRemoved => IsCanceled || IsDownloaded;

        public abstract string DownloadCompletedStatusText { get; }

        public long DownloadedBytes
        {
            get { return _downloadedBytes; }
            set { Set(ref _downloadedBytes, value); }
        }

        public EtaCalculator EtaCalculator { get; set; }

        public bool IsCanceled
        {
            get { return _isCanceled; }
            set
            {
                Set(ref _isCanceled, value);
                if (_isDownloading)
                    _isDownloading = false;
                if (_isPaused)
                    _isPaused = false;
                EtaCalculator = null;
                RaisePropertyChanged(() => CanBeRemoved);
            }
        }

        public bool IsDownloaded
        {
            get { return _isDownloaded; }
            set
            {
                Set(ref _isDownloaded, value);
                if (_isDownloading)
                    _isDownloading = false;
                if (_isPaused)
                    _isPaused = false;
                EtaCalculator = null;
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
                _isDownloading = !_isPaused;
                EtaCalculator?.Reset();
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

        public WebClient WebClient { get; set; } = new WebClient();
    }
}