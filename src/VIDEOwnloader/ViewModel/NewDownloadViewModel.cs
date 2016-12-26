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
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using GalaSoft.MvvmLight.CommandWpf;
using MaterialDesignThemes.Wpf;
using Microsoft.WindowsAPICodePack.Dialogs;
using VIDEOwnloader.Base.Video;
using VIDEOwnloader.Common;
using VIDEOwnloader.Common.Extensions;
using VIDEOwnloader.Model;
using VIDEOwnloader.Services.DataService;
using VIDEOwnloader.View.Dialog;

namespace VIDEOwnloader.ViewModel
{
    public class NewDownloadViewModel : ValidationViewModelBase
    {
        public const bool TestPlaylist = false;

        public NewDownloadViewModel(IDataService dataService)
        {
            DataService = dataService;
#pragma warning disable 162
            // ReSharper disable HeuristicUnreachableCode
            // ReSharper disable ConditionIsAlwaysTrueOrFalse
            // ReSharper disable once InvertIf
            if (IsInDesignMode)
                if (TestPlaylist)
                {
                    DataService.GetVideoAsync(null, async (res, ex) =>
                    {
                        var playlist = new VideoPlaylist(res.Playlists.First());

                        await playlist[0].FillAsync(DataService);
                        playlist.SetFormat(playlist[0].Formats.OrderByDescending(x => x.Filesize).First());
                        playlist[1].IsFilling = true;

                        ResultItem = playlist;
                    }).Wait();
                    VideoSavePath = @"C:\test_movie.webm";
                }
                else
                {
                    DataService.GetVideoAsync(null, (res, ex) => { ResultItem = res.Videos.First(); }).Wait();
                    VideoSavePath =
                        @"C:\test_movie.webm";
                }
            // ReSharper restore HeuristicUnreachableCode
            // ReSharper restore ConditionIsAlwaysTrueOrFalse
#pragma warning restore 162
        }

        private IDataService DataService { get; }

        public IEnumerable<VideoFormat> AvailableFormats
        {
            get
            {
                if (ResultItem == null)
                    return null;

                IEnumerable<VideoFormat> formats;
                if (ResultItem is Video)
                    formats = ((Video)ResultItem).Formats;
                else if (ResultItem is VideoPlaylist)
                    formats = ((VideoPlaylist)ResultItem).Where(x => x.Formats != null)
                        .SelectMany(x => x.Formats).DistinctBy(x => x.FormatId);
                else return null;

                return formats.Where(f => f.AudioOnly || f.HasAudioAndVideo)
                    .OrderByDescending(f => f.Width*f.Height)
                    .ThenByDescending(f => f.Abr);
            }
        }

        public bool InvalidUrl
        {
            get { return Get(() => InvalidUrl); }
            set
            {
                Set(() => InvalidUrl, value);
                if (value)
                    AddError("Invalid URL.", () => Url);
                CommandManager.InvalidateRequerySuggested();
            }
        }

        public bool IsProcessing
        {
            get { return Get(() => IsProcessing); }
            set { Set(() => IsProcessing, value); }
        }

        public bool NoResultFound
        {
            get { return Get(() => NoResultFound); }
            set
            {
                Set(() => NoResultFound, value);
                if (value)
                    AddError("No result found.", () => Url);
                CommandManager.InvalidateRequerySuggested();
            }
        }

        public bool NoResultItem => ResultItem == null;

        public IVideoInfoResultItem ResultItem
        {
            get { return Get(() => ResultItem); }
            set
            {
                Set(() => ResultItem, value);
                RaisePropertyChanged(() => NoResultItem);
                RaisePropertyChanged(() => AvailableFormats);
                VideoSavePath = null;
                CommandManager.InvalidateRequerySuggested();
            }
        }

        public string Url
        {
            get { return Get(() => Url); }
            set
            {
                Set(() => Url, value);

                ClearErrors();
                if (string.IsNullOrEmpty(Url))
                    AddError("Field is required.");
                else if (!Url.IsValidUrl())
                    AddError("Invalid URL.");

                if (ResultItem != null)
                    ResultItem = null;
                if (NoResultFound)
                    NoResultFound = false;
                if (InvalidUrl)
                    InvalidUrl = false;
            }
        }

        public string VideoFilename
        {
            get
            {
                if (string.IsNullOrEmpty(VideoSavePath))
                    return string.Empty;
                if (ResultItem is Video)
                    return Path.GetFileName(VideoSavePath);
                return VideoSavePath;
            }
        }

        public VideoFormat VideoFormat
        {
            get { return Get(() => VideoFormat); }
            set
            {
                Set(() => VideoFormat, value);
                if (ResultItem is Video)
                    VideoSavePath = Path.ChangeExtension(VideoSavePath, value.Extension);
                else (ResultItem as VideoPlaylist)?.SetFormat(value);
                RaisePropertyChanged(() => VideoSavePath);
            }
        }

        public string VideoSavePath
        {
            get { return Get(() => VideoSavePath); }
            set
            {
                Set(() => VideoSavePath, value);
                RaisePropertyChanged(() => VideoFilename);
            }
        }

        public RelayCommand<bool> CloseDialogCommand
        {
            get
            {
                return Get(() => CloseDialogCommand, new RelayCommand<bool>(
                    isSuccess =>
                    {
                        if (isSuccess)
                            if (ResultItem is Video)
                            {
                                var downloadItem = new VideoDownloadItem
                                {
                                    Video = (Video)ResultItem,
                                    VideoFormat = VideoFormat,
                                    Filename = VideoSavePath
                                };
                                MessengerInstance.Send(downloadItem);
                            }
                            else if (ResultItem is VideoPlaylist)
                            {
                                // TODO: playlist send
                                //PlaylistDownloadItem downloadItem = new PlaylistDownloadItem();
                                //MessengerInstance.Send(downloadItem);
                            }

                        ResultItem = null;
                        DialogHost.CloseDialogCommand.Execute(isSuccess, null);
                    },
                    isSuccess => !isSuccess || ((ResultItem != null) && !HasErrors)));
            }
        }

        public RelayCommand GetVideoInfoCommand
        {
            get
            {
                return Get(() => GetVideoInfoCommand, new RelayCommand(
                    async () =>
                    {
                        IsProcessing = true;
                        await LoadVideoInfo(true);
                        IsProcessing = false;
                    },
                    () => !HasErrors));
            }
        }

        public RelayCommand LoadedCommand
        {
            get
            {
                return Get(() => LoadedCommand, new RelayCommand(
                    async () =>
                    {
                        IsProcessing = true;
                        if (Clipboard.ContainsText())
                        {
                            var clipboardText = Clipboard.GetText();
                            if (await IsValidVideoUrl(Clipboard.GetText()))
                            {
                                Url = clipboardText;
                                await LoadVideoInfo(false);
                            }
                        }
                        IsProcessing = false;
                    }));
            }
        }


        public RelayCommand ChooseSavePathCommand
        {
            get
            {
                return Get(() => ChooseSavePathCommand, new RelayCommand(
                    () =>
                    {
                        if (ResultItem == null)
                            return;
                        if (ResultItem is Video)
                        {
                            var saveDialog = new CommonSaveFileDialog
                            {
                                AlwaysAppendDefaultExtension = true,
                                DefaultFileName = GetVideoFileName(),
                                DefaultDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                                Title = "Choose media file save location"
                            };
                            saveDialog.Filters.Add(new CommonFileDialogFilter(VideoFormat.Description,
                                VideoFormat.Extension));
                            var result = saveDialog.ShowDialog(Application.Current.MainWindow);
                            if (result == CommonFileDialogResult.Ok)
                                VideoSavePath = saveDialog.FileName;
                        }
                        else if (ResultItem is VideoPlaylist)
                        {
                            var openFolderDialog = new CommonOpenFileDialog
                            {
                                DefaultDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                                IsFolderPicker = true,
                                Title = "Choose playlist save location"
                            };
                            var result = openFolderDialog.ShowDialog(Application.Current.MainWindow);
                            if (result == CommonFileDialogResult.Ok)
                                VideoSavePath = openFolderDialog.FileName;
                        }
                    }));
            }
        }

        private async Task<bool> IsValidVideoUrl(string url)
        {
            if (!url.IsValidUrl())
                return false;

            var isValid = false;
            await DataService.GetValidAsync(url, (res, ex) =>
            {
                if (ex != null)
                    isValid = false;
                else if (res.ValidationResults.Length > 0)
                    isValid = res.ValidationResults[0].IsValid;
            });

            return isValid;
        }

        private async Task LoadVideoInfo(bool validateUrl)
        {
            Exception getVideosException = null;
            await DialogHost.Show(new WaitDialog(), "NewDownloadDialogHost",
                (DialogOpenedEventHandler)(async (sender, e) =>
                {
                    if (validateUrl)
                        if (!await IsValidVideoUrl(Url))
                        {
                            InvalidUrl = true;
                            e.Session.Close();
                            return;
                        }

                    await DataService.GetVideoAsync(Url, (res, ex) =>
                    {
                        if (ex != null)
                            getVideosException = ex;
                        else if (res.Videos.Length > 0)
                        {
                            ResultItem = res.Videos[0];
                            VideoFormat = AvailableFormats.First();
                            // Set default save path
                            if (ResultItem != null)
                                VideoSavePath =
                                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                                        GetVideoFileName());
                        }
                        else if (res.Playlists.Length > 0)
                        {
                            // ResultItem = new VideoPlaylist(res.Playlists[0]);
                            // VideoSavePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), MakeValidFileName(ResultItem.Title));
                            ResultItem = null;
                            AddError("Playlists are not supported yet.", () => Url);
                        }
                        else
                        {
                            ResultItem = null;
                            NoResultFound = true;
                        }
                    });
                    e.Session.Close();
                }));

            if (getVideosException != null)
            {
                await ShowExceptionDialog(getVideosException, "Loading video info failed!");
                AddError("An error occured.", () => Url);
                return;
            }

            if (ResultItem is VideoPlaylist)
            {
                await ((VideoPlaylist)ResultItem).FillItemsInfo(DataService);
                RaisePropertyChanged(() => AvailableFormats);
            }

            if (AvailableFormats != null)
                VideoFormat = AvailableFormats.FirstOrDefault();
        }

        private string GetVideoFileName()
        {
            if ((ResultItem != null) && (VideoFormat != null))
                return MakeValidFileName($"{ResultItem.Title}_{ResultItem.Id}.{VideoFormat.Extension}");
            return ResultItem != null ? MakeValidFileName($"{ResultItem.Title}_{ResultItem.Id}.???") : null;
        }

        private async Task ShowExceptionDialog(Exception exc, string title)
        {
            await DialogHost.Show(new MessageDialog
            {
                Title = title,
                Message = exc.GetFullMessage(),
                DialogButton = MessageDialogButton.Close,
                MaxWidth = 480
            }, "NewDownloadDialogHost");
        }

        private static string MakeValidFileName(string name)
        {
            var invalidChars = Regex.Escape(new string(Path.GetInvalidFileNameChars()));
            var invalidRegStr = string.Format(@"([{0}]*\.+$)|([{0}]+)", invalidChars);

            return Regex.Replace(name, invalidRegStr, "_");
        }
    }
}