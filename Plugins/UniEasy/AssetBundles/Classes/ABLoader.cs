using UnityEngine.Networking;
using System.Collections;
using UnityEngine;

namespace UniEasy
{
    public class ABLoader : System.IDisposable
    {
        private bool isLoading;
        private bool isDone;
        private AssetLoader assetLoader;
        private ABLoadStart onLoadStart;
        private ABLoadUpdate onLoadUpdate;
        private ABLoadCompleted onLoadCompleted;
        private string abName;
        private string abDownLoadPath;
        private Hash128 abHash;

        public ABLoader(string abName, Hash128 abHash = new Hash128(), ABLoadStart onLoadStart = null, ABLoadUpdate onLoadUpdate = null, ABLoadCompleted onLoadCompleted = null)
        {
            assetLoader = null;
            this.abName = abName;
            this.abHash = abHash;
            this.onLoadStart = onLoadStart;
            this.onLoadUpdate = onLoadUpdate;
            this.onLoadCompleted = onLoadCompleted;
            abDownLoadPath = PathsUtility.GetWWWPath() + "/" + this.abName;
        }

        public IEnumerator LoadAssetBundle()
        {
            while (isLoading)
            {
                yield return null;
            }

            if (assetLoader != null)
            {
                onLoadCompleted?.Invoke(abName);
                yield break;
            }

            isLoading = true;
            isDone = false;

            using (var uwr = new UnityWebRequest(abDownLoadPath))
            {
                uwr.downloadHandler = new DownloadHandlerAssetBundle(uwr.url, abHash, 0);
                uwr.SendWebRequest();
                onLoadStart?.Invoke(abName, uwr);
                while (!uwr.isDone)
                {
                    onLoadUpdate?.Invoke(abName, uwr);
                    yield return null;
                }
                if (uwr.isNetworkError || uwr.isHttpError)
                {
                    Debug.Log(GetType() + "/LoadAssetBundle()/UnityWebRequest download error, please check it! AssetBundle URL: " + abDownLoadPath + " Error Message: " + uwr.error);
                    isDone = Caching.IsVersionCached(abDownLoadPath, abHash);
                }
                else
                {
                    isDone = true;
                }
                if (isDone)
                {
                    var bundle = DownloadHandlerAssetBundle.GetContent(uwr);
                    assetLoader = new AssetLoader(bundle);
                    onLoadCompleted?.Invoke(abName);
                }
            }

            isLoading = false;
        }

        public Object LoadAsset(string assetName, bool isCache)
        {
            if (assetLoader != null)
            {
                return assetLoader.LoadAsset(assetName, isCache);
            }
            Debug.LogError(GetType() + "/LoadAsset()/assetLoader(field) == null, please check it!");
            return null;
        }

        public void UnloadAsset(Object asset)
        {
            if (assetLoader != null)
            {
                assetLoader.UnloadAsset(asset);
            }
            else
            {
                Debug.LogError(GetType() + "/UnloadAsset()/assetLoader(field) == null, please check it!");
            }
        }

        public void DisposeUnused()
        {
            if (assetLoader != null)
            {
                assetLoader.DisposeUnused();
                assetLoader = null;
            }
            else
            {
                Debug.LogError(GetType() + "/DisposeUnused()/assetLoader(field) == null, please check it! abName: " + abName);
            }
        }

        public void Dispose()
        {
            if (assetLoader != null)
            {
                assetLoader.Dispose();
                assetLoader = null;
            }
            else
            {
                Debug.LogError(GetType() + "/Dispose()/assetLoader(field) == null, please check it! abName: " + abName);
            }
        }

        public string[] GetAllAssetNames()
        {
            if (assetLoader != null)
            {
                return assetLoader.GetAllAssetNames();
            }
            Debug.LogError(GetType() + "/GetAllAssetNames()/assetLoader(field) == null, please check it!");
            return null;
        }
    }
}
