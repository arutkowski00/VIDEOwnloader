using System;
using System.IO;
using System.Threading.Tasks;
using NUnit.Framework;
using NUnit.Framework.Internal;
using VIDEOwnloader.Base.Video;
using VIDEOwnloader.Common.Downloader;
using VIDEOwnloader.Services.DataService;
using VIDEOwnloader.Tests.Common;

namespace VIDEOwnloader.Tests.Downloader
{
    [TestFixture]
    public class DownloadSessionTests
    {
        private readonly IDataService _dataService = DataServiceHelper.GetLocalDataService();
        private readonly Random random = new Random();
        private readonly Randomizer randomizer = new Randomizer();

        private async Task<VideoFormat> GetTestVideoFormat()
        {
            VideoFormat format = null;
            await _dataService.GetVideoAsync("https://www.youtube.com/watch?v=B7bqAsxee4I", (response, exception) =>
            {
                if (exception != null)
                    throw exception;

                Assert.AreEqual(1, response.Videos.Length);
                var formatsLength = response.Videos[0].Formats.Length;
                Assert.Greater(formatsLength, 0);
                format = response.Videos[0].Formats[random.Next(0, formatsLength)];
            });
            return format;
        }

        [Test]
        public async Task Download_ShouldBeSuccessfulAndFileExist_WhenGivenGoodUrl()
        {
            var downloadSession = await GetTestDownloadSession();
            await downloadSession.DownloadAsync();
            Assert.AreEqual(DownloadState.Success, downloadSession.State);
            Assert.True(File.Exists(downloadSession.TargetFileName));
            var fileInfo = new FileInfo(downloadSession.TargetFileName);
            Assert.AreEqual(downloadSession.TotalBytes, fileInfo.Length);
        }

        [Test]
        public async Task Download_ShouldBeCancelledAndFileNotExist_WhenCancelInvoked()
        {
            var downloadSession = await GetTestDownloadSession();
            downloadSession.DownloadProgressChanged += async (sender, args) =>
            {
                Assert.AreEqual(DownloadState.Downloading, downloadSession.State);
                // Cancel download session on the first download progress
                await args.Session.CancelAsync();
            };
            await downloadSession.DownloadAsync();
            Assert.AreEqual(DownloadState.Cancelled, downloadSession.State);
            Assert.False(File.Exists(downloadSession.PartFilename));
            Assert.False(File.Exists(downloadSession.TargetFileName));
        }

        [Test]
        public async Task Download_ShouldBeContinued_WhenPausedAndUnpaused()
        {
            var downloadSession = await GetTestDownloadSession();
            long downloadedBytes = 0;
            downloadSession.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName != nameof(downloadSession.DownloadedBytes)) return;

                Assert.AreNotEqual(0, downloadSession.DownloadedBytes);
                Assert.GreaterOrEqual(downloadSession.DownloadedBytes, downloadedBytes);
                downloadedBytes = downloadSession.DownloadedBytes;
            };

            DownloadProgressChangedEventHandler handler = null;
            handler = async (sender, args) =>
            {
                // Execute this only once
                downloadSession.DownloadProgressChanged -= handler;

                Assert.AreEqual(DownloadState.Downloading, downloadSession.State);

                // Pause download session on the first download progress
                await args.Session.PauseAsync();
            };
            downloadSession.DownloadProgressChanged += handler;
            await downloadSession.DownloadAsync();

            // It should be paused right now
            Assert.AreEqual(DownloadState.Paused, downloadSession.State);
            Assert.True(File.Exists(downloadSession.PartFilename));

            // Wait a couple seconds
            await Task.Delay(TimeSpan.FromSeconds(2));

            // Check if download has not progressed any further
            Assert.AreEqual(downloadedBytes, downloadSession.DownloadedBytes);

            // Continue download
            await downloadSession.DownloadAsync();

            Assert.AreEqual(DownloadState.Success, downloadSession.State);
            Assert.True(File.Exists(downloadSession.TargetFileName));
            var fileInfo = new FileInfo(downloadSession.TargetFileName);
            Assert.AreEqual(downloadSession.TotalBytes, fileInfo.Length);
        }

        private async Task<DownloadSession> GetTestDownloadSession()
        {
            var format = await GetTestVideoFormat();
            Assert.False(string.IsNullOrEmpty(format?.Url));
            var downloadUri = new Uri(format.Url);
            var targetFileName = randomizer.GetString(8) + "." + format.Extension;
            return new DownloadSession(downloadUri, targetFileName);
        }
    }
}