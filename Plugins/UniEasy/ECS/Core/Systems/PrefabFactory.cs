using UnityEngine;
using UniEasy.DI;
using System;

namespace UniEasy.ECS
{
    public class PrefabFactory
    {
        [Inject]
        protected DiContainer Container = null;

        public GameObject Instantiate(GameObject prefab, Transform parent = null, bool worldPositionStays = false, Action<GameObject> action = null)
        {
            var wasActive = prefab.activeSelf;
            if (wasActive)
            {
                prefab.SetActive(false);
            }
            var go = GameObject.Instantiate(prefab, parent, worldPositionStays);
            go.name = prefab.name;
            action?.Invoke(go);
            if (wasActive)
            {
                prefab.SetActive(true);
                go.SetActive(true);
            }
            go.ForceActive();
            return go;
        }
    }
}
