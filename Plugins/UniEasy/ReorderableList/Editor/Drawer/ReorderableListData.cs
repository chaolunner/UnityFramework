using System.Collections.Generic;
using UnityEditorInternal;
using UnityEditor;
using UnityEngine;

namespace UniEasy.Editor
{
    public class ReorderableListData
    {
        #region Fields

        public List<PropertyAttribute> ElementAttributes = new List<PropertyAttribute>();
        public System.Func<bool> EditableCallback;
        public System.Func<Rect, string> HeaderCallback = null;
        public System.Func<GenericMenu, bool> HeaderMenuCallback = null;
        public System.Func<int, string> ElementNameCallback = null;
        public System.Func<bool, bool, Color> DrawBackgroundCallback = null;

        private Rect headerPosition;
        private readonly Dictionary<string, ReorderableList> reorderableListDict = new Dictionary<string, ReorderableList>();
        private readonly Dictionary<string, System.Action<SerializedProperty, Object[]>> propDropHandlers = new Dictionary<string, System.Action<SerializedProperty, Object[]>>();
        private readonly Dictionary<string, int> countDict = new Dictionary<string, int>();

        private const string SetArraySizeStr = "Set Array Size";
        private const string HeaderStr = "{0} [{1}]";
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

        public ReorderableListData(string parent)
        {
            Parent = parent;
        }

        #endregion

        #region Methods

        public void AddProperty(SerializedProperty property)
        {
            // Check if this property actually belongs to the same direct child
            if (!property.GetRootPath().Equals(Parent))
            {
                return;
            }

            if (reorderableListDict.ContainsKey(property.propertyPath))
            {
                return;
            }

            var propList = new ReorderableList(
                               property.serializedObject, property,
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
                var displayName = new GUIContent(iterProp.displayName);
                if (ElementNameCallback != null)
                {
                    var elementName = ElementNameCallback(index);
                    displayName = elementName == null ? GUIContent.none : new GUIContent(elementName);
                }

                position.xMin += 5;
                EasyGUI.PropertyField(position, iterProp, displayName, ElementAttributes);
            };

            propList.elementHeightCallback = index => ElementHeightCallback(property, index);

            reorderableListDict.Add(property.propertyPath, propList);
        }

        private float ElementHeightCallback(SerializedProperty property, int index)
        {
            var height = 3f;
            var iterProp = property.GetArrayElementAtIndex(index);
            var displayName = new GUIContent(iterProp.displayName);

            if (ElementNameCallback != null)
            {
                var elementName = ElementNameCallback(index);
                displayName = elementName == null ? GUIContent.none : new GUIContent(elementName);
            }
            height += EasyGUI.GetPropertyHeight(iterProp, ElementAttributes, displayName);

            return height;
        }

        public bool DoProperty(Rect position, SerializedProperty property)
        {
            if (!reorderableListDict.ContainsKey(property.propertyPath))
            {
                return false;
            }

            headerPosition = new Rect(position);
            headerPosition.height = EditorGUIUtility.singleLineHeight;
            // Draw the background
            if (DrawBackgroundCallback != null)
            {
                Rect backgroundPosition = new Rect(headerPosition);
                if (property.isExpanded)
                {
                    backgroundPosition.yMax += 15;
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
                string headerName = string.Format(HeaderStr, property.displayName, property.arraySize);
                EditorGUI.PropertyField(headerPosition, property, new GUIContent(headerName), false);
            }

            // Draw the reorderable list for the property
            if (property.isExpanded)
            {
                EditorGUI.BeginDisabledGroup(!Editable);
                if (!property.editable)
                {
                    EditorGUI.indentLevel++;
                }
                EditorGUI.BeginChangeCheck();
                var sizePosition = new Rect(headerPosition);
                sizePosition.yMin = headerPosition.yMax;
                sizePosition.height = EditorGUIUtility.singleLineHeight;
                var newArraySize = Mathf.Clamp(EditorGUI.IntField(sizePosition, SizeStr, property.arraySize), 0, int.MaxValue);
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(property.serializedObject.targetObject, SetArraySizeStr);
                    property.arraySize = newArraySize;
                    EditorUtility.SetDirty(property.serializedObject.targetObject);
                }
                var listHeight = reorderableListDict[property.propertyPath].GetHeight();
                var listPosition = new Rect(sizePosition);
                listPosition.yMin = sizePosition.yMax;
                listPosition.height = listHeight;
                var indentLevel = EditorGUI.indentLevel;
                listPosition.xMin += EasyGUI.Indent;
                EditorGUI.indentLevel = 0;
                reorderableListDict[property.propertyPath].DoList(listPosition);
                EditorGUI.indentLevel = indentLevel;
                if (!property.editable)
                {
                    EditorGUI.indentLevel--;
                }
                EditorGUI.EndDisabledGroup();
            }

            // Do dropdown menu for the header
            if (property.isArray && Event.current.type == EventType.MouseDown && headerPosition.Contains(Event.current.mousePosition))
            {
                GenericMenu popupMenu = new GenericMenu();
                if (HeaderMenuCallback != null && HeaderMenuCallback(popupMenu))
                {
                    Event.current.Use();
                    if (popupMenu.GetItemCount() != 0)
                    {
                        popupMenu.ShowAsContext();
                    }
                }
            }

            // Handle drag and drop into the header
            Event evt = Event.current;
            if (evt == null)
            {
                return true;
            }

            if (evt.type == EventType.DragUpdated || evt.type == EventType.DragPerform)
            {
                if (!headerPosition.Contains(evt.mousePosition))
                {
                    return true;
                }

                DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
                if (evt.type == EventType.DragPerform)
                {
                    DragAndDrop.AcceptDrag();
                    System.Action<SerializedProperty, Object[]> handler = null;
                    if (propDropHandlers.TryGetValue(property.propertyPath, out handler))
                    {
                        if (handler != null)
                        {
                            handler(property, DragAndDrop.objectReferences);
                        }
                    }
                    else
                    {
                        foreach (Object draggedObject in DragAndDrop.objectReferences)
                        {
                            if (draggedObject.GetType() != property.GetType())
                            {
                                continue;
                            }

                            int newIndex = property.arraySize;
                            property.arraySize++;

                            SerializedProperty target = property.GetArrayElementAtIndex(newIndex);
                            target.objectReferenceInstanceIDValue = draggedObject.GetInstanceID();
                        }
                    }
                    evt.Use();
                }
            }

            return true;
        }

        public float GetPropertyHeight(SerializedProperty property)
        {
            var height = 0f;
            if (reorderableListDict.ContainsKey(property.propertyPath))
            {
                height += EditorGUIUtility.singleLineHeight;
                if (property.isExpanded)
                {
                    height += EditorGUIUtility.singleLineHeight + reorderableListDict[property.propertyPath].GetHeight();
                }
            }
            return height;
        }

        public int GetElementCount(SerializedProperty property)
        {
            if (property.arraySize <= 0)
            {
                return 0;
            }

            int count;
            if (countDict.TryGetValue(property.propertyPath, out count))
            {
                return count;
            }

            var element = property.GetArrayElementAtIndex(0);
            var countElement = element.Copy();
            int childCount = 0;
            if (countElement.NextVisible(true))
            {
                int depth = countElement.Copy().depth;
                do
                {
                    if (countElement.depth != depth)
                    {
                        break;
                    }
                    childCount++;
                } while (countElement.NextVisible(false));
            }

            countDict.Add(property.propertyPath, childCount);
            return childCount;
        }

        public ReorderableList GetPropertyList(SerializedProperty property)
        {
            if (reorderableListDict.ContainsKey(property.propertyPath))
            {
                return reorderableListDict[property.propertyPath];
            }
            return null;
        }

        public void SetDropHandler(SerializedProperty property, System.Action<SerializedProperty, Object[]> handler)
        {
            string path = property.propertyPath;
            if (propDropHandlers.ContainsKey(path))
            {
                propDropHandlers[path] = handler;
            }
            else
            {
                propDropHandlers.Add(path, handler);
            }
        }

        #endregion
    }
}
