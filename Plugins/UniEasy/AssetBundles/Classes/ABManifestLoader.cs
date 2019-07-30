using System.Collections.Generic;
using UnityEngine.Networking;
using System.Collections;
using UnityEngine;
using System.IO;

namespace UniEasy
{
    public class ABManifestLoader : System.IDisposable
    {
        private static ABManifestLoader instance;
        private AssetBundleManifest manifest;
        private string manifestName;
        private string manifestRemotePath;
        private string manifestLocalPath;
        private AssetBundle abReadManifest;
        public readonly List<string> AssetBundleList;
        public bool IsLoadCompleted { get; private set; }

        private ABManifestLoader()
        {
            manifestName = PathsUtility.GetPlatformName();
            manifestRemotePath = PathsUtility.GetWWWPath() + "/" + manifestName;
#if UNITY_EDITOR
            manifestLocalPath = PathsUtility.GetABOutPath() + "Cache/" + manifestName;
#else
            manifestLocalPath = PathsUtility.GetABOutPath() + "/" + manifestName;
#endif
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
            using (var uwr = UnityWebRequest.Get(manifestRemotePath))
            {
                yield return uwr.SendWebRequest();
                if (uwr.isNetworkError || uwr.isHttpError)
                {
                    Debug.Log(GetType() + "/LoadAssetBundle()/UnityWebRequest download error, please check it! Manifest URL: " + manifestRemotePath + " Error Message: " + uwr.error);
                }
                else
                {
                    var folderPath = Path.GetDirectoryName(manifestLocalPath);
                    if (!Directory.Exists(folderPath))
                    {
                        Directory.CreateDirectory(folderPath);
                    }
                    try
                    {
                        File.WriteAllBytes(manifestLocalPath, uwr.downloadHandler.data);
#if UNITY_IOS
                        UnityEngine.iOS.Device.SetNoBackupFlag(manifestLocalPath);
#endif
                    }
                    catch (System.Exception e)
                    {
                        Debug.LogError(GetType() + "/LoadAssetBundle()/Failed to save manifest data, please check it! Manifest Path: " + manifestLocalPath + " Error Message: " + e);
                    }
                }
            }
            if (File.Exists(manifestLocalPath))
            {
                var abCreateRequest = AssetBundle.LoadFromFileAsync(manifestLocalPath);
                yield return abCreateRequest;
                abReadManifest = abCreateRequest.assetBundle;
                manifest = abReadManifest.LoadAsset(ABDefine.ASSETBUNDLE_MANIFEST) as AssetBundleManifest;
                AssetBundleList.AddRange(manifest.GetAllAssetBundles());
                IsLoadCompleted = true;
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
