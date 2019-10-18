using UnityEngine;
using UniEasy.DI;

namespace UniEasy.ECS
{
    public class SceneInstaller : MonoInstaller
    {
        [Reorderable, BackgroundColor("#0080BF80")]
        public SystemBehaviour[] Systems = new SystemBehaviour[0];
        [Reorderable, BackgroundColor("#0060BF80")]
        public SystemBehaviour[] BindingSystems = new SystemBehaviour[0];
        [HideInInspector]
        public bool AutoUpdate = true;

        public override void InstallBindings()
        {
            foreach (var system in BindingSystems)
            {
                Container.Bind(system.GetType()).FromInstance(system).AsSingle();
                var feature = system as RuntimeFeature;
                if (feature != null)
                {
                    foreach (var runtimeSystem in feature.RuntimeSystems)
                    {
                        Container.Bind(runtimeSystem.GetType()).FromInstance(runtimeSystem).AsSingle();
                    }
                }
            }

            foreach (var system in Systems)
            {
                Container.Inject(system);
                var feature = system as RuntimeFeature;
                if (feature != null)
                {
                    foreach (var runtimeSystem in feature.RuntimeSystems)
                    {
                        Container.Inject(runtimeSystem);
                    }
                }
            }
        }

        public void PreLoadingSystems()
        {
            Systems = GetComponentsInChildren<SystemBehaviour>(true);
        }
    }
}
