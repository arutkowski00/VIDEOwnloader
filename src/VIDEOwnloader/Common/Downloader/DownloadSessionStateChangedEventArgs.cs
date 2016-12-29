using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VIDEOwnloader.Common.Downloader
{
    public class DownloadSessionStateChangedEventArgs
    {
        public DownloadSession Session { get; }
        public DownloadState OldState { get; }
        public DownloadState NewState { get; }

        public DownloadSessionStateChangedEventArgs(DownloadSession session, DownloadState oldState, DownloadState newState)
        {
            Session = session;
            OldState = oldState;
            NewState = newState;
        }
    }
}
