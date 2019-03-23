using UnityEngine;
using UnityEditor;
using System.Linq;

namespace UniEasy.Editor
{
    public class HierarchyContextMenu : EasyContextMenu
    {
        private static string GameObjectStr = "GameObject/";
        private static string ForwardSlashStr = "/";
        private static string EmptyStr = "";

        [InitializeOnLoadMethod]
        static void StaticInitialize()
        {
            EditorApplication.hierarchyWindowItemOnGUI += OnHierarchyWindowGUI;
        }

        static void OnHierarchyWindowGUI(int instanceID, Rect selectionRect)
        {
            if (Event.current != null && Event.current.button == 1 && Event.current.type == EventType.MouseUp && selectionRect.Contains(Event.current.mousePosition))
            {
                var menu = new GenericMenu();

                if (EditorSceneManagerHelper.GetSceneByHandle(instanceID).IsValid())
                {
                    SceneHierarchyWindowHelper.CreateMultiSceneHeaderContextClick(menu, instanceID);

                    menu.ShowAsContext();
                }
                else
                {
                    var items = ContextMenuMethods.OrderBy(m => m.Key.priority).GetEnumerator();
                    var separator = 50;
                    while (items.MoveNext())
                    {
                        var attribute = items.Current.Key;
                        var function = items.Current.Value;
                        if (attribute.menuItem.Contains(GameObjectStr))
                        {
                            var itemName = attribute.menuItem.Replace(GameObjectStr, EmptyStr);
                            if (attribute.priority > separator)
                            {
                                menu.AddSeparator(itemName.Substring(0, itemName.LastIndexOf(ForwardSlashStr) + 1));
                                separator += 50;
                            }
                            var result = true;
                            if (attribute.validate)
                            {
                                result = (bool)function.Invoke(null, null);
                                function = ContextMenuMethods.Where(item => !item.Key.validate && item.Key.menuItem == attribute.menuItem).FirstOrDefault().Value;
                            }

                            var param = function.GetParameters();
                            if (param.Length > 1)
                            {
                                result = false;
                            }
                            else if (param.Length == 1)
                            {
                                result = false;
                                if (param[0].ParameterType == typeof(object) && Selection.activeGameObject != null)
                                {
                                    menu.AddItem(new GUIContent(itemName), false, (go) =>
                                    {
                                        function.Invoke(null, new object[] { go });
                                    }, Selection.activeGameObject);

                                    result = true;
                                    continue;
                                }
                            }

                            if (result)
                            {
                                menu.AddItem(new GUIContent(itemName), false, () =>
                                {
                                    function.Invoke(null, null);
                                });
                            }
                            else
                            {
                                menu.AddDisabledItem(new GUIContent(itemName));
                            }
                        }
                    }

                    menu.AddSeparator(EmptyStr);

                    // CreateGameObjectContextClick method already did the menu.ShowAsContext();
                    SceneHierarchyWindowHelper.CreateGameObjectContextClick(menu, instanceID);
                }
            }
        }
    }
}
