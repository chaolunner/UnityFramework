using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace UniEasy.Editor
{
    public partial class ReorderableListData
    {
        #region Fields

        private readonly Dictionary<string, InspectableReorderableList> extendListIndex = new Dictionary<string, InspectableReorderableList>();

        #endregion

        #region Methods

        public void AddProperty(InspectableProperty property)
        {
            // Check if this property actually belongs to the same direct child
            if (!property.GetRootPath().Equals(Parent))
            {
                return;
            }

            if (extendListIndex.ContainsKey(property.PropertyPath))
            {
                return;
            }

            InspectableReorderableList propList = new InspectableReorderableList(
                                                 property.InspectableObject.SerializedObject, property,
                                                 draggable: true, displayHeader: false,
                                                 displayAddButton: true, displayRemoveButton: true)
            {
                headerHeight = 5
            };

            propList.drawElementBackgroundCallback = delegate (Rect position, int index, bool active, bool focused)
            {
                if (DrawBackgroundCallback != null)
                {
                    Rect backgroundRect = new Rect(position);
                    if (index <= 0)
                    {
                        backgroundRect.yMin -= 8;
                    }
                    if (index >= propList.count - 1)
                    {
                        backgroundRect.yMax += 3;
                    }
                    EditorGUI.DrawRect(backgroundRect, DrawBackgroundCallback(active, focused));
                }
                else
                {
                    propList.drawElementBackgroundCallback = null;
                }
            };

            propList.drawElementCallback = delegate (Rect position, int index, bool active, bool focused)
            {
                var iterProp = property.GetArrayElementAtIndex(index);
                var displayName = new GUIContent(iterProp.DisplayName);
                if (ElementNameCallback != null)
                {
                    var elementName = ElementNameCallback(index);
                    displayName = elementName == null ? GUIContent.none : new GUIContent(elementName);
                }

                EasyGUI.TryDrawInspectableObject(position, iterProp, displayName, IsDrawObjectReference);
            };

            propList.elementHeightCallback = index => ElementHeightCallback(property, index);

            extendListIndex.Add(property.PropertyPath, propList);
        }

        private float ElementHeightCallback(InspectableProperty property, int index)
        {
            var height = 3f;
            var iterProp = property.GetArrayElementAtIndex(index);

            if (iterProp.IsExpanded || !ElementHeights.ContainsKey(index))
            {
                var displayName = new GUIContent(iterProp.DisplayName);
                if (ElementNameCallback != null)
                {
                    var elementName = ElementNameCallback(index);
                    displayName = elementName == null ? GUIContent.none : new GUIContent(elementName);
                }

                height += EasyGUI.GetInspectableObjectHeight(iterProp, displayName, IsDrawObjectReference);
                if (!iterProp.IsExpanded)
                {
                    ElementHeights.Add(index, height);
                }
            }
            else
            {
                height = ElementHeights[index];
            }

            return height;
        }

        public bool DoProperty(Rect position, InspectableProperty property)
        {
            if (!extendListIndex.ContainsKey(property.PropertyPath))
            {
                return false;
            }

            headerPosition = new Rect(position);
            headerPosition.height = EasyGUI.GetPropertyHeight(property, GUIContent.none, false);
            // Draw the background
            if (DrawBackgroundCallback != null)
            {
                Rect backgroundPosition = new Rect(headerPosition);
                if (property.IsExpanded)
                {
                    backgroundPosition.yMax += 19;
                }
                EditorGUI.DrawRect(backgroundPosition, DrawBackgroundCallback(false, false));
            }

            // Draw header
            if (HeaderCallback != null)
            {
                HeaderCallback(headerPosition);
            }
            else
            {
                string headerName = string.Format(Header, property.DisplayName, property.ArraySize);
                EasyGUI.PropertyField(headerPosition, property, new GUIContent(headerName), false);
            }

            // Draw the reorderable list for the property
            if (property.IsExpanded)
            {
                EditorGUI.BeginDisabledGroup(!Editable);
                if (!property.Editable)
                {
                    EditorGUI.indentLevel++;
                }
                EditorGUI.BeginChangeCheck();
                var sizePosition = new Rect(position);
                sizePosition.yMin = headerPosition.yMax;
                sizePosition.yMax = headerPosition.yMax + EditorGUIUtility.singleLineHeight;
                var newArraySize = Mathf.Clamp(EditorGUI.IntField(sizePosition, Size, property.ArraySize), 0, int.MaxValue);
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(property.InspectableObject.SerializedObject.targetObject, SetArraySize);
                    property.ArraySize = newArraySize;
                    EditorUtility.SetDirty(property.InspectableObject.SerializedObject.targetObject);
                }
                var listPosition = new Rect(position);
                listPosition.xMin += 15;
                listPosition.yMin = sizePosition.yMax;
                extendListIndex[property.PropertyPath].DoList(listPosition);
                if (!property.Editable)
                {
                    EditorGUI.indentLevel--;
                }
                EditorGUI.EndDisabledGroup();
            }

            return true;
        }

        public float GetPropertyHeight(InspectableProperty property)
        {
            var height = EasyGUI.GetPropertyHeight(property, new GUIContent(property.DisplayName), false);

            if (property.IsExpanded)
            {
                for (int i = 0; i < property.ArraySize; i++)
                {
                    var iterProp = property.GetArrayElementAtIndex(i);
                    var displayName = new GUIContent(iterProp.DisplayName);
                    if (ElementNameCallback != null)
                    {
                        iterProp.DisplayName = ElementNameCallback(i);
                        displayName = iterProp.DisplayName == null ? GUIContent.none : new GUIContent(iterProp.DisplayName);
                    }
                    height += EasyGUI.GetInspectableObjectHeight(iterProp, displayName, IsDrawObjectReference);
                }
                height += 3 * EditorGUIUtility.singleLineHeight;
            }
            return height;
        }

        #endregion
    }
}
