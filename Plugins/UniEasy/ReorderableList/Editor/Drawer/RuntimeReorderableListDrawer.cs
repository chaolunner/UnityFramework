using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.Linq;

namespace UniEasy.Editor
{
    [RuntimeCustomPropertyDrawer(typeof(ReorderableAttribute))]
    public class RuntimeReorderableListDrawer : RuntimePropertyDrawer
    {
        #region Fields

        private bool isInitialized;

        private readonly List<RuntimeReorderableListData> listDataDict = new List<RuntimeReorderableListData>();
        private readonly Dictionary<string, UnityEditor.Editor> editableDict = new Dictionary<string, UnityEditor.Editor>();
        private readonly Dictionary<RuntimeReorderableList, System.Type> reorderableTypeDict = new Dictionary<RuntimeReorderableList, System.Type>();

        private static Dictionary<System.Type, Dictionary<ContextMenuAttribute, System.Type>> dropdownTypeDict = new Dictionary<System.Type, Dictionary<ContextMenuAttribute, System.Type>>();
        private readonly System.Type scriptableObjectType = typeof(ScriptableObject);
        private const string HeaderStr = "{0} [{1}] [InstanceID : {2}]";
        private const string ElementNameStr = "{0} {1}";

        #endregion

        #region Methods

        private void Initialize(RuntimeSerializedProperty property, bool force)
        {
            if (force)
            {
                isInitialized = false;
            }

            Initialize(property);
        }

        private void Initialize(RuntimeSerializedProperty property)
        {
            if (isInitialized)
            {
                return;
            }

            FindTargetProperties(property.Copy());
        }

        private void FindTargetProperties(RuntimeSerializedProperty property)
        {
            listDataDict.Clear();
            editableDict.Clear();

            var depth = property.Depth;
            do
            {
                if (property.IsArray && property.PropertyType != RuntimeSerializedPropertyType.String)
                {
                    var canTurnToList = property.HasAttribute<ReorderableAttribute>();
                    if (canTurnToList)
                    {
                        CreateListData(property.RuntimeSerializedObject.FindProperty(property.PropertyPath));
                    }
                }

                if (property.PropertyType.IsSubclassOf(RuntimeSerializedPropertyType.Object))
                {
                    var propType = property.GetTypeReflection();
                    if (propType == null)
                    {
                        continue;
                    }
                    var isScriptable = propType.IsSubclassOf(scriptableObjectType);
                    if (isScriptable)
                    {
                        var makeEditable = property.HasAttribute<ReorderableAttribute>();
                        if (makeEditable)
                        {
                            UnityEditor.Editor scriptableEditor = null;
                            if (property.ObjectReference != null)
                            {
                                UnityEditor.Editor.CreateCachedEditorWithContext(property.ObjectReference,
                                    property.RuntimeSerializedObject.TargetObject, null,
                                    ref scriptableEditor);
                            }
                            editableDict.Add(property.PropertyPath, scriptableEditor);
                        }
                    }
                }
            } while (property.Depth < depth && property.NextVisible(true));

            isInitialized = true;
        }

        private void CreateListData(RuntimeSerializedProperty property)
        {
            var root = property.GetRootPath();
            // Try to find the grand parent in RuntimeReorderableListData
            var data = listDataDict.Find(listData => listData.Parent.Equals(root));
            if (data == null)
            {
                data = new RuntimeReorderableListData(root);
                listDataDict.Add(data);
            }

            data.AddProperty(property);
            data.EditableCallback = () =>
            {
                if (Application.isPlaying && property.HasAttribute<RuntimeObjectAttribute>())
                {
                    return false;
                }
                return true;
            };

            if (property.HasAttribute<ReorderableAttribute>())
            {
                var reorderableAttr = property.GetAttributes<ReorderableAttribute>()[0] as ReorderableAttribute;
                if (reorderableAttr != null)
                {
                    HandleReorderableOptions(reorderableAttr, property, data);
                }
            }

            if (property.HasAttribute<BackgroundColorAttribute>())
            {
                var bgColorAttr = property.GetAttributes<BackgroundColorAttribute>()[0] as BackgroundColorAttribute;
                if (bgColorAttr != null)
                {
                    HandleBackgroundColorOptions(bgColorAttr, property, data);
                }
            }

            if (property.HasAttribute<DropdownMenuAttribute>())
            {
                var dropDownAttr = property.GetAttributes<DropdownMenuAttribute>()[0] as DropdownMenuAttribute;
                var reorderableList = data.GetPropertyList(property);

                reorderableTypeDict.Add(reorderableList, dropDownAttr.Type);
                reorderableList.onAddDropdownCallback += OnAddDropdownHandler;
                reorderableList.onRemoveCallback += OnRemoveHandler;
            }

            if (property.HasAttribute<PropertyAttribute>())
            {
                foreach (var attr in property.GetAttributes<PropertyAttribute>())
                {
                    if (attr is ReorderableAttribute || attr is BackgroundColorAttribute || attr is DropdownMenuAttribute)
                    {
                    }
                    else
                    {
                        data.ElementAttributes.Add(attr as PropertyAttribute);
                    }
                }
            }
        }

        private void HandleReorderableOptions(ReorderableAttribute reorderableAttr, RuntimeSerializedProperty property, RuntimeReorderableListData data)
        {
            data.HeaderCallback = rect =>
            {
                return DoHeader(property, rect, reorderableAttr.DisplayName);
            };

            if (!string.IsNullOrEmpty(reorderableAttr.ElementName))
            {
                data.ElementNameCallback = i => string.Format(ElementNameStr, reorderableAttr.ElementName, i);
            }
        }

        private void HandleBackgroundColorOptions(BackgroundColorAttribute bgColorAttr, RuntimeSerializedProperty property, RuntimeReorderableListData data)
        {
            data.DrawBackgroundCallback = (active, focused) =>
            {
                if (focused)
                {
                    return 1.35f * bgColorAttr.Color;
                }
                else if (active)
                {
                    return Color.Lerp(Color.white, bgColorAttr.Color, 0.75f);
                }
                else
                {
                    return bgColorAttr.Color;
                }
            };
        }

        public override void OnGUI(Rect position, RuntimeSerializedProperty property, GUIContent label)
        {
            Initialize(property);

            EditorGUI.BeginChangeCheck();

            DrawPropertySortableArray(position, property, label);

            if (EditorGUI.EndChangeCheck())
            {
                property.RuntimeSerializedObject.ApplyModifiedProperties();
                Initialize(property, true);
            }
        }

        public override float GetPropertyHeight(RuntimeSerializedProperty property, GUIContent label)
        {
            // Try to get the sortable list this property belongs to
            RuntimeReorderableListData listData = null;
            if (listDataDict.Count > 0)
            {
                listData = listDataDict.Find(data => property.PropertyPath.StartsWith(data.Parent));
            }

            return listData != null ? listData.GetPropertyHeight(property) : RuntimeEasyGUI.GetPropertyHeight(property, label, property.IsExpanded, null);
        }

        private void DrawPropertySortableArray(Rect position, RuntimeSerializedProperty property, GUIContent label)
        {
            // Try to get the sortable list this property belongs to
            RuntimeReorderableListData listData = null;
            if (listDataDict.Count > 0)
            {
                listData = listDataDict.Find(data => property.PropertyPath.StartsWith(data.Parent));
            }

            UnityEditor.Editor scriptableEditor;
            bool isScriptableEditor = editableDict.TryGetValue(property.PropertyPath, out scriptableEditor);

            // Has ReorderableList and Try to show the list
            if (listData != null && listData.DoProperty(position, property))
            {
            }
            // Else try to draw ScriptableObject editor
            else if (isScriptableEditor)
            {
                bool hasHeader = property.HasAttribute<HeaderAttribute>();
                bool hasSpace = property.HasAttribute<SpaceAttribute>();

                hasSpace |= hasHeader;

                // Reference type is not supported!
            }
            else
            {
                RuntimeEasyGUI.PropertyField(position, property, label, property.IsExpanded, null);
            }
        }

        private string DoHeader(RuntimeSerializedProperty property, Rect position, string displayName)
        {
            displayName = string.IsNullOrEmpty(displayName) ? property.DisplayName : displayName;
            string headerName = string.Format(HeaderStr, displayName, property.ArraySize, property.HashCodeForPropertyPath());
            RuntimeEasyGUI.PropertyField(position, property, new GUIContent(headerName), false, null);
            return headerName;
        }

        private void OnAddDropdownHandler(Rect position, RuntimeReorderableList list)
        {
            System.Type type = null;
            if (reorderableTypeDict.TryGetValue(list, out type))
            {
                OnAddDropdown(position, list, type);
            }
        }

        private void OnRemoveHandler(RuntimeReorderableList list)
        {
            System.Type type = null;
            if (reorderableTypeDict.TryGetValue(list, out type))
            {
                OnRemove(list, type);
            }
        }

        protected virtual void OnAddDropdown(Rect position, RuntimeReorderableList list, System.Type type)
        {
            GenericMenu popupMenu = new GenericMenu();
            Dictionary<ContextMenuAttribute, System.Type> dropdownTypes = null;

            if (!dropdownTypeDict.TryGetValue(type, out dropdownTypes))
            {
                dropdownTypes = new Dictionary<ContextMenuAttribute, System.Type>();

                foreach (var t in EasyEditor.Types.Where(t => type.IsAssignableFrom(t)))
                {
                    if (t.HasAttribute<ContextMenuAttribute>())
                    {
                        dropdownTypes.Add(t.AllAttributes<ContextMenuAttribute>().FirstOrDefault(), t);
                    }
                    else
                    {
                        dropdownTypes.Add(new ContextMenuAttribute(t.Name, false, int.MaxValue), t);
                    }
                }

                dropdownTypes = dropdownTypes.OrderBy(i => i.Key.Priority).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
            }

            if (dropdownTypes.Count > 0)
            {
                foreach (var kvp in dropdownTypes)
                {
                    var content = new GUIContent(kvp.Key.MenuItem);
                    var index = list.RuntimeSerializedProperty.ArraySize;
                    if (kvp.Value == typeof(GameObject) || kvp.Value.IsSubclassOf(typeof(Component)))
                    {
                        popupMenu.AddDisabledItem(content);
                    }
                    else if (kvp.Value.IsSubclassOf(typeof(ScriptableObject)))
                    {
                        // Reference type is not supported!
                    }
                    else
                    {
                        popupMenu.AddItem(content, false, () =>
                        {
                            var obj = System.Activator.CreateInstance(kvp.Value);
                            list.RuntimeSerializedProperty.ArraySize++;
                            list.index = index;
                            list.RuntimeSerializedProperty.GetArrayElementAtIndex(index).StringValue = RuntimeObject.ToJson(kvp.Value.ToString(), JsonUtility.ToJson(obj));
                            list.RuntimeSerializedProperty.RuntimeSerializedObject.ApplyModifiedProperties();
                        });
                    }
                }
            }
            popupMenu.ShowAsContext();
        }

        protected virtual void OnRemove(RuntimeReorderableList list, System.Type type)
        {
            if (typeof(ScriptableObject).IsAssignableFrom(type))
            {
                // Reference type is not supported!
            }
            RuntimeReorderableList.defaultBehaviours.DoRemoveButton(list);
        }

        #endregion
    }
}
