using NUnit.Framework;
using VIDEOwnloader.Services.DataService;

namespace VIDEOwnloader.Test.DataService
{
    [TestFixture]
    public class DataServiceTests
    {
        private readonly IDataService _dataService = new VideoInfoLocalDataService();

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
                Assert.IsNotNull(response, "response != null");
                Assert.IsNotEmpty(response.Playlists, "response.Playlists not empty");
                Assert.True(response.Videos.Length == 1, "response.Videos.Length == 1");
                Assert.True(response.Playlists.Length == 1, "response.Playlists.Length == 1");
            });
        }

        [Test]
        public void GetVideos_ShouldBeVideo_WhenGivenUrl()
        {
            _dataService.GetVideo(_urls[0], (response, exception) =>
            {
                Assert.IsNotNull(response, "response != null");
                Assert.IsNotEmpty(response.Videos, "response.Videos not empty");
                Assert.True(response.Videos.Length == 1, "response.Videos.Length == 1");
                Assert.True(response.Playlists.Length == 1, "response.Playlists.Length == 1");
            });
        }
    }
}