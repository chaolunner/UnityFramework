using System.Collections.Generic;
using UnityEngine;
using UniEasy.DI;

namespace UniEasy.ECS
{
    public class ProjectInstaller : MonoInstaller
    {
        public static Dictionary<GameObject, List<object>> KernelObjects = new Dictionary<GameObject, List<object>>();

        public override void InstallBindings()
        {
            var prefabs = Resources.LoadAll<GameObject>("Kernel");
            foreach (var prefab in prefabs)
            {
                var wasActive = prefab.activeSelf;

                if (wasActive)
                {
                    prefab.SetActive(false);
                }

                var go = Object.Instantiate<GameObject>(prefab);
                go.name = prefab.name;
                KernelObjects.Add(go, new List<object>());
                var systems = go.GetComponentsInChildren<SystemBehaviour>(true);
                KernelObjects[go].AddRange(systems);
                foreach (var system in systems)
                {
                    Container.Bind(system.GetType()).FromInstance(system).AsSingle();
                    var feature = system as Feature;
                    if (feature != null)
                    {
                        foreach (var runtimeSystem in feature.RuntimeSystems)
                        {
                            Container.Bind(runtimeSystem.GetType()).FromInstance(runtimeSystem).AsSingle();
                        }
                    }
                }
                Object.DontDestroyOnLoad(go);
                if (wasActive)
                {
                    prefab.SetActive(true);
                }
            }

            var objs = KernelObjects.GetEnumerator();
            while (objs.MoveNext())
            {
                foreach (var o in objs.Current.Value)
                {
                    Container.Inject(o);
                    var feature = o as Feature;
                    if (feature != null)
                    {
                        foreach (var runtimeSystem in feature.RuntimeSystems)
                        {
                            Container.Inject(runtimeSystem);
                        }
                    }
                }
                objs.Current.Key.SetActive(true);
            }
        }
    }
}
