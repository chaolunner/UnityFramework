using UniEasy.DI;
using UniRx;

namespace UniEasy.ECS
{
    public class ECSInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            Container.Bind<IMessageBroker>().To<MessageBroker>().AsSingle();
            Container.Bind<IEventSystem>().To<EventSystem>().AsSingle();
            Container.Bind<IIdentityGenerator>().To<SequentialIdentityGenerator>().AsSingle();
            Container.Bind<IPoolManager>().To<PoolManager>().AsSingle();
            Container.Bind<GroupFactory>().To<GroupFactory>().AsSingle();
            Container.Bind<PrefabFactory>().To<PrefabFactory>().AsSingle();
        }
    }
}
