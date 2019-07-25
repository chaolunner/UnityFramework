using System.Collections.Generic;
using UnityEngine.Networking;
using UnityEngine;

namespace UniEasy
{
    public static class ABLoaderManager
    {
        private static Dictionary<string, ABLoader> Loaders = new Dictionary<string, ABLoader>();
        private static Dictionary<string, ABDownloader> Downloaders = new Dictionary<string, ABDownloader>();

        public static ABLoader Create(string abName, Hash128 abHash = new Hash128())
        {
            if (!Loaders.ContainsKey(abName))
            {
                Loaders.Add(abName, new ABLoader(abName, abHash, OnDownloadStart, OnDownloadUpdate, OnDownloadCompleted));
            }
            if (!Downloaders.ContainsKey(abName))
            {
                Downloaders.Add(abName, new ABDownloader(abName, abHash));
            }
            return Loaders[abName];
        }

        public static void OnDownloadStart(string abName, UnityWebRequest uwr)
        {
            var downloader = Downloaders[abName];
            string size = uwr.GetResponseHeader("Content-Length");
            downloader.ContentBytes = System.Convert.ToUInt64(size);
            downloader.DownloadedBytes = uwr.downloadedBytes;
            if (downloader.ContentBytes > 0 && downloader.DownloadedBytes == downloader.ContentBytes)
            {
                downloader.DownloadedBytes = (ulong)(downloader.DownloadedBytes * 0.99f);
            }
            downloader.DownloadProgress = Mathf.Clamp(downloader.ContentBytes == 0 ? uwr.downloadProgress : (float)downloader.DownloadedBytes / downloader.ContentBytes, 0, 0.99f);
        }

        public static void OnDownloadUpdate(string abName, UnityWebRequest uwr)
        {
            var downloader = Downloaders[abName];
            if (downloader.ContentBytes == 0)
            {
                string size = uwr.GetResponseHeader("Content-Length");
                downloader.ContentBytes = System.Convert.ToUInt64(size);
            }
            downloader.DownloadedBytes = uwr.downloadedBytes;
            if (downloader.ContentBytes > 0 && downloader.DownloadedBytes == downloader.ContentBytes)
            {
                downloader.DownloadedBytes = (ulong)(downloader.DownloadedBytes * 0.99f);
            }
            downloader.DownloadProgress = Mathf.Clamp(downloader.ContentBytes == 0 ? uwr.downloadProgress : (float)downloader.DownloadedBytes / downloader.ContentBytes, 0, 0.99f);
        }

        public static void OnDownloadCompleted(string abName)
        {
            var downloader = Downloaders[abName];
            downloader.DownloadedBytes = downloader.ContentBytes;
            downloader.DownloadProgress = 1;
        }

        /// <param name="unit">0=byte, 1=KB, 2=MB, 3=GB</param>
        public static float GetContentSize(int unit = 1)
        {
            ulong contentBytes = 0;
            foreach (var kvp in Downloaders)
            {
                contentBytes += kvp.Value.ContentBytes;
            }
            return contentBytes / Mathf.Pow(1024f, unit);
        }

        /// <param name="unit">0=byte, 1=KB, 2=MB, 3=GB</param>
        public static float GetDownloadedSize(int unit = 1)
        {
            ulong downloadedBytes = 0;
            foreach (var kvp in Downloaders)
            {
                downloadedBytes += kvp.Value.DownloadedBytes;
            }
            return downloadedBytes / Mathf.Pow(1024f, unit);
        }

        public static float GetDownloadProgress()
        {
            var downloadProgress = 0f;
            foreach (var kvp in Downloaders)
            {
                downloadProgress += kvp.Value.DownloadProgress;
            }
            if (Downloaders.Count > 0)
            {
                downloadProgress = downloadProgress / Downloaders.Count;
            }
            if (GetContentSize() > 0)
            {
                downloadProgress = Mathf.Min(downloadProgress, GetDownloadedSize() / GetContentSize());
            }
            return downloadProgress;
        }

        public static int GetDownloadIntProgress()
        {
            return Mathf.RoundToInt(GetDownloadProgress() * 100);
        }

        public static void Dispose(string abName)
        {
            if (Loaders.ContainsKey(abName))
            {
                Loaders[abName].Dispose();
                Loaders.Remove(abName);
            }
        }
    }
}
