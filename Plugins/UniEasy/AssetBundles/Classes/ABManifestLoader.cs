using System.Collections.Generic;
using UnityEngine.Networking;
using System.Collections;
using UnityEngine;

namespace UniEasy
{
    public class ABManifestLoader : System.IDisposable
    {
        private static ABManifestLoader instance;
        private AssetBundleManifest manifest;
        private string manifestPath;
        private AssetBundle abReadManifest;
        public readonly List<string> AssetBundleList;
        public bool IsLoadCompleted { get; private set; }

        private ABManifestLoader()
        {
            manifestPath = PathsUtility.GetWWWPath() + "/" + PathsUtility.GetPlatformName();
            manifest = null;
            abReadManifest = null;
            AssetBundleList = new List<string>();
            IsLoadCompleted = false;
        }

        public static ABManifestLoader GetInstance()
        {
            if (instance == null)
            {
                instance = new ABManifestLoader();
            }
            return instance;
        }

        public IEnumerator LoadMainifestFile()
        {
            using (var uwr = new UnityWebRequest(manifestPath))
            {
                uwr.downloadHandler = new DownloadHandlerAssetBundle(uwr.url, 0);
                yield return uwr.SendWebRequest();
                if (uwr.isNetworkError)
                {
                    Debug.Log(GetType() + "/LoadAssetBundle()/UnityWebRequest download error, please check it! Manifest URL: " + manifestPath + " Error Message: " + uwr.error);
                }
                else
                {
                    abReadManifest = DownloadHandlerAssetBundle.GetContent(uwr);
                    manifest = abReadManifest.LoadAsset(ABDefine.ASSETBUNDLE_MANIFEST) as AssetBundleManifest;
                    AssetBundleList.AddRange(manifest.GetAllAssetBundles());
                    IsLoadCompleted = true;
                }
            }
        }

        public AssetBundleManifest GetABManifest()
        {
            if (IsLoadCompleted)
            {
                if (manifest != null)
                {
                    return manifest;
                }
                else
                {
                    Debug.Log(GetType() + "/GetABManifest()/manifest(field) == null, please check it!");
                }
            }
            else
            {
                Debug.Log(GetType() + "/GetABManifest()/IsLoadCompleted == false, manifest hasn't been downloaded yet!");
            }
            return null;
        }

        public string[] GetAllDependencies(string abName)
        {
            if (manifest != null && !string.IsNullOrEmpty(abName))
            {
                return manifest.GetAllDependencies(abName);
            }
            return null;
        }

        public void Dispose()
        {
            if (abReadManifest != null)
            {
                abReadManifest.Unload(true);
            }
        }

        public bool HasAssetBundle(string abName)
        {
            return AssetBundleList.Contains(abName);
        }
    }
}
