using UnityEngine;
using System;
using UniRx;

namespace UniEasy.ECS
{
    [Serializable, ContextMenuAttribute("ECS/RuntimeSystem")]
    public class RuntimeSystem : ISystem, IDisposableContainer, IDisposable
    {
        [HideInInspector]
        public BoolReactiveProperty IsOn = new BoolReactiveProperty(true);

        public IEventSystem EventSystem { get; set; }

        public IPoolManager PoolManager { get; set; }

        public GroupFactory GroupFactory { get; set; }

        public PrefabFactory PrefabFactory { get; set; }

        public CompositeDisposable Disposer { get; set; } = new CompositeDisposable();

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
