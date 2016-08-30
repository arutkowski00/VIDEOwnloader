using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using VIDEOwnloader.Base.Video;
using VIDEOwnloader.DataService;

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