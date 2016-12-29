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
using System.Linq;
using GalaSoft.MvvmLight.CommandWpf;
using MaterialDesignThemes.Wpf;
using VIDEOwnloader.Common;
using VIDEOwnloader.Common.Downloader;
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

        public ObservableCollection<DownloadItem> CompletedList { get; private set; } =
            new ObservableCollection<DownloadItem>();

        private IDataService DataService { get; }

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
                                new DesignVideoDownloadItem
                                {
                                    DestinationPath = "test." + (x.Ext ?? "mp4"),
                                    Video = x,
                                    VideoFormat = x.Formats[random.Next(0, x.Formats.Length - 1)],
                                    DownloadedBytes = (uint)(random.NextDouble()*4 + 2)*1048576,
                                    TotalBytes = (uint)(random.NextDouble()*10 + 8)*1048576,
                                    State = DownloadState.Downloading
                                }));

                CompletedList =
                    new ObservableCollection<DownloadItem>(
                        res.Videos.Select(
                            x => new DesignVideoDownloadItem
                            {
                                DestinationPath = "test." + (x.Ext ?? "mp4"),
                                Video = x,
                                VideoFormat = x.Formats[random.Next(0, x.Formats.Length - 1)],
                                State = DownloadState.Success
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
                    downloadItem.CancelAsync();
                    SetDownloadItemStatusText(downloadItem);
                }
                Settings.Default.DownloadListXml = XmlTools.Serialize(downloadItems);
            }
            else Settings.Default.DownloadListXml = string.Empty;
            Settings.Default.Save();
        }

        private void CancelDownload(DownloadItem item)
        {
            item?.CancelAsync();
        }

        private async void HandleNewVideoDownload(VideoDownloadItem downloadItem)
        {
            downloadItem.DownloadProgressChanged += DownloadItem_DownloadProgressChanged;
            //downloadItem.DownloadSession.DownloadFileCompleted += WebClientOnDownloadFileCompleted;
            DownloadList.Add(downloadItem);
            await downloadItem.DownloadAsync();

            if (downloadItem.State == DownloadState.Success)
            {
                DownloadList.Remove(downloadItem);
                CompletedList.Add(downloadItem);
            }
            // TODO: failures handling
            SetDownloadItemStatusText(downloadItem);
        }

        private void DownloadItem_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            var downloadItem = e.Session as DownloadItem;
            if (downloadItem == null)
                return;

            if (DateTime.Now - downloadItem.LastStatusUpdateTime > _updateProgressTimeSpan)
            {
                SetDownloadItemStatusText(downloadItem);
                downloadItem.LastStatusUpdateTime = DateTime.Now;
            }
        }

        private void RemoveDownloadItem(DownloadItem item)
        {
            if ((item == null) || !item.CanBeRemoved) return;
            DownloadList.Remove(item);
            CompletedList.Remove(item);
        }

        private void SetDownloadItemStatusText(DownloadItem downloadItem)
        {
            var statusText = string.Empty;
            //if (downloadItem.ErrorText != null)
            //{
            //    statusText = $"Error: {downloadItem.ErrorText.Decapitalize()}";
            //}

            switch (downloadItem.State)
            {
                case DownloadState.Ready:
                    statusText = downloadItem.IsDownloaded ? downloadItem.DownloadCompletedStatusText : "Preparing...";
                    break;
                case DownloadState.Downloading:
                    var progressText =
                        $"{downloadItem.DownloadedBytes/MegabytesMultiplier:F1}/{downloadItem.TotalBytes/MegabytesMultiplier:F1} MB";
                    if (downloadItem.EtaCalculator == null)
                        statusText = progressText;
                    else if (downloadItem.EtaCalculator.EtaIsAvailable)
                        statusText = progressText + $", {downloadItem.EtaCalculator.Etr.ToReadableString()}";
                    else statusText = progressText + ", estimating time...";
                    break;
                case DownloadState.Success:
                    break;
                case DownloadState.Paused:
                    statusText =
                        $"Paused, {downloadItem.DownloadedBytes/MegabytesMultiplier:F1}/{downloadItem.TotalBytes/MegabytesMultiplier:F1} MB";
                    break;
                case DownloadState.Cancelled:
                    statusText = "Cancelled";
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            downloadItem.StatusText = statusText;
        }

        private async void ShowNewDownloadDialog()
        {
            var view = new NewDownloadDialog();
            await DialogHost.Show(view, "RootDialog");
        }
    }
}