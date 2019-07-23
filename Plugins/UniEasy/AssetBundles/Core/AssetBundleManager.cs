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
        private ulong contentBytes;
        private ulong downloadedBytes;
        private float downloadProgress;
        private Dictionary<string, MultiABLoader> container = new Dictionary<string, MultiABLoader>();
        private Dictionary<string, ABDownloader> downloadContainer = new Dictionary<string, ABDownloader>();
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

        public bool IsDownloading { get { return downloadContainer.Count > 0; } }

        /// <param name="unit">0=byte, 1=KB, 2=MB, 3=GB</param>
        public float GetContentSize(int unit = 1)
        {
            return contentBytes / Mathf.Pow(1024f, unit);
        }

        /// <param name="unit">0=byte, 1=KB, 2=MB, 3=GB</param>
        public float GetDownloadedSize(int unit = 1)
        {
            return downloadedBytes / Mathf.Pow(1024f, unit);
        }

        public int GetDownloadProgress()
        {
            return Mathf.RoundToInt(downloadProgress * 100);
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
            if (IsDownloading) { yield break; }

            yield return StartCoroutine(WaitUntilMainifestLoad());

            foreach (var abName in manifest.GetAllAssetBundles())
            {
                var abHash = manifest.GetAssetBundleHash(abName);
                var downloader = new ABDownloader(abName, abHash);
                downloadContainer.Add(abName, downloader);
                StartCoroutine(downloader.LoadAssetBundle());
            }
            downloadProgress = 0;
            while (downloadProgress < 1)
            {
                contentBytes = 0;
                downloadedBytes = 0;
                var progress = 0f;
                foreach (var downloader in downloadContainer.Values)
                {
                    contentBytes += downloader.ContentBytes;
                    downloadedBytes += downloader.DownloadedBytes;
                    progress += downloader.DownloadProgress;
                }
                downloadProgress = progress / downloadContainer.Count;
                yield return null;
            }
            Resources.UnloadUnusedAssets();
            System.GC.Collect();
            downloadContainer.Clear();
        }

        public IEnumerator LoadAssetBundle(string sceneName, string abName, ABLoadCompleted onLoadCompleted = null)
        {
            if (string.IsNullOrEmpty(sceneName) || string.IsNullOrEmpty(abName))
            {
                Debug.LogError(GetType() + "/LoadAssetBundle()/sceneName or abName is null, please check it!");
                yield return null;
            }

            yield return StartCoroutine(WaitUntilMainifestLoad());

            if (!container.ContainsKey(sceneName))
            {
                container.Add(sceneName, new MultiABLoader(abName, manifest, onLoadCompleted));
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
            container.Remove(sceneName);
        }

        public void DisposeUnloaded(params string[] excludeScenes)
        {
            var loadedScenes = new List<string>();
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                loadedScenes.Add(SceneManager.GetSceneAt(i).name);
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
