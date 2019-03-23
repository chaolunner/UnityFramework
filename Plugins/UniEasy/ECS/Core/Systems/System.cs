using System.Collections;
using UniEasy.DI;
using System;
using UniRx;

namespace UniEasy.ECS
{
    public abstract class System : ISystem, IDisposableContainer, IDisposable
    {
        [Inject] public IEventSystem EventSystem { get; set; }

        [Inject] public IPoolManager PoolManager { get; set; }

        [Inject] public GroupFactory GroupFactory { get; set; }

        [Inject] public PrefabFactory PrefabFactory { get; set; }

        protected CompositeDisposable disposer = new CompositeDisposable();

        public CompositeDisposable Disposer
        {
            get { return disposer; }
            set { disposer = value; }
        }

        public virtual void Initialize()
        {

        }

        public virtual void Dispose()
        {
            Disposer.Dispose();
        }
    }
}
