using System.Collections.Generic;
using UnityEditorInternal;
using UnityEditor;
using UnityEngine;

namespace UniEasy.Editor
{
    public partial class ReorderableListData
    {
        #region Fields

        public bool IsDrawObjectReference;
        public System.Func<bool> EditableCallback;
        public System.Func<Rect, string> HeaderCallback = null;
        public System.Func<GenericMenu, bool> HeaderMenuCallback = null;
        public System.Func<Rect, GUIContent, InspectableObject, bool> ElementHeaderCallback = null;
        public System.Action<Rect, InspectableObject> ElementFooterCallback = null;
        public System.Func<int, string> ElementNameCallback = null;
        public System.Func<bool, bool, Color> DrawBackgroundCallback = null;

        private Rect headerPosition;
        private Dictionary<int, float> ElementHeights = new Dictionary<int, float>();
        private readonly Dictionary<string, ReorderableList> listIndex = new Dictionary<string, ReorderableList>();
        private readonly Dictionary<string, System.Action<SerializedProperty, Object[]>> propDropHandlers = new Dictionary<string, System.Action<SerializedProperty, Object[]>>();
        private readonly Dictionary<string, int> countIndex = new Dictionary<string, int>();

        private static string SetArraySize = "Set Array Size";
        private static string Header = "{0} [{1}]";
        private static string Size = "Size";

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

            if (listIndex.ContainsKey(property.propertyPath))
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
                var displayName = new GUIContent(iterProp.displayName);
                if (ElementNameCallback != null)
                {
                    var elementName = ElementNameCallback(index);
                    displayName = elementName == null ? GUIContent.none : new GUIContent(elementName);
                }

                position.xMin += 5;
                if (EasyGUI.TryDrawInspectableObject(position, iterProp, IsDrawObjectReference, ElementHeaderCallback, ElementFooterCallback))
                {
                }
                else
                {
                    EasyGUI.TryDrawPropertyField(position, iterProp, displayName, IsDrawObjectReference);
                }
            };

            propList.elementHeightCallback = index => ElementHeightCallback(property, index);

            listIndex.Add(property.propertyPath, propList);
        }

        private float ElementHeightCallback(SerializedProperty property, int index)
        {
            var height = 3f;
            var iterProp = property.GetArrayElementAtIndex(index);

            if (iterProp.isExpanded || !ElementHeights.ContainsKey(index))
            {
                var displayName = new GUIContent(iterProp.displayName);
                if (ElementNameCallback != null)
                {
                    var elementName = ElementNameCallback(index);
                    displayName = elementName == null ? GUIContent.none : new GUIContent(elementName);
                }

                if (iterProp.IsInspectableObjectData())
                {
                    height += EasyGUI.GetInspectableObjectHeight(iterProp, IsDrawObjectReference);
                }
                else
                {
                    height += EasyGUI.GetPropertyFieldHeight(iterProp, displayName, IsDrawObjectReference);
                }
                if (!iterProp.isExpanded)
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

        public bool DoProperty(Rect position, SerializedProperty property)
        {
            if (!listIndex.ContainsKey(property.propertyPath))
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
                string headerName = string.Format(Header, property.displayName, property.arraySize);
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
                var newArraySize = Mathf.Clamp(EditorGUI.IntField(sizePosition, Size, property.arraySize), 0, int.MaxValue);
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(property.serializedObject.targetObject, SetArraySize);
                    property.arraySize = newArraySize;
                    EditorUtility.SetDirty(property.serializedObject.targetObject);
                }
                var listHeight = listIndex[property.propertyPath].GetHeight();
                var listPosition = new Rect(sizePosition);
                listPosition.yMin = sizePosition.yMax;
                listPosition.height = listHeight;
                var indentLevel = EditorGUI.indentLevel;
                listPosition.xMin += EasyGUI.Indent;
                EditorGUI.indentLevel = 0;
                listIndex[property.propertyPath].DoList(listPosition);
                EditorGUI.indentLevel = indentLevel;
                if (!property.editable)
                {
                    EditorGUI.indentLevel--;
                }
                EditorGUI.EndDisabledGroup();
            }

            // Do dropdown menu for the header
            if (property.IsInspectableObjectDataArrayOrList() && Event.current.type == EventType.MouseDown && headerPosition.Contains(Event.current.mousePosition))
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
            if (listIndex.ContainsKey(property.propertyPath))
            {
                height += EditorGUIUtility.singleLineHeight;
                if (property.isExpanded)
                {
                    height += EditorGUIUtility.singleLineHeight + listIndex[property.propertyPath].GetHeight();
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
            if (countIndex.TryGetValue(property.propertyPath, out count))
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

            countIndex.Add(property.propertyPath, childCount);
            return childCount;
        }

        public ReorderableList GetPropertyList(SerializedProperty property)
        {
            if (listIndex.ContainsKey(property.propertyPath))
            {
                return listIndex[property.propertyPath];
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
