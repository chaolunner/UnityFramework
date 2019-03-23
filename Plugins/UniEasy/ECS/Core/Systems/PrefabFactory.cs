using UnityEngine;
using UniEasy.DI;

namespace UniEasy.ECS
{
    public class PrefabFactory
    {
        [Inject]
        protected DiContainer Container = null;

        public GameObject Instantiate(GameObject prefab, Transform parent = null, bool worldPositionStays = false)
        {
            var wasActive = prefab.activeSelf;
            if (wasActive)
            {
                prefab.SetActive(false);
            }
            var go = Object.Instantiate(prefab, parent, worldPositionStays);
            go.name = prefab.name;
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
