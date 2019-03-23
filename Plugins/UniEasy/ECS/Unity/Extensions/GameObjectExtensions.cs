using UnityEngine;

namespace UniEasy.ECS
{
    public static class GameObjectExtensions
    {
        public static void ForceActive(this GameObject gameObject)
        {
            if (gameObject.activeInHierarchy)
            {
                return;
            }
            var wasActive = gameObject.activeSelf;

            gameObject.SetActive(true);
            if (gameObject.activeInHierarchy)
            {
                return;
            }

            var parent = gameObject.transform.parent;
            var index = gameObject.transform.GetSiblingIndex();

            gameObject.transform.SetParent(null);
            gameObject.SetActive(wasActive);
            gameObject.transform.SetParent(parent);
            gameObject.transform.SetSiblingIndex(index);
        }
    }
}
