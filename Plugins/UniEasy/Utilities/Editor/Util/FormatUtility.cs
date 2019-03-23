using CustomMenu = UnityEngine.ContextMenu;
using System.Text.RegularExpressions;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEditor;
using UnityEngine;
using System.Linq;

namespace UniEasy.Editor
{
    public static class FormatUtility
    {
        private static string FormatPatternStr = "[0-9]*\\s*\\(*[0-9]*\\)*";
        private static string ZeroStr = "0";
        private static string EmptyStr = "";

        [MenuItem("GameObject/Format Selected GameObjects Name", false, 151)]
        [CustomMenu("GameObject/UniEasy/Format Selected GameObjects Name", false, 151)]
        private static void FormatSelectedGameObjectsName()
        {
            FormatGameObjectsName(Selection.gameObjects);
        }

        [MenuItem("GameObject/Format Selected GameObjects Name", true, 151)]
        [MenuItem("GameObject/Order Selected GameObjects", true, 153)]
        [CustomMenu("GameObject/UniEasy/Format Selected GameObjects Name", true, 151)]
        [CustomMenu("GameObject/UniEasy/Order Selected GameObjects", true, 153)]
        private static bool IsSceneObjectSelected()
        {
            return Selection.activeTransform != null;
        }

        public static GameObject[] FormatGameObjectsName(Transform parent)
        {
            var goes = new GameObject[parent.childCount];
            for (int i = 0; i < parent.childCount; i++)
            {
                goes[i] = parent.GetChild(i).gameObject;
            }
            FormatGameObjectsName(goes);
            return goes;
        }

        public static void FormatGameObjectsName(GameObject[] gameObjects)
        {
            var indexs = new Dictionary<string, List<GameObject>>();
            var goes = gameObjects.OrderBy(go => go.transform.GetSiblingIndex());

            Object assetObject = null;

            foreach (var go in goes)
            {
                if (PrefabUtility.GetPrefabType(go) == PrefabType.Prefab || PrefabUtility.GetPrefabType(go) == PrefabType.PrefabInstance)
                {
#if UNITY_2018_2_OR_NEWER
                    assetObject = PrefabUtility.GetCorrespondingObjectFromSource(go) ?? PrefabUtility.GetPrefabObject(go);
#else
                    assetObject = PrefabUtility.GetPrefabParent(go) ?? PrefabUtility.GetPrefabObject(go);
#endif
                    go.name = assetObject.name;
                }
                else
                {
                    go.name = Regex.Replace(go.name, FormatPatternStr, EmptyStr);
                }
            }
            foreach (var go in goes)
            {
                var name = go.name;
                if (indexs.ContainsKey(name))
                {
                    if (indexs[name].Count == 1)
                    {
                        indexs[name][0].name += ZeroStr;
                    }
                    go.name += indexs[name].Count;
                }
                else
                {
                    indexs.Add(name, new List<GameObject>());
                }
                indexs[name].Add(go);
                EditorUtility.SetDirty(go);
            }

            EditorSceneManager.MarkAllScenesDirty();
        }

        [MenuItem("GameObject/Order Selected GameObjects", false, 153)]
        [CustomMenu("GameObject/UniEasy/Order Selected GameObjects", false, 153)]
        public static void OrderSelectedGameObjects()
        {
            var goes = Selection.gameObjects.OrderBy(go => go.name);

            foreach (var go in goes)
            {
                go.transform.SetAsLastSibling();
                EditorUtility.SetDirty(go);
            }

            EditorSceneManager.MarkAllScenesDirty();
        }

        public static Dictionary<Material, List<GameObject>> GetOrderedMaterialsFromActivatedScenes()
        {
            var mats = new Dictionary<Material, List<GameObject>>();

            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                foreach (var root in SceneManager.GetSceneAt(i).GetRootGameObjects())
                {
                    foreach (var renderer in root.GetComponentsInChildren<Renderer>(true))
                    {
                        foreach (var material in renderer.sharedMaterials)
                        {
                            if (material == null)
                            {
                                continue;
                            }
                            if (!mats.ContainsKey(material))
                            {
                                mats.Add(material, new List<GameObject>());
                            }
                            if (!mats[material].Contains(renderer.gameObject))
                            {
                                mats[material].Add(renderer.gameObject);
                            }
                        }
                    }

                    foreach (var graphic in root.GetComponentsInChildren<Graphic>(true))
                    {
                        if (graphic.material == null)
                        {
                            continue;
                        }
                        if (!mats.ContainsKey(graphic.material))
                        {
                            mats.Add(graphic.material, new List<GameObject>());
                        }
                        if (!mats[graphic.material].Contains(graphic.gameObject))
                        {
                            mats[graphic.material].Add(graphic.gameObject);
                        }
                    }
                }
            }

            return mats.OrderBy(kvp => kvp.Key.renderQueue).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        }
    }
}
