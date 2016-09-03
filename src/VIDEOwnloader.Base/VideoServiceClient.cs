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
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using VIDEOwnloader.Base.Validation;
using VIDEOwnloader.Base.Video;

namespace VIDEOwnloader.Base
{
    public class VideoServiceClient : IDisposable,
        IRestService<VideoInfoRequest, VideoInfoResponse>,
        IRestService<UrlValidationRequest, UrlValidationResponse>
    {
        private bool _disposedValue; // To detect redundant calls

        /// <summary>
        ///     Initializes a new instance of the VideoServiceClient class with a specific URI endpoint.
        /// </summary>
        /// <param name="endpoint">The service's endpoint</param>
        public VideoServiceClient(Uri endpoint)
        {
            Client = new HttpClient
            {
                BaseAddress = endpoint
            };
            Client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));
        }

        /// <summary>
        ///     Initializes a new instance of the VideoServiceClient class with a specific string endpoint.
        /// </summary>
        /// <param name="endpoint">The service's endpoint (as string)</param>
        public VideoServiceClient(string endpoint)
            : this(new Uri(endpoint))
        {
        }

        private HttpClient Client { get; }

        #region IDisposable Members

        /// <summary>
        ///     Disposes the managed and unmanaged resources of the HttpClient
        /// </summary>
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
        }

        #endregion

        #region IRestService<UrlValidationRequest,UrlValidationResponse> Members

        /// <summary>
        ///     Gets response from the service with url validation results.
        /// </summary>
        /// <param name="request">The URL validaton request</param>
        /// <returns>The URL validation response with validation results for each URL.</returns>
        public UrlValidationResponse GetResponse(UrlValidationRequest request)
        {
            return GetRestResponse<UrlValidationRequest, UrlValidationResponse>(request, "/valid");
        }

        /// <summary>
        ///     Gets response from the service with url validation results.
        /// </summary>
        /// <param name="request">The URL validaton request</param>
        /// <returns>The URL validation response with validation results for each URL.</returns>
        public Task<UrlValidationResponse> GetResponseAsync(UrlValidationRequest request)
        {
            return Task.Factory.StartNew(() => GetResponse(request));
        }

        #endregion

        #region IRestService<VideoInfoRequest,VideoInfoResponse> Members

        /// <summary>
        ///     Gets response from the service with videos and playlists informations.
        /// </summary>
        /// <param name="request">The video info request</param>
        /// <returns>The video info response with videos and playlists informations.</returns>
        public VideoInfoResponse GetResponse(VideoInfoRequest request)
        {
            return GetRestResponse<VideoInfoRequest, VideoInfoResponse>(request, "/videos");
        }

        /// <summary>
        ///     Gets response from the service with videos and playlists informations.
        /// </summary>
        /// <param name="request">The video info request</param>
        /// <returns>The video info response with videos and playlists informations.</returns>
        public Task<VideoInfoResponse> GetResponseAsync(VideoInfoRequest request)
        {
            return Task.Factory.StartNew(() => GetResponse(request));
        }

        #endregion

        private TRes GetRestResponse<TReq, TRes>(TReq request, string url)
        {
            var requestJson = JsonConvert.SerializeObject(request, Formatting.None);
            HttpContent content = new StringContent(requestJson, Encoding.UTF8, "application/json");
            var response = ThrowExceptionOrGetResult(Client.PostAsync(url, content));
            var responseJson = ThrowExceptionOrGetResult(response.Content.ReadAsStringAsync());
            var result = JsonConvert.DeserializeObject<TRes>(responseJson);
            return result;
        }

        private static T ThrowExceptionOrGetResult<T>(Task<T> response)
        {
            try
            {
                response.Wait();
            }
            catch (AggregateException ex)
            {
                if (response.Exception?.InnerException != null)
                    throw ex.InnerException;
            }
            return response.Result;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposedValue) return;
            if (disposing)
                Client.Dispose();

            _disposedValue = true;
        }
    }
}