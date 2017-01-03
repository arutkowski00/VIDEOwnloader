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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
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
        private RelayCommand<DownloadItem> _pauseDownloadCommand;
        private RelayCommand<DownloadItem> _removeDownloadCommand;
        private RelayCommand<DownloadItem> _unpauseDownloadCommand;

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
            Application.Current.Exit += (sender, args) => Cleanup();
        }

        public bool AreItemsInCompletedList => CompletedList.Count > 0;
        public bool AreItemsInDownloadList => DownloadList.Count > 0;

        public RelayCommand<DownloadItem> CancelDownloadCommand
            => _cancelDownloadCommand ?? (_cancelDownloadCommand = new RelayCommand<DownloadItem>(CancelDownload));

        public ObservableCollection<DownloadItem> CompletedList { get; private set; } =
            new ObservableCollection<DownloadItem>();

        public ObservableCollection<DownloadItem> DownloadList { get; private set; } =
            new ObservableCollection<DownloadItem>();

        public bool IsDownloadListEmpty => DownloadList.Count == 0;

        public RelayCommand NewDownloadCommand
            => _newDownloadCommand ?? (_newDownloadCommand = new RelayCommand(ShowNewDownloadDialog));

        public RelayCommand<DownloadItem> PauseDownloadCommand
            => _pauseDownloadCommand ?? (_pauseDownloadCommand = new RelayCommand<DownloadItem>(PauseDownloadItem));

        public RelayCommand<DownloadItem> RemoveDownloadCommand
            => _removeDownloadCommand ?? (_removeDownloadCommand = new RelayCommand<DownloadItem>(RemoveDownloadItem));

        public RelayCommand<DownloadItem> UnpauseDownloadCommand
            =>
            _unpauseDownloadCommand ?? (_unpauseDownloadCommand = new RelayCommand<DownloadItem>(UnpauseDownloadItem));

        private IDataService DataService { get; }

        public override void Cleanup()
        {
            base.Cleanup();
            MessengerInstance.Unregister(this);
            Settings.Default.CompletedListXml = CompletedList.Count > 0
                ? XmlTools.Serialize(CompletedList.ToArray())
                : string.Empty;

            if (DownloadList.Count > 0)
            {
                var downloadItems = DownloadList.ToArray();
                var tasks = new List<Task>();
                foreach (var downloadItem in downloadItems)
                {
                    var cancelTask =
                        downloadItem.CancelAsync().ContinueWith(task => SetDownloadItemStatusText(downloadItem));
                    tasks.Add(cancelTask);
                }
                Task.WaitAll(tasks.ToArray());
                Settings.Default.DownloadListXml = XmlTools.Serialize(downloadItems);
            }
            else Settings.Default.DownloadListXml = string.Empty;
            Settings.Default.Save();
        }

        private async void CancelDownload(DownloadItem item)
        {
            var cancelMethod = item?.CancelAsync();
            if (cancelMethod != null) await cancelMethod;
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

        private void DownloadItem_StateChanged(object sender, DownloadSessionStateChangedEventArgs e)
        {
            var downloadItem = e.Session as DownloadItem;
            if (downloadItem == null) return;

            if (((e.OldState == DownloadState.Downloading) || (e.OldState == DownloadState.Paused) ||
                 (e.OldState == DownloadState.Ready)) &&
                ((e.NewState == DownloadState.Cancelled) || (e.NewState == DownloadState.Failed) ||
                 (e.NewState == DownloadState.Success)))
            {
                DownloadList.Remove(downloadItem);
                CompletedList.Add(downloadItem);
            }
            else if (CompletedList.Contains(downloadItem))
            {
                CompletedList.Remove(downloadItem);
                DownloadList.Add(downloadItem);
            }
            SetDownloadItemStatusText(downloadItem);
        }

        private async void HandleNewVideoDownload(VideoDownloadItem downloadItem)
        {
            downloadItem.DownloadProgressChanged += DownloadItem_DownloadProgressChanged;
            downloadItem.StateChanged += DownloadItem_StateChanged;
            //downloadItem.DownloadSession.DownloadFileCompleted += WebClientOnDownloadFileCompleted;
            DownloadList.Add(downloadItem);
            await downloadItem.DownloadAsync(false);
        }

        private async void PauseDownloadItem(DownloadItem downloadItem)
        {
            await downloadItem.PauseAsync();
            SetDownloadItemStatusText(downloadItem);
        }

        private void RemoveDownloadItem(DownloadItem downloadItem)
        {
            if ((downloadItem == null) || !downloadItem.CanBeRemoved) return;
            DownloadList.Remove(downloadItem);
            CompletedList.Remove(downloadItem);
        }


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

        private void SetDownloadItemStatusText(DownloadItem downloadItem)
        {
            if (downloadItem == null)
                throw new ArgumentNullException(nameof(downloadItem));

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
                    statusText = downloadItem.DownloadCompletedStatusText;
                    break;
                case DownloadState.Paused:
                    statusText =
                        $"Paused, {downloadItem.DownloadedBytes/MegabytesMultiplier:F1}/{downloadItem.TotalBytes/MegabytesMultiplier:F1} MB";
                    break;
                case DownloadState.Cancelled:
                    statusText = "Cancelled";
                    break;
                case DownloadState.Failed:
                    statusText = downloadItem.Error == null
                        ? "Failed!"
                        : $"Error: {downloadItem.Error.GetFullMessage()}";
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

        private async void UnpauseDownloadItem(DownloadItem downloadItem)
        {
            await downloadItem.DownloadAsync(false);
        }
    }
}