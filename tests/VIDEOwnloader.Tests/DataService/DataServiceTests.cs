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

using System.IO;
using NUnit.Framework;
using VIDEOwnloader.Services.DataService;
using VIDEOwnloader.Tests.Common;

namespace VIDEOwnloader.Test.DataService
{
    [TestFixture]
    public class DataServiceTests
    {
        private readonly IDataService _dataService = DataServiceHelper.GetLocalDataService();

        private readonly string[] _urls =
        {
            "https://www.youtube.com/watch?v=DPEJB-FCItk", // one video
            "https://www.youtube.com/playlist?list=PLbpi6ZahtOH66hnix9rPjEMxT3kNGXVTO", // one playlist
            "http://isitfridayyet.net/" // one invalid
        };

        [Test]
        public void GetValid_ShouldBeInvalid_WhenGivenBadUrl()
        {
            _dataService.GetValid(_urls[2], (response, exception) =>
            {
                if (exception != null) throw exception;
                Assert.IsNotNull(response, "response != null");
                Assert.IsNotEmpty(response.ValidationResults, "ValidationResults not empty");
                Assert.True(response.ValidationResults.Length == 1, "response.ValidationResults.Length == 1");
                Assert.False(response.ValidationResults[0].IsValid, "response.ValidationResults[0].IsValid == false");
            });
        }

        [Test]
        public void GetValid_ShouldBeValid_WhenGivenGoodUrl()
        {
            _dataService.GetValid(_urls[0], (response, exception) =>
            {
                if (exception != null) throw exception;
                Assert.IsNotNull(response, "response != null");
                Assert.IsNotEmpty(response.ValidationResults, "ValidationResults not empty");
                Assert.True(response.ValidationResults.Length == 1, "response.ValidationResults.Length == 1");
                Assert.True(response.ValidationResults[0].IsValid, "response.ValidationResults[0].IsValid");
            });
        }

        [Test]
        public void GetVideos_ShouldBeNothing_WhenGivenInvalidUrl()
        {
            _dataService.GetVideo(_urls[2], (response, exception) =>
            {
                if (exception != null) throw exception;
                Assert.IsNotNull(response, "response != null");
                Assert.IsEmpty(response.Playlists, "response.Playlists empty");
                Assert.IsEmpty(response.Videos, "response.Videos empty");
            });
        }

        [Test]
        public void GetVideos_ShouldBePlaylist_WhenGivenUrl()
        {
            _dataService.GetVideo(_urls[1], (response, exception) =>
            {
                if (exception != null) throw exception;
                Assert.IsNotNull(response, "response != null");
                Assert.IsNotEmpty(response.Playlists, "response.Playlists not empty");
                Assert.True(response.Videos.Length == 0, "response.Videos.Length == 0");
                Assert.True(response.Playlists.Length == 1, "response.Playlists.Length == 1");
                Assert.True(response.Playlists[0].Entries.Length > 0, "has at least one video");
            });
        }

        [Test]
        public void GetVideos_ShouldBeVideo_WhenGivenUrl()
        {
            _dataService.GetVideo(_urls[0], (response, exception) =>
            {
                if (exception != null) throw exception;
                Assert.IsNotNull(response, "response != null");
                Assert.IsNotEmpty(response.Videos, "response.Videos not empty");
                Assert.True(response.Videos.Length == 1, "response.Videos.Length == 1");
                Assert.True(response.Playlists.Length == 0, "response.Playlists.Length == 0");
            });
        }
    }
}