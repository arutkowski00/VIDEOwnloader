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

using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VIDEOwnloader.Base.Video;
using VIDEOwnloader.Common;
using VIDEOwnloader.Services.DataService;

namespace VIDEOwnloader.Model
{
    public class VideoPlaylist : IReadOnlyCollection<VideoPlaylistItem>, IVideoInfoResultItem, INotifyCollectionChanged
    {
        private IReadOnlyList<VideoPlaylistItem> _playlistItems;

        public VideoPlaylist(Playlist playlist)
        {
            Playlist = playlist;
            InitializeCollection(playlist);
        }

        public VideoPlaylistItem this[int index] => _playlistItems[index];

        public Playlist Playlist { get; }

        #region INotifyCollectionChanged Members

        public event NotifyCollectionChangedEventHandler CollectionChanged;

        #endregion

        #region IReadOnlyCollection<VideoPlaylistItem> Members

        public int Count => _playlistItems.Count;

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)_playlistItems).GetEnumerator();
        }

        public IEnumerator<VideoPlaylistItem> GetEnumerator()
        {
            return _playlistItems.GetEnumerator();
        }

        #endregion

        #region IVideoInfoResultItem Members

        public string Extractor
        {
            get { return Playlist.Extractor; }
            set { Playlist.Extractor = value; }
        }

        public string ExtractorKey
        {
            get { return Playlist.ExtractorKey; }
            set { Playlist.ExtractorKey = value; }
        }

        public string Id
        {
            get { return Playlist.Id; }
            set { Playlist.Id = value; }
        }

        public string Title
        {
            get { return Playlist.Title; }
            set { Playlist.Title = value; }
        }

        public string WebpageUrl
        {
            get { return Playlist.WebpageUrl; }
            set { Playlist.WebpageUrl = value; }
        }

        public string WebpageUrlBasename
        {
            get { return Playlist.WebpageUrlBasename; }
            set { Playlist.WebpageUrlBasename = value; }
        }

        #endregion

        private void RaiseCollectionChanged(NotifyCollectionChangedAction act, object item)
        {
            CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(act, item));
        }

        private void RaiseCollectionChanged(NotifyCollectionChangedAction act, object item, int index)
        {
            CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(act, item, index));
        }

        private void ResetCollectionChanged()
        {
            CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        private void InitializeCollection(Playlist playlist)
        {
            var items = new VideoPlaylistItem[playlist.Entries.Length];
            for (var i = 0; i < playlist.Entries.Length; i++)
            {
                var entry = playlist.Entries[i];
                var item = new VideoPlaylistItem(entry.Id, entry.Title, entry.Url);
                item.PropertyChanged += PlaylistItemChanged;
                items[i] = item;
            }
            _playlistItems = items;
        }

        private void PlaylistItemChanged(object sender, PropertyChangedEventArgs e)
        {
            RaiseCollectionChanged(NotifyCollectionChangedAction.Replace, sender);
        }

        public async Task FillItemsInfo(IDataService dataService, CancellationToken cancellationToken = new CancellationToken())
        {
            var scheduler = new LimitedConcurrencyLevelTaskScheduler(5);
            var factory = new TaskFactory(scheduler);
            var fillTasks = _playlistItems.Select(item =>
                factory.StartNew(() =>
                {
                    item.Fill(dataService);
                    RaiseCollectionChanged(NotifyCollectionChangedAction.Replace, item);
                }, cancellationToken));
            await Task.WhenAll(fillTasks);
        }


        public void SetFormat(VideoFormat format)
        {
            for (var i = 0; i < _playlistItems.Count; i++)
            {
                var item = _playlistItems[i];
                if (!item.IsFilled)
                    item.SelectFormat(format.FormatId);
                RaiseCollectionChanged(NotifyCollectionChangedAction.Replace, item, i);
            }
        }
    }
}