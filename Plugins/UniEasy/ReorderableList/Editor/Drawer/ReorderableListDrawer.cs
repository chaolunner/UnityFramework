using System.Collections.Generic;
using UnityEditorInternal;
using UnityEditor;
using UnityEngine;
using System.Linq;

namespace UniEasy.Editor
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(Object), true, isFallback = true)]
    public class ReorderableListDrawer : EasyContextMenu
    {
        protected enum IterateControl
        {
            Draw,
            Continue,
            Break
        }

        #region Fields

        public bool IsSubEditor;
        public bool IgnoreHeader = false;

        private readonly GUIContent Create = new GUIContent("Create");
        private readonly List<ReorderableListData> listDataDict = new List<ReorderableListData>();
        private readonly Dictionary<string, UnityEditor.Editor> editableDict = new Dictionary<string, UnityEditor.Editor>();
        private readonly Dictionary<ReorderableList, System.Type> reorderableTypeDict = new Dictionary<ReorderableList, System.Type>();

        private static Dictionary<System.Type, Dictionary<ContextMenuAttribute, System.Type>> dropdownTypeDict = new Dictionary<System.Type, Dictionary<ContextMenuAttribute, System.Type>>();
        private readonly System.Type scriptableObjectType = typeof(ScriptableObject);
        private const string ElementNameStr = "{0} {1}";
        private const string M_ScriptStr = "m_Script";
        private const string HeaderStr = "{0} [{1}] [InstanceID : {2}]";
        private const string RevertValueToPrefabStr = "Revert Value to Prefab";
        private const string PasteAllComponentsStr = "Paste All Components";
        private const string CopyAllComponentsStr = "Copy All Components";
        private const string EmptyStr = "";

        #endregion

        #region Methods

        protected override void Initialize()
        {
            base.Initialize();

            if (target != null)
            {
                FindTargetProperties();
            }
        }

        protected void FindTargetProperties()
        {
            listDataDict.Clear();
            editableDict.Clear();

            var iterProp = serializedObject.GetIterator();
            // This iterator goes through all the child serialized properties, looking
            // for properties that have the reorderable attribute
            if (iterProp.NextVisible(true))
            {
                do
                {
                    if (iterProp.isArray && iterProp.propertyType != SerializedPropertyType.String)
                    {
                        var canTurnToList = iterProp.HasAttribute<ReorderableAttribute>();
                        if (canTurnToList)
                        {
                            CreateListData(serializedObject.FindProperty(iterProp.propertyPath));
                        }
                    }

                    if (iterProp.propertyType == SerializedPropertyType.ObjectReference)
                    {
                        var propType = iterProp.GetTypeReflection();
                        if (propType == null)
                        {
                            continue;
                        }
                        var isScriptable = propType.IsSubclassOf(scriptableObjectType);
                        if (isScriptable)
                        {
                            var makeEditable = iterProp.HasAttribute<ReorderableAttribute>();
                            if (makeEditable)
                            {
                                UnityEditor.Editor scriptableEditor = null;
                                if (iterProp.objectReferenceValue != null)
                                {
                                    CreateCachedEditorWithContext(iterProp.objectReferenceValue,
                                        serializedObject.targetObject, null,
                                        ref scriptableEditor);
                                    var reorderableList = scriptableEditor as ReorderableListDrawer;
                                    if (reorderableList != null)
                                    {
                                        reorderableList.IsSubEditor = true;
                                    }
                                }
                                editableDict.Add(iterProp.propertyPath, scriptableEditor);
                            }
                        }
                    }
                } while (iterProp.NextVisible(true));
            }

            isInitialized = true;
        }

        private void CreateListData(SerializedProperty property)
        {
            var root = property.GetRootPath();
            // Try to find the grand parent in ReorderableListData
            var data = listDataDict.Find(listData => listData.Parent.Equals(root));
            if (data == null)
            {
                data = new ReorderableListData(root);
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

        private void HandleReorderableOptions(ReorderableAttribute reorderableAttr, SerializedProperty property, ReorderableListData data)
        {
            data.HeaderCallback = rect =>
            {
                return DoHeader(property, rect, reorderableAttr.DisplayName);
            };

            data.HeaderMenuCallback = (genericMenu) =>
            {
                return DoHeaderMenu(property, genericMenu);
            };

            if (!string.IsNullOrEmpty(reorderableAttr.ElementName))
            {
                data.ElementNameCallback = i => string.Format(ElementNameStr, reorderableAttr.ElementName, i);
            }
        }

        private void HandleBackgroundColorOptions(BackgroundColorAttribute bgColorAttr, SerializedProperty property, ReorderableListData data)
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

        public override void OnInspectorGUI()
        {
            DrawInspector();

            base.OnInspectorGUI();
        }

        protected ReorderableListData GetReorderableListData(SerializedProperty property)
        {
            if (listDataDict.Count == 0)
            {
                return null;
            }
            return listDataDict.Find(listData => listData.Parent.Equals(property.GetRootPath()));
        }

        protected ReorderableList GetReorderableList(SerializedProperty property)
        {
            var data = GetReorderableListData(property);
            if (data == null)
            {
                return null;
            }

            return data.GetPropertyList(property);
        }

        protected bool SetDragDropHandler(SerializedProperty property, System.Action<SerializedProperty, Object[]> handler)
        {
            var data = GetReorderableListData(property);
            if (data == null)
            {
                return false;
            }

            data.SetDropHandler(property, handler);
            return true;
        }

        protected virtual void DrawInspector()
        {
            var position = GUILayoutUtility.GetRect(0, 0);
            DrawPropertiesAll(position);
            GUILayoutUtility.GetRect(position.width, GetPropertiesAllHeights());
        }

        public void DrawPropertiesAll(Rect position)
        {
            EditorGUI.BeginChangeCheck();

            var iterProp = serializedObject.GetIterator();
            IterateDrawProperty(position, iterProp);

            if (EditorGUI.EndChangeCheck())
            {
                serializedObject.ApplyModifiedProperties();
                Initialize(true);
            }
        }

        public float GetPropertiesAllHeights()
        {
            var iterProp = serializedObject.GetIterator();
            return IterateGetPropertyHeight(iterProp);
        }

        protected void IterateDrawProperty(Rect position, SerializedProperty property, System.Func<IterateControl> filter = null)
        {
            if (property.NextVisible(true))
            {
                // Remember depth iteration started from
                int depth = property.depth;
                do
                {
                    // If goes deeper than the iteration depth, get out
                    if (property.depth != depth)
                    {
                        break;
                    }
                    if (IsSubEditor && property.name.Equals(M_ScriptStr))
                    {
                        continue;
                    }

                    if (filter != null)
                    {
                        var filterResult = filter();
                        if (filterResult == IterateControl.Break)
                        {
                            break;
                        }
                        if (filterResult == IterateControl.Continue)
                        {
                            continue;
                        }
                    }

                    position.height = GetPropertyHeight(property);
                    DrawProperty(position, property);
                    position.yMin = position.yMax;
                } while (property.NextVisible(false));
            }
        }

        protected float IterateGetPropertyHeight(SerializedProperty property, System.Func<IterateControl> filter = null)
        {
            var height = 0f;
            if (property.NextVisible(true))
            {
                int depth = property.depth;
                do
                {
                    if (property.depth != depth)
                    {
                        break;
                    }
                    if (IsSubEditor && property.name.Equals(M_ScriptStr))
                    {
                        continue;
                    }
                    if (filter != null)
                    {
                        var filterResult = filter();
                        if (filterResult == IterateControl.Break)
                        {
                            break;
                        }
                        if (filterResult == IterateControl.Continue)
                        {
                            continue;
                        }
                    }
                    height += GetPropertyHeight(property);
                } while (property.NextVisible(false));
            }
            return height;
        }

        protected void DrawProperty(Rect position, SerializedProperty property)
        {
            // Try to get the sortable list this property belongs to
            var listData = GetReorderableListData(property);
            UnityEditor.Editor scriptableEditor;
            var isScriptableEditor = editableDict.TryGetValue(property.propertyPath, out scriptableEditor);

            // Has ReorderableList
            if (listData != null)
            {
                // Try to show the list
                if (!listData.DoProperty(position, property))
                {
                    var headerPosition = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
                    EditorGUI.PropertyField(headerPosition, property, false);
                    position.y += EditorGUIUtility.singleLineHeight;
                    if (property.isExpanded)
                    {
                        EditorGUI.indentLevel++;
                        IterateDrawProperty(position, property.Copy());
                        EditorGUI.indentLevel--;
                    }
                }
            }
            // Else try to draw ScriptableObject editor
            else if (isScriptableEditor)
            {
                bool hasHeader = property.HasAttribute<HeaderAttribute>();
                bool hasSpace = property.HasAttribute<SpaceAttribute>();

                hasSpace |= hasHeader;

                // No data in property, draw property field with create button
                if (scriptableEditor == null)
                {
                    var objPosition = new Rect(position);
                    var btnPosition = new Rect(position);

                    objPosition.xMax -= hasSpace ? 66 : 56;
                    btnPosition.xMin = btnPosition.xMax - 56;
                    EditorGUI.PropertyField(objPosition, property, false);
                    var doCreate = GUI.Button(btnPosition, Create, EditorStyles.miniButton);

                    if (doCreate)
                    {
                        var propType = property.GetTypeReflection();
                        ScriptableObjectUtility.CreateAssetWithSavePrompt(propType, postCreated: createdAsset =>
                        {
                            property.objectReferenceValue = createdAsset;
                            property.isExpanded = true;
                            return false;
                        });
                    }
                }
                // Has data in property, draw foldout and editor
                else
                {
                    EasyGUI.PropertyField(position, property, new GUIContent(property.displayName), true, null);
                }
            }
            else
            {
                var isStartProp = property.propertyPath.StartsWith(M_ScriptStr);
                if (isStartProp && IgnoreHeader)
                {
                }
                else
                {
                    using (new EditorGUI.DisabledScope(isStartProp))
                    {
                        EditorGUI.PropertyField(position, property, property.isExpanded);
                    }
                }
            }
        }

        protected float GetPropertyHeight(SerializedProperty property)
        {
            var height = 0f;
            var listData = GetReorderableListData(property);
            UnityEditor.Editor scriptableEditor;
            var isScriptableEditor = editableDict.TryGetValue(property.propertyPath, out scriptableEditor);

            if (listData != null)
            {
                height += listData.GetPropertyHeight(property);
                if (height == 0f)
                {
                    height += EditorGUI.GetPropertyHeight(property, false);
                    if (property.isExpanded)
                    {
                        height += IterateGetPropertyHeight(property.Copy());
                    }
                }
            }
            else if (isScriptableEditor)
            {
                if (scriptableEditor == null)
                {
                    height += EditorGUI.GetPropertyHeight(property, false);
                }
                else
                {
                    height += EasyGUI.GetPropertyHeight(property, null, null, true);
                }
            }
            else
            {
                var isStartProp = property.propertyPath.StartsWith(M_ScriptStr);
                if (isStartProp && IgnoreHeader)
                {
                }
                else
                {
                    height += EditorGUI.GetPropertyHeight(property, property.isExpanded);
                }
            }
            return height;
        }

        protected virtual string DoHeader(SerializedProperty property, Rect position, string displayName)
        {
            displayName = string.IsNullOrEmpty(displayName) ? property.displayName : displayName;
            string headerName = string.Format(HeaderStr, displayName, property.arraySize, property.serializedObject.GetHashCode());
            EditorGUI.PropertyField(position, property, new GUIContent(headerName), false);
            return headerName;
        }

        protected virtual bool DoHeaderMenu(SerializedProperty property, GenericMenu genericMenu)
        {
            genericMenu.AddItem(EditorGUIUtility.TrTextContent(RevertValueToPrefabStr), false, (object o) =>
            {
                TargetChoiceHandler.RevertPrefabPropertyOverride(o);
            }, property);
            if (genericMenu.GetItemCount() > 0)
            {
                genericMenu.AddSeparator(EmptyStr);
            }
            genericMenu.AddItem(EditorGUIUtility.TrTextContent(CopyAllComponentsStr), false, (object o) =>
            {
                TargetChoiceHandler.CopyAllComponents(o);
            }, property);
            if (TargetChoiceHandler.CanPaste(property))
            {
                genericMenu.AddItem(EditorGUIUtility.TrTextContent(PasteAllComponentsStr), false, (object o) =>
                {
                    TargetChoiceHandler.PasteAllComponents(o);
                }, property);
            }
            else
            {
                genericMenu.AddDisabledItem(EditorGUIUtility.TrTextContent(PasteAllComponentsStr));
            }
            return true;
        }

        private void OnAddDropdownHandler(Rect position, ReorderableList list)
        {
            System.Type type = null;
            if (reorderableTypeDict.TryGetValue(list, out type))
            {
                OnAddDropdown(position, list, type);
            }
        }

        private void OnRemoveHandler(ReorderableList list)
        {
            System.Type type = null;
            if (reorderableTypeDict.TryGetValue(list, out type))
            {
                OnRemove(list, type);
            }
        }

        protected virtual void OnAddDropdown(Rect position, ReorderableList list, System.Type type)
        {
            GenericMenu popupMenu = new GenericMenu();
            Dictionary<ContextMenuAttribute, System.Type> dropdownTypes = null;

            if (!dropdownTypeDict.TryGetValue(type, out dropdownTypes))
            {
                dropdownTypes = new Dictionary<ContextMenuAttribute, System.Type>();

                foreach (var t in Types.Where(t => type.IsAssignableFrom(t)))
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
                    var index = list.serializedProperty.arraySize;
                    if (kvp.Value == typeof(GameObject) || kvp.Value.IsSubclassOf(typeof(Component)))
                    {
                        popupMenu.AddDisabledItem(content);
                    }
                    else if (kvp.Value.IsSubclassOf(typeof(ScriptableObject)))
                    {
                        popupMenu.AddItem(content, false, () =>
                        {
                            var scriptableObject = CreateInstance(kvp.Value);

                            Object assetObject = null;
                            if (target is ScriptableObject)
                            {
                                assetObject = target;
                            }
                            else
                            {
#if UNITY_2018_3_OR_NEWER
                                assetObject = PrefabUtility.GetCorrespondingObjectFromSource(target) ?? PrefabUtility.GetPrefabInstanceHandle(target);
#elif UNITY_2018_2
                                assetObject = PrefabUtility.GetCorrespondingObjectFromSource(target) ?? PrefabUtility.GetPrefabObject(target);
#else
                                assetObject = PrefabUtility.GetPrefabParent(target) ?? PrefabUtility.GetPrefabObject(target);
#endif
                            }
                            if (assetObject != null)
                            {
                                scriptableObject.name = kvp.Value.Name;
                                AssetDatabase.AddObjectToAsset(scriptableObject, assetObject);
                                list.serializedProperty.arraySize++;
                                list.index = index;
                                list.serializedProperty.GetArrayElementAtIndex(index).objectReferenceValue = scriptableObject;
                            }
                            else
                            {
#if UNITY_EDITOR
                                Debug.LogWarning("You can't add " + scriptableObject + " to a scene object!", target);
#endif
                            }
                            serializedObject.ApplyModifiedProperties();
                        });
                    }
                    else
                    {
                        popupMenu.AddItem(content, false, () =>
                        {
                            var obj = System.Activator.CreateInstance(kvp.Value);
                            list.serializedProperty.arraySize++;
                            list.index = index;
                            list.serializedProperty.GetArrayElementAtIndex(index).stringValue = RuntimeObject.ToJson(kvp.Value.ToString(), JsonUtility.ToJson(obj));
                            serializedObject.ApplyModifiedProperties();
                        });
                    }
                }
            }
            popupMenu.ShowAsContext();
        }

        protected virtual void OnRemove(ReorderableList list, System.Type type)
        {
            if (typeof(ScriptableObject).IsAssignableFrom(type))
            {
                var action = list.serializedProperty.GetArrayElementAtIndex(list.index).objectReferenceValue;
                if (action != null)
                {
                    DestroyImmediate(action, true);
                }
            }
            ReorderableList.defaultBehaviours.DoRemoveButton(list);
        }

        #endregion
    }
}
