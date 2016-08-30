using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using VIDEOwnloader.Base.Validation;
using VIDEOwnloader.Base.Video;
using VIDEOwnloader.Common;

namespace VIDEOwnloader.DataService
{
    public class VideoInfoLocalDataService : IDataService
    {
        private const string DefaultArguments = "";

        #region IDataService Members

        public void GetValid(string url, Action<UrlValidationResponse, Exception> callback)
        {
            try
            {
                var startInfo = new ProcessStartInfo("youtube-dl.exe")
                {
                    Arguments = $"--ignore-errors --get-url {url}",
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    RedirectStandardOutput = true
                };

                UrlValidationResult validationResult = null;
                using (var ydlProcess = Process.Start(startInfo))
                {
                    if (ydlProcess == null)
                    {
                        callback(null, new Exception("Cannot start validation process."));
                        return;
                    }
                    var output = ydlProcess.StandardOutput;
                    while (!output.EndOfStream)
                    {
                        // Read first line
                        var outputLine = output.ReadLine();
                        validationResult = new UrlValidationResult
                        {
                            IsValid = outputLine.IsValidUrl(),
                            Url = url
                        };
                        // First line should tell everything, so let's close the process.
                        ydlProcess.Close();
                    }
                    ydlProcess.WaitForExit();
                }

                if (validationResult == null)
                    validationResult = new UrlValidationResult
                    {
                        IsValid = false,
                        Url = url
                    };

                var response = new UrlValidationResponse
                {
                    ValidationResults = new[] {validationResult}
                };
                callback(response, null);
            }
            catch (Exception ex)
            {
                callback(null, ex);
            }
        }

        public Task GetValidAsync(string url, Action<UrlValidationResponse, Exception> callback)
        {
            return Task.Factory.StartNew(() => GetValid(url, callback));
        }

        public void GetVideo(string url, Action<VideoInfoResponse, Exception> callback)
        {
            try
            {
                var startInfo = new ProcessStartInfo("youtube-dl.exe")
                {
                    Arguments = $"--ignore-errors --ignore-config --flat-playlist --dump-single-json {url}",
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    RedirectStandardOutput = true
                };

                var response = new VideoInfoResponse();
                var videos = new List<Video>();
                var playlists = new List<Playlist>();
                using (var ydlProcess = Process.Start(startInfo))
                {
                    if (ydlProcess == null)
                    {
                        callback(null, new Exception("Cannot start info fetching process."));
                        return;
                    }
                    var output = ydlProcess.StandardOutput;
                    while (!output.EndOfStream)
                    {
                        var json = output.ReadLine();
                        if (json == "null") continue;
                        var jsonObject = JsonConvert.DeserializeObject<JObject>(json);
                        var jsonProperties = jsonObject.Children<JProperty>();
                        if (jsonProperties.All(x => x.Name != "_type"))
                        {
                            // TODO: check if no error
                            videos.Add(JsonConvert.DeserializeObject<Video>(json));
                        }
                        else
                        {
                            var modelType = ((dynamic)jsonObject)._type.ToString();
                            if (modelType == "playlist")
                                playlists.Add(JsonConvert.DeserializeObject<Playlist>(json));
                        }
                        //if (typeProperty == null)
                        //{
                        //    videos.Add(JsonConvert.DeserializeObject<Video>(json));
                        //}
                        //else if (typeProperty.Value.ToString() == "playlist")
                        //{
                        //    
                        //}
                    }
                    ydlProcess.WaitForExit();
                }
                response.Videos = videos.ToArray();
                response.Playlists = playlists.ToArray();
                callback(response, null);
            }
            catch (Exception ex)
            {
                callback(null, ex);
            }
        }

        public Task GetVideoAsync(string url, Action<VideoInfoResponse, Exception> callback)
        {
            return Task.Factory.StartNew(() => GetVideo(url, callback));
        }

        #endregion
    }
}