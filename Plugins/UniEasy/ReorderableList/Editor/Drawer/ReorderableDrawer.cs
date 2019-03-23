using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace UniEasy.Editor
{
    [CustomPropertyDrawer(typeof(ReorderableAttribute))]
    public class ReorderableDrawer : PropertyDrawer
    {
        #region Fields

        private bool isInitialized;

        private readonly GUIContent Create = new GUIContent("Create");

        private readonly List<ReorderableListData> listIndex = new List<ReorderableListData>();
        private readonly Dictionary<string, UnityEditor.Editor> editableIndex = new Dictionary<string, UnityEditor.Editor>();

        private static System.Type scriptableObjectType = typeof(ScriptableObject);
        private static string HeaderStr = "{0} [{1}]";
        private static string ElementNameStr = "{0} {1}";

        #endregion

        #region Methods

        private void Initialize(SerializedProperty property, bool force)
        {
            if (force)
            {
                isInitialized = false;
            }

            Initialize(property);
        }

        private void Initialize(SerializedProperty property)
        {
            if (isInitialized)
            {
                return;
            }

            FindTargetProperties(property.Copy());
        }

        private void FindTargetProperties(SerializedProperty property)
        {
            listIndex.Clear();
            editableIndex.Clear();

            var depth = property.depth;
            do
            {
                if (property.isArray && property.propertyType != SerializedPropertyType.String)
                {
                    var canTurnToList = property.HasAttribute<ReorderableAttribute>();
                    if (canTurnToList)
                    {
                        CreateListData(property.serializedObject.FindProperty(property.propertyPath));
                    }
                }

                if (property.propertyType == SerializedPropertyType.ObjectReference)
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
                            if (property.objectReferenceValue != null)
                            {
                                UnityEditor.Editor.CreateCachedEditorWithContext(property.objectReferenceValue,
                                    property.serializedObject.targetObject, null,
                                    ref scriptableEditor);
                            }
                            editableIndex.Add(property.propertyPath, scriptableEditor);
                        }
                    }
                }
            } while (property.depth < depth && property.NextVisible(true));

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
        }

        private void HandleReorderableOptions(ReorderableAttribute reorderableAttr, SerializedProperty property, ReorderableListData data)
        {
            data.HeaderCallback = rect =>
            {
                return DoHeader(property, rect, reorderableAttr.DisplayName);
            };

            if (!string.IsNullOrEmpty(reorderableAttr.ElementName))
            {
                data.ElementNameCallback = i => string.Format(ElementNameStr, reorderableAttr.ElementName, i);
            }
            else if (reorderableAttr.ElementName == null)
            {
                data.ElementNameCallback = i => null;
            }

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

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            Initialize(property);

            EditorGUI.BeginChangeCheck();

            DrawPropertySortableArray(position, property, label);

            if (EditorGUI.EndChangeCheck())
            {
                property.serializedObject.ApplyModifiedProperties();
                Initialize(property, true);
            }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            // Try to get the sortable list this property belongs to
            ReorderableListData listData = null;
            if (listIndex.Count > 0)
            {
                listData = listIndex.Find(data => property.propertyPath.StartsWith(data.Parent));
            }

            return listData != null ? listData.GetPropertyHeight(property) : EasyGUI.GetPropertyFieldHeight(property, label, property.isExpanded);
        }

        private void DrawPropertySortableArray(Rect position, SerializedProperty property, GUIContent label)
        {
            // Try to get the sortable list this property belongs to
            ReorderableListData listData = null;
            if (listIndex.Count > 0)
            {
                listData = listIndex.Find(data => property.propertyPath.StartsWith(data.Parent));
            }

            UnityEditor.Editor scriptableEditor;
            bool isScriptableEditor = editableIndex.TryGetValue(property.propertyPath, out scriptableEditor);

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

                // No data in property, draw property field with create button
                if (scriptableEditor == null)
                {
                    var fieldPosition = new Rect(position);
                    var buttonPosition = new Rect(position);
                    fieldPosition.width -= hasSpace ? 60 : 50;
                    buttonPosition.xMin = fieldPosition.xMax;

                    EditorGUI.PropertyField(fieldPosition, property);
                    bool doCreate = GUI.Button(buttonPosition, Create, EditorStyles.miniButton);

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
            }
            else
            {
                EditorGUI.PropertyField(position, property, label, property.isExpanded);
            }
        }

        private string DoHeader(SerializedProperty property, Rect position, string displayName)
        {
            displayName = string.IsNullOrEmpty(displayName) ? property.displayName : displayName;
            string headerName = string.Format(HeaderStr, displayName, property.arraySize);
            EditorGUI.PropertyField(position, property, new GUIContent(headerName), false);
            return headerName;
        }

        #endregion
    }
}
