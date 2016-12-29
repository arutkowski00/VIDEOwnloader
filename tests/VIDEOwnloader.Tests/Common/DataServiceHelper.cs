using System.IO;
using NUnit.Framework;
using VIDEOwnloader.Services.DataService;

namespace VIDEOwnloader.Tests.Common
{
    public static class DataServiceHelper
    {
        public static VideoInfoLocalDataService GetLocalDataService()
        {
            var dataService = new VideoInfoLocalDataService
            {
                YoutubeDlFilename = Path.Combine(TestContext.CurrentContext.TestDirectory, "youtube-dl.exe")
            };

            return dataService;
        }
    }
}