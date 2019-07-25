using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using System.Linq;

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
                DontDestroyOnLoad(instance);
            }
            return instance;
        }

        private void Awake()
        {
            StartCoroutine(ABManifestLoader.GetInstance().LoadMainifestFile());
        }

        private IEnumerator WaitUntilMainifestLoad()
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

        public IEnumerator DownloadAssetBundle()
        {
            yield return WaitUntilMainifestLoad();

            foreach (var abName in ABManifestLoader.GetInstance().AssetBundleList)
            {
                var sceneName = abName.Substring(0, abName.IndexOf("/"));
                StartCoroutine(LoadAssetBundle(sceneName, abName));
            }
            while (ABLoaderManager.GetDownloadProgress() < 1) { yield return null; }
            Resources.UnloadUnusedAssets();
            System.GC.Collect();
        }

        public IEnumerator LoadAssetBundle(string sceneName, string abName, ABLoadCompleted onLoadCompleted = null)
        {
            if (string.IsNullOrEmpty(sceneName) || string.IsNullOrEmpty(abName))
            {
                Debug.LogError(GetType() + "/LoadAssetBundle()/sceneName or abName is null, please check it!");
                yield return null;
            }

            yield return WaitUntilMainifestLoad();

            if (!ABManifestLoader.GetInstance().HasAssetBundle(abName)) { yield break; }

            if (!container.ContainsKey(sceneName))
            {
                container.Add(sceneName, new MultiABLoader(abName, manifest, onLoadCompleted));
            }

            var loader = container[sceneName];
            if (loader == null)
            {
                Debug.LogError(GetType() + "/LoadAssetBundle()/MultiABLoader is null, please check it!");
            }
            yield return loader.LoadAssetBundle(abName);
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
            container.Remove(sceneName);
        }

        public void DisposeUnloaded(params string[] excludeScenes)
        {
            var loadedScenes = new List<string>();
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                loadedScenes.Add(SceneManager.GetSceneAt(i).name.ToLower());
            }
            foreach (var sceneName in container.Keys.ToList())
            {
                if (excludeScenes.Contains(sceneName))
                {
                    continue;
                }
                if (!loadedScenes.Contains(sceneName))
                {
                    Dispose(sceneName);
                }
            }
        }

        private void OnDestroy()
        {
            foreach (var sceneName in container.Keys.ToList())
            {
                Dispose(sceneName);
            }
        }
    }
}
