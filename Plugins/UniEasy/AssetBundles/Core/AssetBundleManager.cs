using System.Collections.Generic;
using System.Collections;
using UnityEngine;

namespace UniEasy
{
    public class AssetBundleManager : MonoBehaviour
    {
        private static AssetBundleManager instance;
        private Dictionary<string, MultiABLoader> container = new Dictionary<string, MultiABLoader>();
        private AssetBundleManifest manifest = null;

        public static AssetBundleManager GetInstance()
        {
            if (instance == null)
            {
                instance = new GameObject("AssetBundleManager").AddComponent<AssetBundleManager>();
            }
            return instance;
        }

        void Awake()
        {
            StartCoroutine(ABManifestLoader.GetInstance().LoadMainifestFile());
        }

        public IEnumerator WaitMainifestLoaded()
        {
            while (!ABManifestLoader.GetInstance().IsLoadCompleted)
            {
                yield return null;
            }

            manifest = ABManifestLoader.GetInstance().GetABManifest();
            if (manifest == null)
            {
                Debug.LogError(GetType() + "/LoadAssetBundle()/manifest(field) is null, please make sure manifest file loaded first!");
                yield return null;
            }
        }

        public IEnumerator DownloadAssetBundle(ABLoadCompleted onLoadCompleted)
        {
            yield return StartCoroutine(WaitMainifestLoaded());

            foreach (var abName in manifest.GetAllAssetBundles())
            {
                var sceneName = abName.Substring(0, abName.IndexOf("/"));
                LoadAssetBundle(sceneName, abName, onLoadCompleted);
                Debug.Log(sceneName + " : " + abName);
            }
        }

        public IEnumerator LoadAssetBundle(string sceneName, string abName, ABLoadCompleted onLoadCompleted)
        {
            if (string.IsNullOrEmpty(sceneName) || string.IsNullOrEmpty(abName))
            {
                Debug.LogError(GetType() + "/LoadAssetBundle()/sceneName or abName is null, please check it!");
                yield return null;
            }

            yield return StartCoroutine(WaitMainifestLoaded());

            if (!container.ContainsKey(sceneName))
            {
                container.Add(sceneName, new MultiABLoader(sceneName, abName, manifest, onLoadCompleted));
            }

            var loader = container[sceneName];
            if (loader == null)
            {
                Debug.LogError(GetType() + "/LoadAssetBundle()/MultiABLoader is null, please check it!");
            }
            yield return loader.LoadAssetBundleByRecursive(abName);
        }

        public Object LoadAsset(string sceneName, string abName, string assetName, bool isCache)
        {
            if (container.ContainsKey(sceneName))
            {
                return container[sceneName].LoadAsset(abName, assetName, isCache);
            }
            Debug.LogError(GetType() + "/LoadAsset()/can't found the scene, can't load assets in the bundle, please check it! sceneName=" + sceneName);
            return null;
        }

        public void Dispose(string sceneName)
        {
            if (container.ContainsKey(sceneName))
            {
                container[sceneName].Dispose();
            }
            else
            {
                Debug.LogError(GetType() + "/Dispose()/can't found the scene, can't dispose assets in the bundle, please check it! sceneName=" + sceneName);
            }
        }
    }
}
