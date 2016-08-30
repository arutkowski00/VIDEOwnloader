using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using GalaSoft.MvvmLight.CommandWpf;
using MaterialDesignThemes.Wpf;
using VIDEOwnloader.Common;
using VIDEOwnloader.DataService;
using VIDEOwnloader.Model;
using VIDEOwnloader.View.Dialog;

namespace VIDEOwnloader.ViewModel
{
    public class DownloadsViewModel : ExtendedViewModelBase
    {
        private readonly TimeSpan _updateProgressTimeSpan = new TimeSpan(0, 0, 0, 0, 500);
        private RelayCommand _newDownloadCommand;
        private RelayCommand<DownloadItem> _removeCommand;
        private RelayCommand<DownloadItem> _stopDownloadCommand;

        public DownloadsViewModel(IDataService dataService)
        {
            DataService = dataService;

            if (IsInDesignMode)
                DataService.GetVideoAsync(null, (res, ex) =>
                {
                    var random = new Random();
                    DownloadList =
                        new ObservableCollection<DownloadItem>(
                            res.Videos.Select(
                                x =>
                                    new VideoDownloadItem(x, x.Formats[random.Next(0, x.Formats.Length - 1)],
                                        "test." + (x.Ext ?? "mp4"))
                                    {
                                        DownloadedBytes = (uint)(random.NextDouble()*4 + 2)*1048576,
                                        TotalBytes = (uint)(random.NextDouble()*10 + 8)*1048576,
                                        IsDownloading = true
                                    }));

                    CompletedList =
                        new ObservableCollection<DownloadItem>(
                            res.Videos.Select(
                                x =>
                                    new VideoDownloadItem(x, x.Formats[random.Next(0, x.Formats.Length - 1)],
                                        "test." + (x.Ext ?? "mp4"))
                                    {
                                        IsDownloaded = true
                                    }));
                }).Wait();

            DownloadList.CollectionChanged += (sender, e) =>
            {
                RaisePropertyChanged(() => AreItemsInDownloadList);
                RaisePropertyChanged(() => IsDownloadListEmpty);
            };

            CompletedList.CollectionChanged += (sender, e) => { RaisePropertyChanged(() => AreItemsInCompletedList); };

            MessengerInstance.Register<VideoDownloadItem>(this, HandleNewVideoDownload);
            //MessengerInstance.Register<PlaylistDownloadItem>(this, HandleNewPlaylistDownload);
        }

        public bool AreItemsInCompletedList => DownloadList.Count > 0;
        public bool AreItemsInDownloadList => DownloadList.Count > 0;

        [RaisePropertyChanged]
        public ObservableCollection<DownloadItem> CompletedList { get; private set; } =
            new ObservableCollection<DownloadItem>();

        public IDataService DataService { get; }

        [RaisePropertyChanged]
        public ObservableCollection<DownloadItem> DownloadList { get; private set; } =
            new ObservableCollection<DownloadItem>();

        public bool IsDownloadListEmpty => DownloadList.Count == 0;

        public RelayCommand NewDownloadCommand
        {
            get
            {
                return _newDownloadCommand ?? (_newDownloadCommand = new RelayCommand(
                           async () =>
                           {
                               var view = new NewDownloadDialog();
                               await DialogHost.Show(view, "RootDialog");
                           }));
            }
        }

        public RelayCommand<DownloadItem> RemoveCommand
        {
            get
            {
                return _removeCommand
                       ?? (_removeCommand = new RelayCommand<DownloadItem>(
                           item =>
                           {
                               if ((item == null) || !item.CanBeRemoved) return;
                               DownloadList.Remove(item);
                               CompletedList.Remove(item);
                           }, item => (item != null) && item.CanBeRemoved));
            }
        }

        public RelayCommand<DownloadItem> StopDownloadCommand
        {
            get
            {
                return _stopDownloadCommand
                       ?? (_stopDownloadCommand = new RelayCommand<DownloadItem>(
                           item => item?.WebClient.CancelAsync(), item => (item != null) && item.CanBeCancelled));
            }
        }

        private void HandleNewVideoDownload(VideoDownloadItem downloadItem)
        {
            downloadItem.WebClient.DownloadProgressChanged += WebClientOnDownloadProgressChanged;
            downloadItem.WebClient.DownloadFileCompleted += WebClientOnDownloadFileCompleted;
            downloadItem.WebClient.DownloadFileAsync(new Uri(downloadItem.VideoFormat.Url), downloadItem.Filename,
                downloadItem);
            DownloadList.Add(downloadItem);
        }

        private void WebClientOnDownloadFileCompleted(object sender, AsyncCompletedEventArgs asyncCompletedEventArgs)
        {
            var downloadItem = asyncCompletedEventArgs.UserState as DownloadItem;
            if (downloadItem == null) return;
            downloadItem.IsDownloading = false;

            if ((asyncCompletedEventArgs.Error != null) || asyncCompletedEventArgs.Cancelled)
            {
                if (downloadItem is VideoDownloadItem)
                {
                    var videoItem = (VideoDownloadItem)downloadItem;
                    if (File.Exists(videoItem.Filename))
                        File.Delete(videoItem.Filename);
                }
                // TODO: handle playlists
                // TODO: handle error (message dialog or state change)

                if (asyncCompletedEventArgs.Cancelled)
                    downloadItem.IsCanceled = true;
            }
            else
            {
                downloadItem.IsDownloaded = true;
                DownloadList.Remove(downloadItem);
                CompletedList.Add(downloadItem);
            }
        }

        private void WebClientOnDownloadProgressChanged(object sender,
            DownloadProgressChangedEventArgs downloadProgressChangedEventArgs)
        {
            var downloadItem = downloadProgressChangedEventArgs.UserState as DownloadItem;
            if (downloadItem == null)
                return;
            if (!downloadItem.IsDownloading)
                downloadItem.IsDownloading = true;
            if (DateTime.Now - downloadItem.LastStatusUpdateTime > _updateProgressTimeSpan)
            {
                downloadItem.DownloadedBytes = downloadProgressChangedEventArgs.BytesReceived;
                downloadItem.TotalBytes = downloadProgressChangedEventArgs.TotalBytesToReceive;
                downloadItem.LastStatusUpdateTime = DateTime.Now;
            }
        }
    }
}