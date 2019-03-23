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
        private readonly List<ReorderableListData> listIndex = new List<ReorderableListData>();
        private readonly Dictionary<string, UnityEditor.Editor> editableIndex = new Dictionary<string, UnityEditor.Editor>();
        private readonly Dictionary<ReorderableList, System.Type> reorderableTypeIndex = new Dictionary<ReorderableList, System.Type>();

        private static Dictionary<System.Type, Dictionary<ContextMenuAttribute, System.Type>> dropdownTypeIndex = new Dictionary<System.Type, Dictionary<ContextMenuAttribute, System.Type>>();
        private static System.Type scriptableObjectType = typeof(ScriptableObject);
        private static string RevertValueToPrefabStr = "Revert Value to Prefab";
        private static string PasteComponentAsNewStr = "Paste Component As New";
        private static string PasteAllComponentsStr = "Paste All Components";
        private static string CopyAllComponentsStr = "Copy All Components";
        private static string ElementNameStr = "{0} {1}";
        private static string M_ScriptStr = "m_Script";
        private static string HeaderStr = "{0} [{1}]";
        private static string EmptyStr = "";

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
            listIndex.Clear();
            editableIndex.Clear();

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
                                editableIndex.Add(iterProp.propertyPath, scriptableEditor);
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
            var data = listIndex.Find(listData => listData.Parent.Equals(root));
            if (data == null)
            {
                data = new ReorderableListData(root);
                listIndex.Add(data);
            }

            data.AddProperty(property);
            data.EditableCallback = () =>
            {
                if (!property.IsInspectableObjectDataArrayOrList())
                {
                    return !Application.isPlaying;
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

                reorderableTypeIndex.Add(reorderableList, dropDownAttr.Type);
                reorderableList.onAddDropdownCallback += OnAddDropdownHandler;
                reorderableList.onRemoveCallback += OnRemoveHandler;
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
            else if (reorderableAttr.ElementName == null)
            {
                data.ElementNameCallback = i => null;
            }

            data.ElementHeaderCallback = (position, label, inspectableObject) =>
            {
                return DoElementHeader(position, label, inspectableObject);
            };

            data.ElementFooterCallback = (position, inspectableObject) =>
            {
                DoElementFooter(position, inspectableObject);
            };

            data.IsDrawObjectReference = reorderableAttr.IsDrawObjectReference;
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
            if (listIndex.Count == 0)
            {
                return null;
            }
            return listIndex.Find(listData => listData.Parent.Equals(property.GetRootPath()));
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
            var isScriptableEditor = editableIndex.TryGetValue(property.propertyPath, out scriptableEditor);

            // Has ReorderableList
            if (listData != null)
            {
                // Try to show the list
                if (!listData.DoProperty(position, property))
                {
                    EditorGUI.PropertyField(position, property, false);
                    position.y += EditorGUI.GetPropertyHeight(property, false);
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
                    EasyGUI.TryDrawObjectReference(position, property, new GUIContent(property.displayName), true);
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
            var isScriptableEditor = editableIndex.TryGetValue(property.propertyPath, out scriptableEditor);

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
                    height += EasyGUI.GetObjectReferenceHeight(property, true);
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
            string headerName = string.Format(HeaderStr, displayName, property.arraySize);
            EditorGUI.PropertyField(position, property, new GUIContent(headerName), false);
            return headerName;
        }

        protected virtual bool DoHeaderMenu(SerializedProperty property, GenericMenu genericMenu)
        {
            genericMenu.AddItem(EditorGUIUtility.TrTextContent(RevertValueToPrefabStr), false, delegate (object o)
            {
                TargetChoiceHandler.SetPrefabOverride(o);
            }, property);
            if (genericMenu.GetItemCount() > 0)
            {
                genericMenu.AddSeparator(EmptyStr);
            }
            if (TargetChoiceHandler.CanPasteAsNew(property))
            {
                genericMenu.AddItem(EditorGUIUtility.TrTextContent(PasteComponentAsNewStr), false, delegate (object o)
                {
                    TargetChoiceHandler.PasteComponentAsNew(o);
                }, property);
            }
            else
            {
                genericMenu.AddDisabledItem(EditorGUIUtility.TrTextContent(PasteComponentAsNewStr));
            }
            genericMenu.AddItem(EditorGUIUtility.TrTextContent(CopyAllComponentsStr), false, delegate (object o)
            {
                TargetChoiceHandler.CopyAllComponents(o);
            }, property);
            if (TargetChoiceHandler.CanPaste(property))
            {
                genericMenu.AddItem(EditorGUIUtility.TrTextContent(PasteAllComponentsStr), false, delegate (object o)
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

        protected virtual bool DoElementHeader(Rect position, GUIContent label, InspectableObject inspectableObject)
        {
            return EasyGUI.TryDrawDefaultElementHeader(position, label, inspectableObject);
        }

        protected virtual void DoElementFooter(Rect position, InspectableObject inspectableObject)
        {
        }

        private void OnAddDropdownHandler(Rect position, ReorderableList list)
        {
            System.Type type = null;
            if (reorderableTypeIndex.TryGetValue(list, out type))
            {
                OnAddDropdown(position, list, type);
            }
        }

        private void OnRemoveHandler(ReorderableList list)
        {
            System.Type type = null;
            if (reorderableTypeIndex.TryGetValue(list, out type))
            {
                OnRemove(list, type);
            }
        }

        protected virtual void OnAddDropdown(Rect position, ReorderableList list, System.Type type)
        {
            GenericMenu popupMenu = new GenericMenu();
            Dictionary<ContextMenuAttribute, System.Type> dropdownTypes = null;

            if (!dropdownTypeIndex.TryGetValue(type, out dropdownTypes))
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
                    if (kvp.Value.IsSubclassOf(typeof(ScriptableObject)))
                    {
                        popupMenu.AddItem(content, false, () =>
                        {
                            var component = CreateInstance(kvp.Value);

                            Object assetObject = null;
                            if (target is ScriptableObject)
                            {
                                assetObject = target;
                            }
                            else
                            {
#if UNITY_2018_2_OR_NEWER
                                assetObject = PrefabUtility.GetCorrespondingObjectFromSource(target) ?? PrefabUtility.GetPrefabObject(target);
#else
                                assetObject = PrefabUtility.GetPrefabParent(target) ?? PrefabUtility.GetPrefabObject(target);
#endif
                            }
                            if (assetObject != null)
                            {
                                component.name = kvp.Value.Name;
                                AssetDatabase.AddObjectToAsset(component, assetObject);
                                list.serializedProperty.arraySize++;
                                list.index = index;
                                list.serializedProperty.GetArrayElementAtIndex(index).objectReferenceValue = component;
                            }
                            else
                            {
#if UNITY_EDITOR
                                Debug.LogWarning("The object you added scriptable components must be prefab or asset!");
#endif
                            }
                            serializedObject.ApplyModifiedProperties();
                        });
                    }
                    else
                    {
                        popupMenu.AddItem(content, false, () =>
                        {
                            var component = System.Activator.CreateInstance(kvp.Value);
                            list.serializedProperty.arraySize++;
                            list.index = index;
                            list.serializedProperty.GetArrayElementAtIndex(index).SetInspectableObjectData(new InspectableObjectData(kvp.Value.ToString(), JsonUtility.ToJson(component)));
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
