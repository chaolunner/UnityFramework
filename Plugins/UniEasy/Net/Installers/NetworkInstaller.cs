using UniEasy.DI;

namespace UniEasy.Net
{
    public class NetworkInstaller : MonoInstaller
    {
        private INetworkBroker networkBroker;
        private INetworkSystem networkSystem;

        public override void InstallBindings()
        {
            Container.Bind<INetworkBroker>().To<NetworkBroker>().AsSingle();
            Container.Bind<INetworkSystem>().To<NetworkSystem>().AsSingle();
            networkBroker = Container.Resolve<INetworkBroker>();
            networkSystem = Container.Resolve<INetworkSystem>();
        }

        private void Update()
        {
            networkBroker.Update();
        }

        private void OnDestroy()
        {
            networkSystem.Dispose();
        }
    }
}
