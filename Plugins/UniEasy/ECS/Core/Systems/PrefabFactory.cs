using UnityEngine;
using UniEasy.DI;
using System;

namespace UniEasy.ECS
{
    public class PrefabFactory
    {
        [Inject]
        protected DiContainer Container = null;

        public GameObject Instantiate(GameObject prefab, Vector3 position, Quaternion rotation, Transform parent = null, Action<GameObject> action = null)
        {
            return Instantiate(prefab, position, rotation, parent, false, action);
        }

        public GameObject Instantiate(GameObject prefab, Transform parent = null, bool worldPositionStays = false, Action<GameObject> action = null)
        {
            return Instantiate(prefab, Vector3.zero, Quaternion.identity, parent, worldPositionStays, action);
        }

        private GameObject Instantiate(GameObject prefab, Vector3 position, Quaternion rotation, Transform parent, bool worldPositionStays, Action<GameObject> action)
        {
            var wasActive = prefab.activeSelf;
            if (wasActive)
            {
                prefab.SetActive(false);
            }
            var go = worldPositionStays ? GameObject.Instantiate(prefab, parent, true) : GameObject.Instantiate(prefab, position, rotation, parent);
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
