using System;
using System.Threading.Tasks;
using VIDEOwnloader.Base.Validation;
using VIDEOwnloader.Base.Video;

namespace VIDEOwnloader.DataService
{
    public interface IDataService
    {
        void GetValid(string url, Action<UrlValidationResponse, Exception> callback);
        Task GetValidAsync(string url, Action<UrlValidationResponse, Exception> callback);
        void GetVideo(string url, Action<VideoInfoResponse, Exception> callback);
        Task GetVideoAsync(string url, Action<VideoInfoResponse, Exception> callback);
    }
}