using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace UniEasy.Editor
{
    public class RuntimeReorderableListData
    {
        #region Fields

        public List<PropertyAttribute> ElementAttributes = new List<PropertyAttribute>();
        public System.Func<bool> EditableCallback;
        public System.Func<Rect, string> HeaderCallback = null;
        public System.Func<int, string> ElementNameCallback = null;
        public System.Func<bool, bool, Color> DrawBackgroundCallback = null;

        private Rect headerPosition;

        private readonly Dictionary<string, RuntimeReorderableList> runtimeReorderableListDict = new Dictionary<string, RuntimeReorderableList>();
        private const string SetArraySizeStr = "Set Array Size";
        private const string HeaderStr = "{0} [{1}] [InstanceID : {2}]";
        private const string SizeStr = "Size";

        #endregion

        #region Properties

        public string Parent { get; private set; }

        public bool Editable
        {
            get
            {
                if (EditableCallback != null)
                {
                    return EditableCallback();
                }
                return true;
            }
        }

        #endregion

        #region Constructors

        public RuntimeReorderableListData(string parent)
        {
            Parent = parent;
        }

        #endregion

        #region Methods

        public void AddProperty(RuntimeSerializedProperty property)
        {
            // Check if this property actually belongs to the same direct child
            if (!property.GetRootPath().Equals(Parent))
            {
                return;
            }

            if (runtimeReorderableListDict.ContainsKey(property.PropertyPath))
            {
                return;
            }

            RuntimeReorderableList propList = new RuntimeReorderableList(
                                                 property.RuntimeSerializedObject.SerializedObject, property,
                                                 draggable: true, displayHeader: false,
                                                 displayAddButton: true, displayRemoveButton: true)
            {
                headerHeight = 5
            };

            propList.drawElementBackgroundCallback = (Rect position, int index, bool active, bool focused) =>
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

            propList.drawElementCallback = (Rect position, int index, bool active, bool focused) =>
            {
                var iterProp = property.GetArrayElementAtIndex(index);
                var elementName = iterProp.DisplayName;
                if (ElementNameCallback != null)
                {
                    elementName = ElementNameCallback(index);
                }
                RuntimeEasyGUI.PropertyField(position, iterProp, new GUIContent(elementName), ElementAttributes);
            };

            propList.elementHeightCallback = index => ElementHeightCallback(property, index);

            runtimeReorderableListDict.Add(property.PropertyPath, propList);
        }

        private float ElementHeightCallback(RuntimeSerializedProperty property, int index)
        {
            var height = 3f;
            var iterProp = property.GetArrayElementAtIndex(index);
            var elementName = iterProp.DisplayName;
            if (ElementNameCallback != null)
            {
                elementName = ElementNameCallback(index);
            }
            height += RuntimeEasyGUI.GetPropertyHeight(iterProp, new GUIContent(elementName), ElementAttributes);

            return height;
        }

        public bool DoProperty(Rect position, RuntimeSerializedProperty property)
        {
            if (!runtimeReorderableListDict.ContainsKey(property.PropertyPath))
            {
                return false;
            }

            headerPosition = new Rect(position);
            headerPosition.height = RuntimeEasyGUI.GetPropertyHeight(property, GUIContent.none, false, null);
            // Draw the background
            if (DrawBackgroundCallback != null)
            {
                Rect backgroundPosition = new Rect(headerPosition);
                backgroundPosition.xMin += EasyGUI.Indent;
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
                string headerName = string.Format(HeaderStr, property.DisplayName, property.ArraySize, property.HashCodeForPropertyPath());
                RuntimeEasyGUI.PropertyField(headerPosition, property, new GUIContent(headerName), false, null);
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
                var sizePosition = new Rect(headerPosition);
                sizePosition.yMin = headerPosition.yMax;
                sizePosition.height = EditorGUIUtility.singleLineHeight;
                var newArraySize = Mathf.Clamp(EditorGUI.IntField(sizePosition, SizeStr, property.ArraySize), 0, int.MaxValue);
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(property.RuntimeSerializedObject.TargetObject, SetArraySizeStr);
                    property.ArraySize = newArraySize;
                    EditorUtility.SetDirty(property.RuntimeSerializedObject.TargetObject);
                }
                var listPosition = new Rect(sizePosition);
                listPosition.xMin += EasyGUI.Indent;
                listPosition.yMin = sizePosition.yMax;
                var indentLevel = EditorGUI.indentLevel;
                EditorGUI.indentLevel = 0;
                runtimeReorderableListDict[property.PropertyPath].DoList(listPosition);
                EditorGUI.indentLevel = indentLevel;
                if (!property.Editable)
                {
                    EditorGUI.indentLevel--;
                }
                EditorGUI.EndDisabledGroup();
            }

            return true;
        }

        public float GetPropertyHeight(RuntimeSerializedProperty property)
        {
            var height = 0f;
            if (runtimeReorderableListDict.ContainsKey(property.PropertyPath))
            {
                height += EditorGUIUtility.singleLineHeight;
                if (property.IsExpanded)
                {
                    height += EditorGUIUtility.singleLineHeight + runtimeReorderableListDict[property.PropertyPath].GetHeight();
                }
            }
            return height;
        }

        public RuntimeReorderableList GetPropertyList(RuntimeSerializedProperty property)
        {
            if (runtimeReorderableListDict.ContainsKey(property.PropertyPath))
            {
                return runtimeReorderableListDict[property.PropertyPath];
            }
            return null;
        }

        #endregion
    }
}
