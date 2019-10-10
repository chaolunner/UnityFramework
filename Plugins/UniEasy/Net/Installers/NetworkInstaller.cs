using UniEasy.DI;

namespace UniEasy.Net
{
    public class NetworkInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            Container.Bind<NetworkSystem>().FromInstance(NetworkSystem.GetInstance()).AsSingle();
        }
    }
}
