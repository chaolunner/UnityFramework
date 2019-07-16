#if UNITY_2017_3_OR_NEWER
using UnityEngine.Networking;
#endif
using System.Collections;
using UnityEngine;

namespace UniEasy
{
    public class ABLoader : System.IDisposable
    {
        private AssetLoader assetLoader;
        private ABLoadCompleted onLoadCompleted;
        private string abName;
        private string abDownLoadPath;
        private Hash128 abHash;

        public ABLoader(string abName, Hash128 abHash = new Hash128(), ABLoadCompleted loadCompleted = null)
        {
            assetLoader = null;
            this.abName = abName;
            this.abHash = abHash;
            onLoadCompleted = loadCompleted;
            abDownLoadPath = PathsUtility.GetWWWPath() + "/" + this.abName;
            Debug.Log(abHash.ToString());
        }

        public IEnumerator LoadAssetBundle()
        {
#if UNITY_2017_3_OR_NEWER
            using (var uwr = new UnityWebRequest(abDownLoadPath))
            {
                uwr.downloadHandler = new DownloadHandlerAssetBundle(uwr.url, abHash, 0);
                yield return uwr.SendWebRequest();
                if (uwr.isNetworkError)
                {
                    Debug.LogError(GetType() + "/LoadAssetBundle()/UnityWebRequest download error, please check it! AssetBundle URL: " + abDownLoadPath + " Error Message: " + uwr.error);
                }
                else
                {
                    var bundle = DownloadHandlerAssetBundle.GetContent(uwr);
                    assetLoader = new AssetLoader(bundle);
                    onLoadCompleted?.Invoke(abName);
                }
            }
#else
            using (var www = new WWW.LoadFromCacheOrDownload(abDownLoadPath, abHash, 0))
            {
                yield return www;
                if (www.progress >= 1)
                {
                    var bundle = www.assetBundle;
                    if (bundle != null)
                    {
                        assetLoader = new AssetLoader(bundle);
                        onLoadCompleted?.Invoke(abName);
                    }
                    else
                    {
                        Debug.LogError(GetType() + "/LoadAssetBundle()/WWW download error, please check it! AssetBundle URL: " + abDownLoadPath + " Error Message: " + www.error);
                    }
                }
            }
#endif
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
                Debug.LogError(GetType() + "/DisposeUnused()/assetLoader(field) == null, please check it!");
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
                Debug.LogError(GetType() + "/Dispose()/assetLoader(field) == null, please check it!");
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
