using UnityEngine.SceneManagement;
using System.Collections;
using UniEasy.DI;

namespace UniEasy
{
    public class AssetBundleInstaller : MonoInstaller
    {
        private bool isInitialized;

        public string[] DontUnloadOnLoadList = new string[]
        {
            "luacore",
            "lua"
        };

        public override void InstallBindings()
        {
            Container.Bind<AssetBundleManager>().FromInstance(AssetBundleManager.GetInstance()).AsSingle();
            Container.Bind<ABManifestLoader>().FromInstance(ABManifestLoader.GetInstance()).AsSingle();
        }

        private void Awake()
        {
            StartCoroutine(Initialize());
        }

        private IEnumerator Initialize()
        {
            yield return AssetBundleManager.GetInstance().DownloadAssetBundle();
            isInitialized = true;
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
            if (isInitialized)
            {
                AssetBundleManager.GetInstance().DisposeUnloaded(DontUnloadOnLoadList);
            }
        }
    }
}
