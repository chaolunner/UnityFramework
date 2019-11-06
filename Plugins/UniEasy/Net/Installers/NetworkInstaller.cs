using UniEasy.DI;

namespace UniEasy.Net
{
    public class NetworkInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            Container.Bind<INetworkBroker>().To<NetworkBroker>().AsSingle();
            Container.Bind<INetworkSystem>().To<NetworkSystem>().AsSingle();
        }

        private void Update()
        {
            Container.Resolve<INetworkBroker>().RunOnMainThread();
        }

        private void OnDestroy()
        {
            Container.Resolve<INetworkSystem>().Dispose();
        }
    }
}
