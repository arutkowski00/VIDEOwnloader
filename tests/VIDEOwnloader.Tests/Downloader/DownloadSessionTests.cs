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

        private async Task<VideoFormat> GetTestVideoFormat(string url = null)
        {
            VideoFormat format = null;
            await
                _dataService.GetVideoAsync(url ?? "https://www.youtube.com/watch?v=B7bqAsxee4I",
                    (response, exception) =>
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

        private static void IsSuccessfulAndFileExists(DownloadSession downloadSession)
        {
            Assert.AreEqual(DownloadState.Success, downloadSession.State);
            Assert.True(File.Exists(downloadSession.TargetFileName));
            var fileInfo = new FileInfo(downloadSession.TargetFileName);
            Assert.AreEqual(downloadSession.TotalBytes, fileInfo.Length);
        }

        private static void IsCancelled(DownloadSession downloadSession)
        {
            Assert.AreEqual(DownloadState.Cancelled, downloadSession.State);
            Assert.False(File.Exists(downloadSession.PartFilename));
            Assert.False(File.Exists(downloadSession.TargetFileName));
        }

        private static void IsPaused(DownloadSession downloadSession)
        {
            Assert.AreEqual(DownloadState.Paused, downloadSession.State);
            Assert.True(File.Exists(downloadSession.PartFilename));
        }

        private async Task<DownloadSession> GetTestDownloadSession()
        {
            var format = await GetTestVideoFormat();
            Assert.False(string.IsNullOrEmpty(format?.Url));
            var downloadUri = new Uri(format.Url);
            var targetFileName = randomizer.GetString(8) + "." + format.Extension;
            return new DownloadSession(downloadUri, targetFileName);
        }

        [Test]
        public async Task Download_ShouldBeCancelled_WhenCancelledWhilePaused()
        {
            var downloadSession = await GetTestDownloadSession();

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
            IsPaused(downloadSession);

            // Wait a couple seconds
            await Task.Delay(TimeSpan.FromSeconds(2));

            // Continue download
            await downloadSession.CancelAsync();

            // then it should be cancelled and both target and partial files should not exist
            IsCancelled(downloadSession);
        }

        [Test]
        public async Task Download_ShouldBeCancelledAndFileNotExist_WhenCancelInvoked()
        {
            // given new download session with test video URI
            var downloadSession = await GetTestDownloadSession();
            // when I cancel the download on very first progress achieved...
            downloadSession.DownloadProgressChanged += async (sender, args) =>
            {
                Assert.AreEqual(DownloadState.Downloading, downloadSession.State);
                // Cancel download session on the first download progress
                await args.Session.CancelAsync();
            };
            // and I try to download the file
            await downloadSession.DownloadAsync();

            // then it should be cancelled and both target and partial files should not exist
            IsCancelled(downloadSession);
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
            IsPaused(downloadSession);

            // Wait a couple seconds
            await Task.Delay(TimeSpan.FromSeconds(2));

            // Check if download has not progressed any further
            Assert.AreEqual(downloadedBytes, downloadSession.DownloadedBytes);

            // Continue download
            await downloadSession.DownloadAsync();

            // then it should be successful and file with non-zero length should exist
            IsSuccessfulAndFileExists(downloadSession);
        }

        [Test]
        public void Download_ShouldBeFailedAndFileNotExist_WhenGivenBadUrl()
        {
            // given new download session with invalid URI
            var downloadUri = new Uri("https://www.y0u7ub3.c0m/w47ch?v=M29PRpdYbaM");
            var targetFileName = randomizer.GetString(8) + ".test";
            var downloadSession = new DownloadSession(downloadUri, targetFileName);

            // when I try to download the file
            // then it should throw an exception with no parameters...
            Assert.CatchAsync(async () => { await downloadSession.DownloadAsync(); });
            // and it shouldn't throw an exception with throwOnFailure = false...
            Assert.DoesNotThrowAsync(async () => { await downloadSession.DownloadAsync(false); });
            // ... and it should be marked as failed and file should not exists
            Assert.AreEqual(DownloadState.Failed, downloadSession.State);
            Assert.AreEqual(0, downloadSession.DownloadedBytes);
            Assert.IsNotNull(downloadSession.Error);
            Assert.False(File.Exists(downloadSession.TargetFileName));
        }

        [Test]
        public async Task Download_ShouldBeSuccessfulAndFileExist_WhenGivenGoodUrl()
        {
            // given new download session with test video URI
            var downloadSession = await GetTestDownloadSession();

            // when I try to download the file
            await downloadSession.DownloadAsync();

            // then it should be successful and file with non-zero length should exist
            IsSuccessfulAndFileExists(downloadSession);
        }
    }
}