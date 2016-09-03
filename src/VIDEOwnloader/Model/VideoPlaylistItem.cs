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

using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using VIDEOwnloader.Base.Video;
using VIDEOwnloader.Services.DataService;

namespace VIDEOwnloader.Model
{
    public class VideoPlaylistItem : Video, INotifyPropertyChanged
    {
        private bool _isFilled;
        private bool _isFilling;
        private VideoFormat _selectedFormat;
        private Video _video;

        public VideoPlaylistItem(string id, string title, string url)
        {
            Video = new Video
            {
                Id = id,
                Title = title,
                Url = url
            };
        }

        public VideoPlaylistItem(string id, string title, string url, IDataService service)
            : this(id, title, url)
        {
            if (service != null)
                Fill(service);
        }

        public VideoPlaylistItem(Video video)
        {
            Video = video;
            if ((video.Formats.Length > 0) && !string.IsNullOrEmpty(video.Title) && !string.IsNullOrEmpty(video.Id))
                IsFilled = true;
        }

        public bool IsFilled
        {
            get { return _isFilled; }
            private set { Set(ref _isFilled, value); }
        }

        public bool IsFilling
        {
            get { return _isFilling; }
            set { Set(ref _isFilling, value); }
        }

        public VideoFormat SelectedFormat
        {
            get { return _selectedFormat; }
            private set { Set(ref _selectedFormat, value); }
        }

        public Video Video
        {
            get { return _video; }
            private set { Set(ref _video, value); }
        }

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        public void Fill(IDataService dataService)
        {
            if (IsFilled)
                return;

            IsFilling = true;
            dataService.GetVideo(Url, (res, ex) =>
            {
                if (ex != null)
                    throw ex;

                Video = res.Videos[0];
            });
            IsFilling = false;
            IsFilled = true;
        }

        public async Task FillAsync(IDataService dataService)
        {
            if (IsFilled)
                return;
            await Task.Factory.StartNew(() => Fill(dataService));
        }

        public void SelectFormat(string formatId)
        {
            SelectedFormat = Formats.FirstOrDefault(f => f.FormatId == formatId);
        }

        private void Set<T>(ref T storage, T value, [CallerMemberName] string propertyName = null)
        {
            if (!Equals(storage, value))
            {
                storage = value;
                RaisePropertyChanged(propertyName);
            }
        }

        private void RaisePropertyChanged([CallerMemberName] string propertyName = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}