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
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using GalaSoft.MvvmLight.CommandWpf;
using MaterialDesignThemes.Wpf;
using VIDEOwnloader.Common;
using VIDEOwnloader.Common.Extensions;
using VIDEOwnloader.Model;
using VIDEOwnloader.Properties;
using VIDEOwnloader.Services.DataService;
using VIDEOwnloader.View.Dialog;

namespace VIDEOwnloader.ViewModel
{
    public class DownloadsViewModel : ExtendedViewModelBase
    {
        private const double MegabytesMultiplier = 1048576D;

        private readonly TimeSpan _updateProgressTimeSpan = new TimeSpan(0, 0, 0, 0, 500);

        private RelayCommand<DownloadItem> _cancelDownloadCommand;
        private RelayCommand _newDownloadCommand;
        private RelayCommand<DownloadItem> _removeDownloadCommand;

        public DownloadsViewModel(IDataService dataService)
        {
            DataService = dataService;
            if (IsInDesignMode)
            {
                SetDesignMode();
            }
            else
            {
                if (!string.IsNullOrEmpty(Settings.Default.CompletedListXml))
                {
                    DownloadItem[] downloadItems;
                    if (XmlTools.TryDeserialize(Settings.Default.CompletedListXml, out downloadItems))
                        CompletedList = new ObservableCollection<DownloadItem>(downloadItems);
                }
                if (!string.IsNullOrEmpty(Settings.Default.DownloadListXml))
                {
                    DownloadItem[] downloadItems;
                    if (XmlTools.TryDeserialize(Settings.Default.DownloadListXml, out downloadItems))
                        DownloadList = new ObservableCollection<DownloadItem>(downloadItems);
                }
            }

            DownloadList.CollectionChanged += (sender, e) =>
            {
                RaisePropertyChanged(() => AreItemsInDownloadList);
                RaisePropertyChanged(() => IsDownloadListEmpty);
            };

            CompletedList.CollectionChanged += (sender, e) => { RaisePropertyChanged(() => AreItemsInCompletedList); };

            MessengerInstance.Register<VideoDownloadItem>(this, HandleNewVideoDownload);
            //MessengerInstance.Register<PlaylistDownloadItem>(this, HandleNewPlaylistDownload);
        }

        public bool AreItemsInCompletedList => CompletedList.Count > 0;
        public bool AreItemsInDownloadList => DownloadList.Count > 0;

        public RelayCommand<DownloadItem> CancelDownloadCommand
            => _cancelDownloadCommand ?? (_cancelDownloadCommand = new RelayCommand<DownloadItem>(CancelDownload));

        [RaisePropertyChanged]
        public ObservableCollection<DownloadItem> CompletedList { get; private set; } =
            new ObservableCollection<DownloadItem>();

        public IDataService DataService { get; }

        [RaisePropertyChanged]
        public ObservableCollection<DownloadItem> DownloadList { get; private set; } =
            new ObservableCollection<DownloadItem>();

        public bool IsDownloadListEmpty => DownloadList.Count == 0;

        public RelayCommand NewDownloadCommand
            => _newDownloadCommand ?? (_newDownloadCommand = new RelayCommand(ShowNewDownloadDialog));

        public RelayCommand<DownloadItem> RemoveDownloadCommand
            => _removeDownloadCommand ?? (_removeDownloadCommand = new RelayCommand<DownloadItem>(RemoveDownloadItem));

        private void SetDesignMode()
        {
            DataService.GetVideo(null, (res, ex) =>
            {
                var random = new Random();
                DownloadList =
                    new ObservableCollection<DownloadItem>(
                        res.Videos.Select(
                            x =>
                                new VideoDownloadItem
                                {
                                    Filename = "test." + (x.Ext ?? "mp4"),
                                    Video = x,
                                    VideoFormat = x.Formats[random.Next(0, x.Formats.Length - 1)],
                                    DownloadedBytes = (uint)(random.NextDouble()*4 + 2)*1048576,
                                    TotalBytes = (uint)(random.NextDouble()*10 + 8)*1048576,
                                    IsDownloading = true
                                }));

                CompletedList =
                    new ObservableCollection<DownloadItem>(
                        res.Videos.Select(
                            x => new VideoDownloadItem
                            {
                                Filename = "test." + (x.Ext ?? "mp4"),
                                Video = x,
                                VideoFormat = x.Formats[random.Next(0, x.Formats.Length - 1)],
                                IsDownloaded = true
                            }));

                foreach (var downloadItem in CompletedList.Union(DownloadList))
                    SetDownloadItemStatusText(downloadItem);
            });
        }

        ~DownloadsViewModel()
        {
            Settings.Default.CompletedListXml = CompletedList.Count > 0
                ? XmlTools.Serialize(CompletedList.ToArray())
                : string.Empty;

            if (DownloadList.Count > 0)
            {
                var downloadItems = DownloadList.ToArray();
                foreach (var downloadItem in downloadItems)
                {
                    downloadItem.IsDownloading = false;
                    downloadItem.IsCanceled = true;
                    SetDownloadItemStatusText(downloadItem);
                }
                Settings.Default.DownloadListXml = XmlTools.Serialize(downloadItems);
            }
            else Settings.Default.DownloadListXml = string.Empty;
            Settings.Default.Save();
        }

        private void CancelDownload(DownloadItem item)
        {
            item?.WebClient.CancelAsync();
        }

        private void HandleNewVideoDownload(VideoDownloadItem downloadItem)
        {
            downloadItem.WebClient.DownloadProgressChanged += WebClientOnDownloadProgressChanged;
            downloadItem.WebClient.DownloadFileCompleted += WebClientOnDownloadFileCompleted;
            downloadItem.WebClient.DownloadFileAsync(new Uri(downloadItem.VideoFormat.Url), downloadItem.Filename,
                downloadItem);
            downloadItem.EtaCalculator = new EtaCalculator(2, 15);
            DownloadList.Add(downloadItem);
        }

        private void RemoveDownloadItem(DownloadItem item)
        {
            if ((item == null) || !item.CanBeRemoved) return;
            DownloadList.Remove(item);
            CompletedList.Remove(item);
        }

        private void SetDownloadItemStatusText(DownloadItem downloadItem)
        {
            string statusText;
            if (downloadItem.ErrorText != null)
            {
                statusText = $"Error: {downloadItem.ErrorText.Decapitalize()}";
            }
            else if (downloadItem.IsDownloading)
            {
                var progressText =
                    $"{downloadItem.DownloadedBytes/MegabytesMultiplier:F1}/{downloadItem.TotalBytes/MegabytesMultiplier:F1} MB";
                if (downloadItem.EtaCalculator == null)
                    statusText = progressText;
                else if (downloadItem.EtaCalculator.EtaIsAvailable)
                    statusText = progressText + $", {downloadItem.EtaCalculator.Etr.ToReadableString()}";
                else statusText = progressText + ", estimating time...";
            }
            else if (downloadItem.IsPaused)
            {
                statusText =
                    $"Paused, {downloadItem.DownloadedBytes/MegabytesMultiplier:F1}/{downloadItem.TotalBytes/MegabytesMultiplier:F1} MB";
            }
            else if (downloadItem.IsCanceled)
            {
                statusText = "Cancelled";
            }
            else
            {
                statusText = downloadItem.IsDownloaded ? downloadItem.DownloadCompletedStatusText : "Preparing...";
            }

            downloadItem.StatusText = statusText;
        }

        private async void ShowNewDownloadDialog()
        {
            var view = new NewDownloadDialog();
            await DialogHost.Show(view, "RootDialog");
        }

        private void WebClientOnDownloadFileCompleted(object sender, AsyncCompletedEventArgs args)
        {
            var downloadItem = args.UserState as DownloadItem;
            if (downloadItem == null) return;
            downloadItem.IsDownloading = false;
            downloadItem.EtaCalculator = null;

            if ((args.Error != null) || args.Cancelled)
            {
                downloadItem.IsCanceled = true;

                if (downloadItem is VideoDownloadItem)
                {
                    var videoItem = (VideoDownloadItem)downloadItem;
                    if (File.Exists(videoItem.Filename))
                        File.Delete(videoItem.Filename);
                }
                // TODO: handle playlists

                if (args.Error != null)
                    downloadItem.ErrorText = args.Error.GetFullMessage();
            }
            else
            {
                downloadItem.IsDownloaded = true;
                DownloadList.Remove(downloadItem);
                CompletedList.Add(downloadItem);
            }
            SetDownloadItemStatusText(downloadItem);
        }

        private void WebClientOnDownloadProgressChanged(object sender,
            DownloadProgressChangedEventArgs args)
        {
            var downloadItem = args.UserState as DownloadItem;
            if (downloadItem == null)
                return;
            if (!downloadItem.IsDownloading)
                downloadItem.IsDownloading = true;

            downloadItem.DownloadedBytes = args.BytesReceived;
            downloadItem.TotalBytes = args.TotalBytesToReceive;
            var progressValue = (float)downloadItem.DownloadedBytes/downloadItem.TotalBytes;
            downloadItem.ProgressValue = progressValue;
            downloadItem.EtaCalculator?.Update(progressValue);

            if (DateTime.Now - downloadItem.LastStatusUpdateTime > _updateProgressTimeSpan)
            {
                SetDownloadItemStatusText(downloadItem);
                downloadItem.LastStatusUpdateTime = DateTime.Now;
            }
        }
    }
}