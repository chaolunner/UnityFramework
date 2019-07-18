using UnityEngine.Networking;
using UnityEngine;

namespace UniEasy
{
    public class ABDownloader : ABLoader
    {
        public bool IsVersionCached;
        public ulong ContentBytes;
        public ulong DownloadedBytes;
        public float DownloadProgress;

        public ABDownloader(string abName, Hash128 abHash = new Hash128()) : base(abName, abHash)
        {
            onLoadStart = OnDownloadStart;
            onLoadUpdate = OnDownloadUpdate;
            onLoadCompleted = OnDownloadCompleted;
        }

        public virtual void OnDownloadStart(string abName, UnityWebRequest uwr)
        {
            IsVersionCached = Caching.IsVersionCached(abDownLoadPath, abHash);
            string size = uwr.GetResponseHeader("Content-Length");
            ContentBytes = System.Convert.ToUInt64(size);
            DownloadedBytes = uwr.downloadedBytes;
            DownloadProgress = IsVersionCached ? 1 : (ContentBytes == 0 ? uwr.downloadProgress : (float)DownloadedBytes / ContentBytes);
        }

        public virtual void OnDownloadUpdate(string abName, UnityWebRequest uwr)
        {
            if (ContentBytes == 0)
            {
                string size = uwr.GetResponseHeader("Content-Length");
                ContentBytes = System.Convert.ToUInt64(size);
            }
            DownloadedBytes = uwr.downloadedBytes;
            DownloadProgress = IsVersionCached ? 1 : (ContentBytes == 0 ? uwr.downloadProgress : (float)DownloadedBytes / ContentBytes);
        }

        public virtual void OnDownloadCompleted(string abName)
        {
            DownloadedBytes = ContentBytes;
            DownloadProgress = 1;
            Dispose();
        }
    }
}
