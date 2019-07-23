using UnityEngine.SceneManagement;
using System.Collections;
using UniEasy.DI;

namespace UniEasy
{
    public class AssetBundleInstaller : MonoInstaller
    {
        public static string[] ExcludeList = new string[]
        {
            "lua",
        };

        public override void InstallBindings() { }

        private void Awake()
        {
            StartCoroutine(Initialize());
        }

        private IEnumerator Initialize()
        {
            yield return AssetBundleManager.GetInstance().DownloadAssetBundle();
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                OnSceneLoaded(SceneManager.GetSceneAt(i), i == 0 ? LoadSceneMode.Single : LoadSceneMode.Additive);
            }
        }

        private void OnEnable()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnDisable()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            AssetBundleManager.GetInstance().DisposeUnloaded(ExcludeList);
        }
    }
}
