#if UNITY_2017_3_OR_NEWER
using UnityEngine.Networking;
#endif
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
        public bool IsLoadCompleted { get; private set; }

        private ABManifestLoader()
        {
            manifestPath = PathsUtility.GetWWWPath() + "/" + PathsUtility.GetPlatformName();
            manifest = null;
            abReadManifest = null;
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
#if UNITY_2017_3_OR_NEWER
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
                    manifest = abReadManifest.LoadAsset(ABUtility.ASSETBUNDLE_MANIFEST) as AssetBundleManifest;
                    IsLoadCompleted = true;
                }
            }
#else
            using (var www = new WWW(manifestPath))
            {
                yield return www;
                if (www.progress >= 1)
                {
                    var bundle = www.assetBundle;
                    if (bundle != null)
                    {
                        abReadManifest = bundle;
                        manifest = abReadManifest.LoadAsset(ABUtility.ASSETBUNDLE_MANIFEST) as AssetBundleManifest;
                        IsLoadCompleted = true;
                    }
                    else
                    {
                        Debug.Log(GetType() + "/LoadMainifestFile()/WWW download error, please check it! Manifest URL: " + manifestPath + " Error Message: " + www.error);
                    }
                }

            }
#endif
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
    }
}
