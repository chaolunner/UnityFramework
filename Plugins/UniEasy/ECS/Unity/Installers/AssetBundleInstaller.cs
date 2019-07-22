using UniEasy.DI;

namespace UniEasy
{
    public class AssetBundleInstaller : MonoInstaller
    {
        public override void InstallBindings() { }

        private void Awake()
        {
            StartCoroutine(AssetBundleManager.GetInstance().DownloadAssetBundle());
        }
    }
}
