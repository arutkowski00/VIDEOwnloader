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
        private bool _pauseRequested;
        private DownloadState? _requestedState;
        private DownloadState _state;
        private long _totalBytes;

        public DownloadSession(Uri source, string targetFileName)
            : this()
        {
            Source = source.ToString();
            TargetFileName = targetFileName;
        }

        protected DownloadSession()
        {
            _state = DownloadState.Ready;
        }

        [XmlIgnore]
        public bool CanBeCancelled => (State == DownloadState.Downloading) || (State == DownloadState.Paused);

        public bool CanBePaused
        {
            get { return _canBePaused; }
            set { Set(ref _canBePaused, value); }
        }

        public long DownloadedBytes
        {
            get { return _downloadedBytes; }
            set
            {
                Set(ref _downloadedBytes, value);
                RaisePropertyChanged(() => ProgressValue);
            }
        }

        [XmlIgnore]
        public Exception Error { get; set; }

        [XmlIgnore]
        public IEtaCalculator EtaCalculator { get; } = new EtaCalculator(2, 15);

        [XmlIgnore]
        public string PartFilename => TargetFileName + ".part";

        [XmlIgnore]
        public float ProgressValue => (float)DownloadedBytes / TotalBytes;

        public string Source { get; set; }

        public DownloadState State
        {
            get { return _state; }
            set
            {
                var oldState = _state;
                Set(ref _state, value);
                OnStateChanged(oldState, _state);
                RaisePropertyChanged(() => CanBeCancelled);
            }
        }

        public string TargetFileName { get; set; }

        public long TotalBytes
        {
            get { return _totalBytes; }
            set
            {
                Set(ref _totalBytes, value);
                RaisePropertyChanged(() => ProgressValue);
            }
        }

        public async Task CancelAsync()
        {
            if (!CanBeCancelled) return;

            if (!_cancellationTokenSource.IsCancellationRequested)
                _cancellationTokenSource.Cancel();
            await _downloadTask;
        }

        public async Task DownloadAsync(bool throwOnFailure = true)
        {
            if ((_downloadTask != null) && !_downloadTask.IsCompleted)
                throw new InvalidOperationException(
                    $"Download already in progress; use new {nameof(DownloadSession)} or invoke {nameof(CancelAsync)}");

            _cancellationTokenSource = new CancellationTokenSource();
            _downloadTask = DownloadAsyncInternal(_cancellationTokenSource.Token).ContinueWith(task =>
            {
                if (_requestedState.HasValue)
                {
                    State = _requestedState.Value;
                    _requestedState = null;
                }
                else if (task.IsCanceled)
                {
                    State = _pauseRequested ? DownloadState.Paused : DownloadState.Cancelled;
                }
                else if (task.IsFaulted)
                {
                    Error = task.Exception?.InnerException;
                    State = DownloadState.Failed;
                }
                else
                {
                    State = DownloadState.Success;
                }

                if (State != DownloadState.Paused)
                {
                    CanBePaused = false;

                    if (File.Exists(PartFilename))
                        File.Delete(PartFilename);

                    if (State != DownloadState.Success && File.Exists(TargetFileName))
                        File.Delete(TargetFileName);
                }

                if (throwOnFailure && task.Exception?.InnerException != null)
                    throw task.Exception.InnerException;
            });
            await _downloadTask;
        }

        public event DownloadProgressChangedEventHandler DownloadProgressChanged;

        public async Task PauseAsync()
        {
            if (!CanBePaused || (State != DownloadState.Downloading)) return;
            _pauseRequested = true;

            if (!_cancellationTokenSource.IsCancellationRequested)
                _cancellationTokenSource.Cancel();
            await _downloadTask;
        }

        public event DownloadSessionStateChangedEventHandler StateChanged;


        private async Task DownloadAsyncInternal(CancellationToken ct)
        {
            var headRequest = WebRequest.CreateHttp(Source);
            headRequest.Method = "HEAD";
            headRequest.AllowAutoRedirect = false;

            var tryResume = State == DownloadState.Paused && _lastRequestDate.HasValue;
            if (tryResume)
                headRequest.IfModifiedSince = _lastRequestDate.Value;

            State = DownloadState.Downloading;
            var getRequest = WebRequest.CreateHttp(Source);

            // Send HEAD to investigate the headers and prepare proper GET request
            using (var response = (HttpWebResponse)await headRequest.GetResponseAsync())
            {
                if (tryResume && (response.StatusCode == HttpStatusCode.NotModified ||
                    (response.LastModified != default(DateTime) &&
                     response.LastModified < _lastRequestDate.Value)))
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