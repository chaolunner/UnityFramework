using CustomMenu = UnityEngine.ContextMenu;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEditor;
using System;

namespace UniEasy.Editor
{
    public class SearchSceneMissingContextMenu : UnityEditor.Editor
    {
        [CustomMenu("GameObject/UniEasy/Search for All Missing Components", false, 1)]
        public static void SearchForAllMissingComponents()
        {
            SceneHierarchyWindowHelper.Show();
            SceneHierarchyWindowHelper.SetRows(null);
            SceneHierarchyWindowHelper.ReloadData();
            SceneHierarchyWindowHelper.SearchFilter = "t: Component";
            SceneHierarchyWindowHelper.SearchChanged();
            SearchProcess(go =>
            {
                return HaveMissingComponent(go);
            });
            SceneHierarchyWindowHelper.Repaint();
        }

        static void SearchProcess(Func<GameObject, bool> e)
        {
            var list = new List<object>();
            var items = SceneHierarchyWindowHelper.GetRows().GetEnumerator();
            var itemType = items.GetType().GetGenericArguments()[0];
            var itemIdPropertyInfo = itemType.GetProperty("id", BindingFlags.Public | BindingFlags.Instance);
            var activeId = Selection.activeInstanceID;
            while (items.MoveNext())
            {
                var id = (int)itemIdPropertyInfo.GetValue(items.Current, null);
                Selection.activeInstanceID = id;
                if (e(Selection.activeGameObject))
                {
                    list.Add(items.Current);
                }
            }
            Selection.activeInstanceID = activeId;
            var itemList = Activator.CreateInstance(typeof(List<>).MakeGenericType(new Type[] {
                itemType,
            }));
            var addMethod = itemList.GetType().GetMethod("Add");
            for (int i = 0; i < list.Count; i++)
            {
                addMethod.Invoke(itemList, new object[] {
                    list [i],
                });
            }
            SceneHierarchyWindowHelper.SetRows(itemList);
            SceneHierarchyWindowHelper.SetRowCount(list.Count);
        }

        static bool HaveMissingComponent(GameObject go)
        {
            if (go != null)
            {
                var components = go.GetComponents<Component>();
                for (int i = 0; i < components.Length; i++)
                {
                    if (components[i] == null)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        static GameObject[] FindMissingComponents(GameObject[] gameObjects)
        {
            var result = new List<GameObject>();
            for (int i = 0; i < gameObjects.Length; i++)
            {
                if (HaveMissingComponent(gameObjects[i]))
                {
                    result.Add(gameObjects[i]);
                }
            }
            return result.ToArray();
        }

        [CustomMenu("GameObject/UniEasy/Clean Selected Missing Components", false, 2)]
        static void CleanSelectedMissingComponents()
        {
            CleanupMissingComponents<Component>(Selection.gameObjects);
        }

        [CustomMenu("GameObject/UniEasy/Clean Selected Missing Components", true, 2)]
        static bool CheckSelectedGameObjectsInSceneNotEmpty()
        {
            if (Selection.gameObjects.Length > 0)
            {
                return true;
            }
            return false;
        }

        static void CleanupMissingComponents<T>(GameObject[] gameObjects)
        {
            foreach (var go in gameObjects)
            {
                var components = go.GetComponents<T>();
                var serializedObject = new SerializedObject(go);
                var prop = serializedObject.FindProperty("m_Component");
                int r = 0;
                for (int i = 0; i < components.Length; i++)
                {
                    if (components[i] == null)
                    {
                        prop.DeleteArrayElementAtIndex(i - r);
                        r++;
                    }
                }
                serializedObject.ApplyModifiedProperties();
            }
        }
    }
}
