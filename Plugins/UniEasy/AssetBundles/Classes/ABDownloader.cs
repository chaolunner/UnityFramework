using UnityEngine;

namespace UniEasy
{
    public class ABDownloader
    {
        public ulong ContentBytes;
        public ulong DownloadedBytes;
        public float DownloadProgress;
        private string abName;
        private string abDownLoadPath;
        private Hash128 abHash;

        public bool IsVersionCached
        {
            get
            {
                return Caching.IsVersionCached(abDownLoadPath, abHash);
            }
        }

        public ABDownloader(string abName, Hash128 abHash)
        {
            this.abName = abName;
            this.abHash = abHash;
            abDownLoadPath = PathsUtility.GetWWWPath() + "/" + this.abName;
        }
    }
}
