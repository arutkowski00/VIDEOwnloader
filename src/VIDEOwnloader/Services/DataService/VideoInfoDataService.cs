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
using System.Configuration;
using System.Threading.Tasks;
using VIDEOwnloader.Base;
using VIDEOwnloader.Base.Validation;
using VIDEOwnloader.Base.Video;

namespace VIDEOwnloader.Services.DataService
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