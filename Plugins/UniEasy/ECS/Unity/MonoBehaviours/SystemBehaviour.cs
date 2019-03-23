using UnityEngine;
using UniEasy.DI;
using System;
using UniRx;

namespace UniEasy.ECS
{
    public class SystemBehaviour : MonoBehaviour, ISystem, IDisposable, IDisposableContainer
    {
        public IEventSystem EventSystem { get; set; }

        public IPoolManager PoolManager { get; set; }

        public GroupFactory GroupFactory { get; set; }

        public PrefabFactory PrefabFactory { get; set; }

        private CompositeDisposable disposer = new CompositeDisposable();

        public CompositeDisposable Disposer
        {
            get { return disposer; }
            set { disposer = value; }
        }

        [Inject]
        public virtual void Initialize(IEventSystem eventSystem, IPoolManager poolManager, GroupFactory groupFactory, PrefabFactory prefabFactory)
        {
            EventSystem = eventSystem;
            PoolManager = poolManager;
            GroupFactory = groupFactory;
            PrefabFactory = prefabFactory;
        }

        public virtual void OnEnable()
        {
        }

        public virtual void OnDisable()
        {
            Disposer.Clear();
        }

        public virtual void OnDestroy()
        {
            Dispose();
        }

        public virtual void Dispose()
        {
            Disposer.Dispose();
        }
    }
}
