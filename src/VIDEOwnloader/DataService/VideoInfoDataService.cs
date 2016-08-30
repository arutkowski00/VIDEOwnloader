using System;
using System.Configuration;
using System.Threading.Tasks;
using VIDEOwnloader.Base;
using VIDEOwnloader.Base.Validation;
using VIDEOwnloader.Base.Video;

namespace VIDEOwnloader.DataService
{
    public class VideoInfoDataService : IDataService
    {
        public VideoInfoDataService()
        {
            Client = new VideoServiceClient(ConfigurationManager.AppSettings["VideoServiceClientEndpoint"]);
        }

        public VideoServiceClient Client { get; }

        #region IDataService Members

        public void GetValid(string url, Action<UrlValidationResponse, Exception> callback)
        {
            try
            {
                var response = Client.GetResponse(new UrlValidationRequest {Urls = new[] {url}});
                ValidateValidationResults(response, url);
                callback(response, null);
            }
            catch (Exception ex)
            {
                callback(null, ex);
            }
        }

        public async Task GetValidAsync(string url, Action<UrlValidationResponse, Exception> callback)
        {
            try
            {
                var response = await Client.GetResponseAsync(new UrlValidationRequest {Urls = new[] {url}});
                ValidateValidationResults(response, url);
                callback(response, null);
            }
            catch (Exception ex)
            {
                callback(null, ex);
            }
        }

        public void GetVideo(string url, Action<VideoInfoResponse, Exception> callback)
        {
            try
            {
                var response = Client.GetResponse(new VideoInfoRequest {Urls = new[] {url}});
                callback(response, null);
            }
            catch (Exception ex)
            {
                callback(null, ex);
            }
        }

        public async Task GetVideoAsync(string url, Action<VideoInfoResponse, Exception> callback)
        {
            try
            {
                var response = await Client.GetResponseAsync(new VideoInfoRequest {Urls = new[] {url}});
                callback(response, null);
            }
            catch (Exception ex)
            {
                callback(null, ex);
            }
        }

        #endregion

        private static void ValidateValidationResults(UrlValidationResponse response, string url)
        {
            if ((response.ValidationResults == null) || (response.ValidationResults.Length == 0))
                response.ValidationResults = new[]
                {
                    new UrlValidationResult
                    {
                        Url = url,
                        IsValid = false
                    }
                };
        }
    }
}