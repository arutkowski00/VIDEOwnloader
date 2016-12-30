using System;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Serialization;
using GalaSoft.MvvmLight;

namespace VIDEOwnloader.Common.Downloader
{
    public delegate void DownloadProgressChangedEventHandler(object sender, DownloadProgressChangedEventArgs e);

    public delegate void DownloadSessionStateChangedEventHandler(object sender, DownloadSessionStateChangedEventArgs e);

    [Serializable]
    public class DownloadSession : ObservableObject
    {
        private bool _canBePaused;
        private CancellationTokenSource _cancellationTokenSource;
        private long _downloadedBytes;
        private Task _downloadTask;
        private DateTime? _lastRequestDate;
        private DownloadState? _requestedState;
        private DownloadState _state;
        private long _totalBytes;

        public DownloadSession(Uri source, string targetFileName)
        {
            Source = source;
            TargetFileName = targetFileName;

            _state = DownloadState.Ready;
        }

        public bool CanBePaused
        {
            get { return _canBePaused; }
            private set { Set(ref _canBePaused, value); }
        }

        public long DownloadedBytes
        {
            get { return _downloadedBytes; }
            private set
            {
                Set(ref _downloadedBytes, value);
                RaisePropertyChanged(() => ProgressValue);
            }
        }

        public long TotalBytes
        {
            get { return _totalBytes; }
            private set
            {
                Set(ref _totalBytes, value);
                RaisePropertyChanged(() => ProgressValue);
            }
        }

        public DownloadState State
        {
            get { return _state; }
            private set
            {
                var oldState = _state;
                Set(ref _state, value);
                OnStateChanged(oldState, _state);
                RaisePropertyChanged(() => CanBeCancelled);
            }
        }

        [XmlIgnore]
        public bool CanBeCancelled => (State == DownloadState.Downloading) || (State == DownloadState.Paused);

        [XmlIgnore]
        public IEtaCalculator EtaCalculator { get; } = new EtaCalculator(2, 15);

        [XmlIgnore]
        public string PartFilename => TargetFileName + ".part";

        [XmlIgnore]
        public float ProgressValue => (float)DownloadedBytes / TotalBytes;

        public Uri Source { get; }
        public string TargetFileName { get; }

        public event DownloadProgressChangedEventHandler DownloadProgressChanged;
        public event DownloadSessionStateChangedEventHandler StateChanged;

        public async Task DownloadAsync()
        {
            if ((_downloadTask != null) && !_downloadTask.IsCompleted)
                throw new InvalidOperationException(
                    $"Download already in progress; use new {nameof(DownloadSession)} or invoke {nameof(CancelAsync)}");

            _cancellationTokenSource = new CancellationTokenSource();
            _downloadTask = DownloadAsyncInternal(_cancellationTokenSource.Token);
            await _downloadTask.ContinueWith(task =>
            {
                if (_requestedState.HasValue)
                {
                    State = _requestedState.Value;
                    _requestedState = null;
                }
                else if (task.IsCanceled)
                {
                    State = DownloadState.Cancelled;
                }
                else if (task.IsFaulted)
                {
                    State = DownloadState.Failed;
                }
                else
                {
                    State = DownloadState.Success;
                }

                if (task.Exception == null) return;
                if (File.Exists(PartFilename))
                    File.Delete(PartFilename);
                if (File.Exists(TargetFileName))
                    File.Delete(TargetFileName);
                throw task.Exception;
            });
        }


        private async Task DownloadAsyncInternal(CancellationToken ct)
        {
            var headRequest = WebRequest.CreateHttp(Source);
            headRequest.Method = "HEAD";

            var canContinue = (State == DownloadState.Paused) && _lastRequestDate.HasValue;
            if (canContinue)
                headRequest.IfModifiedSince = _lastRequestDate.Value;

            State = DownloadState.Downloading;
            var getRequest = WebRequest.CreateHttp(Source);

            // Send HEAD to investigate the headers and prepare proper GET request
            using (var response = (HttpWebResponse)await headRequest.GetResponseAsync())
            {
                if ((response.StatusCode == HttpStatusCode.NotModified) ||
                    (canContinue && (response.LastModified != default(DateTime)) &&
                     (response.LastModified < _lastRequestDate.Value)))
                {
                    var partFileInfo = new FileInfo(PartFilename);
                    if (partFileInfo.Exists)
                    {
                        DownloadedBytes = partFileInfo.Length;
                        getRequest.AddRange("bytes", partFileInfo.Length, TotalBytes);
                    }
                }
                else
                {
                    TotalBytes = response.ContentLength;
                    CanBePaused =
                        string.Compare(response.Headers["Accept-Ranges"], "bytes", StringComparison.OrdinalIgnoreCase) ==
                        0;
                }
            }

            ct.ThrowIfCancellationRequested();
            // Actual download (GET)
            using (var response = (HttpWebResponse)await getRequest.GetResponseAsync())
            {
                _lastRequestDate = DateTime.UtcNow;
                var responseStream = response.GetResponseStream();
                if (responseStream == null)
                    throw new IOException("Cannot get HTTP response stream");

                // Append new data or create new file by default
                var fileMode = FileMode.Append;

                // If Content-Length doesn't match already known size, start downloading from scratch
                if (response.ContentLength != TotalBytes - DownloadedBytes)
                {
                    DownloadedBytes = 0;
                    TotalBytes = response.ContentLength;
                    fileMode = FileMode.Truncate;
                    EtaCalculator.Reset();
                }

                using (var fileStream = new FileStream(PartFilename, fileMode, FileAccess.Write, FileShare.None))
                {
                    var buffer = new byte[64 * 1024];

                    int read;
                    while (
                        (read = await responseStream.ReadAsync(buffer, 0, buffer.Length, ct)) > 0)
                    {
                        await fileStream.WriteAsync(buffer, 0, read, ct);
                        DownloadedBytes += read;
                        EtaCalculator.Update(ProgressValue);
                        OnDownloadProgressChanged();
                    }
                }
                File.Move(PartFilename, TargetFileName);
            }
            State = DownloadState.Success;
        }

        public async Task PauseAsync()
        {
            if (!CanBePaused || (State != DownloadState.Downloading)) return;

            if (!_requestedState.HasValue)
                _requestedState = DownloadState.Paused;

            if (!_cancellationTokenSource.IsCancellationRequested)
                _cancellationTokenSource.Cancel();
            await Task.WhenAll(_downloadTask);
        }

        public async Task CancelAsync()
        {
            if (!CanBeCancelled) return;

            if (!_requestedState.HasValue)
                _requestedState = DownloadState.Cancelled;

            if (!_cancellationTokenSource.IsCancellationRequested)
                _cancellationTokenSource.Cancel();
            await _downloadTask.ContinueWith((task, o) =>
            {
                if (File.Exists(PartFilename))
                    File.Delete(PartFilename);
            }, null);
        }

        private void OnDownloadProgressChanged()
        {
            DownloadProgressChanged?.Invoke(this, new DownloadProgressChangedEventArgs(this));
        }

        private void OnStateChanged(DownloadState oldState, DownloadState newState)
        {
            StateChanged?.Invoke(this, new DownloadSessionStateChangedEventArgs(this, oldState, newState));
        }
    }
}