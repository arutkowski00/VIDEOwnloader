using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VIDEOwnloader.Common.Downloader
{
    public class DownloadProgressChangedEventArgs
    {
        public DownloadSession Session { get; }

        public DownloadProgressChangedEventArgs(DownloadSession session)
        {
            Session = session;
        }
    }
}
