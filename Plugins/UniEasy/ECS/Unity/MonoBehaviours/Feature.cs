using System.Collections.Generic;
using UniRx;

namespace UniEasy.ECS
{
    public class Feature : SystemBehaviour
    {
        [Reorderable("Runtime Systems"), DropdownMenu(typeof(RuntimeSystem)), BackgroundColor("#00408080")]
        public List<InspectableObjectData> RuntimeSystemsData = new List<InspectableObjectData>();

        private List<RuntimeSystem> runtimeSystems = new List<RuntimeSystem>();

        public List<RuntimeSystem> RuntimeSystems
        {
            get
            {
                if (runtimeSystems.Count == 0)
                {
                    foreach (var system in RuntimeSystemsData)
                    {
                        var runtimeSystem = system.CreateInstance() as RuntimeSystem;

                        if (runtimeSystem != null)
                        {
                            runtimeSystems.Add(runtimeSystem);
                        }
                    }
                }
                return runtimeSystems;
            }
        }

        public override void Initialize(IEventSystem eventSystem, IPoolManager poolManager, GroupFactory groupFactory, PrefabFactory prefabFactory)
        {
            base.Initialize(eventSystem, poolManager, groupFactory, prefabFactory);

            foreach (var system in RuntimeSystems)
            {
                system.Initialize(eventSystem, poolManager, groupFactory, prefabFactory);

                system.IsOn.DistinctUntilChanged().Where(_ => this.isActiveAndEnabled).Subscribe(b =>
                {
                    if (b)
                    {
                        system.OnEnable();
                    }
                    else
                    {
                        system.OnDisable();
                    }
                }).AddTo(this.Disposer);
            }
        }

        public override void OnEnable()
        {
            base.OnEnable();

            foreach (var system in RuntimeSystems)
            {
                if (system.IsOn.Value)
                {
                    system.OnEnable();
                }
            }
        }

        public override void OnDisable()
        {
            base.OnDisable();

            foreach (var system in RuntimeSystems)
            {
                if (system.IsOn.Value)
                {
                    system.OnDisable();
                }
            }
        }

        public override void OnDestroy()
        {
            base.OnDestroy();

            foreach (var system in RuntimeSystems)
            {
                system.OnDestroy();
            }
        }

        public override void Dispose()
        {
            base.Dispose();

            foreach (var system in RuntimeSystems)
            {
                system.Dispose();
            }
        }
    }
}
