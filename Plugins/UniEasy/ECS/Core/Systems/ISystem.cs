using UniRx;

namespace UniEasy.ECS
{
    public interface ISystem
    {
        IEventSystem EventSystem { get; set; }

        IPoolManager PoolManager { get; set; }

        GroupFactory GroupFactory { get; set; }

        PrefabFactory PrefabFactory { get; set; }

        CompositeDisposable Disposer { get; set; }
    }
}
