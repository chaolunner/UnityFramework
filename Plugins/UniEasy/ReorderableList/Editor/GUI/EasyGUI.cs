using System.Collections.Generic;
using UnityEditor.IMGUI.Controls;
using UnityEditor;
using UnityEngine;

namespace UniEasy.Editor
{
    public partial class EasyGUI
    {
        [System.Flags]
        public enum ObjectFieldValidatorOptions
        {
            None = 0,
            ExactObjectTypeValidation = (1 << 0)
        }

        #region Static Fields

        public static int kControlVerticalSpacing = 2;
        public static float kSingleLineHeight = 16;
        public static string EmptyStr = "";

        #endregion

        #region Static Properties

        public static float Indent
        {
            get
            {
                return EditorGUIHelper.Indent;
            }
        }

        public static bool IsCollectingTooltips
        {
            get
            {
                return EditorGUIHelper.IsCollectingTooltips;
            }
        }

        #endregion

        #region Static Methods

        public static void ClearStacks()
        {
            s_PropertyStack.Clear();
            ScriptAttributeUtility.s_DrawerStack.Clear();
            InspectableAttributeUtility.s_DrawerStack.Clear();
        }

        public static bool CheckForCrossSceneReferencing(Object obj1, Object obj2)
        {
            return EditorGUIHelper.CheckForCrossSceneReferencing(obj1, obj2);
        }

        public static int ArraySizeField(Rect position, GUIContent label, int value, GUIStyle style)
        {
            return EditorGUIHelper.GetArraySizeField(position, label, value, style);
        }

        public static GameObject GetGameObjectFromObject(Object obj)
        {
            return EditorGUIHelper.GetGameObjectFromObject(obj);
        }

        public static void EndEditingActiveTextField()
        {
            EditorGUIHelper.EndEditingActiveTextField();
        }

        public static bool LabelHasContent(GUIContent label)
        {
            return EditorGUIHelper.LabelHasContent(label);
        }

        public static bool HasVisibleChildFields(SerializedProperty property)
        {
            return EditorGUIHelper.HasVisibleChildFields(property);
        }

        public static Rect PrefixLabel(Rect position, out Rect labelPosition)
        {
            labelPosition = new Rect(position.x + Indent, position.y, EditorGUIUtility.labelWidth - Indent, 16f);
            return new Rect(position.x + EditorGUIUtility.labelWidth, position.y, position.width - EditorGUIUtility.labelWidth, position.height);
        }

        public static string SearchFieldWithPopupMenu(Rect position, string label, SearchField searchField, string searchCondition, System.Action<GenericMenu> func, string labelHtmlString = "", string tooltip = "")
        {
            Color labelColor;
            Rect labelPosition;

            var searchPosition = PrefixLabel(position, out labelPosition);
            var color = GUI.color;

            if (ColorUtility.TryParseHtmlString(labelHtmlString, out labelColor))
            {
                GUI.color = labelColor;
            }
            EditorGUI.LabelField(labelPosition, new GUIContent(label, tooltip));
            GUI.color = color;

            searchCondition = searchField.OnGUI(searchPosition, searchCondition);

            if (!string.IsNullOrEmpty(searchCondition))
            {
                var popupMenu = new GenericMenu();

                func(popupMenu);

                if ((Event.current.keyCode == KeyCode.KeypadEnter || Event.current.keyCode == KeyCode.Return) && popupMenu.GetItemCount() > 0)
                {
                    popupMenu.DropDown(position);
                }
            }
            return searchCondition;
        }

        private static T CreateCachedEditorWithContext<T>(Object targetObject, Object context) where T : UnityEditor.Editor
        {
            if (targetObject != null && context != null)
            {
                if (!editableIndex.ContainsKey(targetObject))
                {
                    editableIndex.Add(targetObject, new Dictionary<Object, UnityEditor.Editor>());
                }
                if (!editableIndex[targetObject].ContainsKey(context))
                {
                    UnityEditor.Editor scriptableEditor = null;
                    UnityEditor.Editor.CreateCachedEditorWithContext(targetObject, context, null, ref scriptableEditor);
                    editableIndex[targetObject].Add(context, scriptableEditor);
                }
                return editableIndex[targetObject][context] as T;
            }
            return null;
        }

        public static Gradient GradientField(GUIContent label, Rect position, Gradient gradient)
        {
            return EditorGUIHelper.GradientField(label, position, gradient);
        }

        public static bool PropertyField(Rect position, SerializedProperty property)
        {
            return PropertyField(position, property, null, false);
        }

        public static bool PropertyField(Rect position, SerializedProperty property, GUIContent label)
        {
            return PropertyField(position, property, label, false);
        }

        public static bool PropertyField(Rect position, SerializedProperty property, GUIContent label, bool includeChildren)
        {
            return PropertyFieldInternal(position, property, label, includeChildren);
        }

        public static bool PropertyFieldInternal(Rect position, SerializedProperty property, GUIContent label, bool includeChildren)
        {
            return ScriptAttributeUtility.GetHandler(property).OnGUI(position, property, label, includeChildren);
        }

        public static bool DefaultPropertyField(Rect position, SerializedProperty property, GUIContent label)
        {
            return EditorGUIHelper.DefaultPropertyField(position, property, label);
        }

        public static float GetSinglePropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUIHelper.GetSinglePropertyHeight(property, label);
        }

        public static float GetPropertyHeight(SerializedProperty property, GUIContent label = null, bool includeChildren = true)
        {
            return ScriptAttributeUtility.GetHandler(property).GetHeight(property, label, includeChildren);
        }

        public static bool TryDrawObjectReference(Rect position, SerializedProperty property, GUIContent displayName, bool drawObjectReference = false)
        {
            if (drawObjectReference && property != null && property.propertyType == SerializedPropertyType.ObjectReference)
            {
                position.height = EditorGUI.GetPropertyHeight(property, displayName, false);
                if (TryDrawObjectReference(position, property.objectReferenceValue, CreateCachedEditorWithContext<ReorderableListDrawer>(property.objectReferenceValue, property.serializedObject.targetObject)))
                {
                    EditorGUI.PropertyField(position, property, displayName, false);
                    return true;
                }
            }
            return false;
        }

        public static float GetObjectReferenceHeight(SerializedProperty property, bool drawObjectReference = false)
        {
            if (drawObjectReference && property != null && property.propertyType == SerializedPropertyType.ObjectReference)
            {
                var headerHeight = EditorGUI.GetPropertyHeight(property, GUIContent.none, false) - EditorGUIUtility.singleLineHeight;
                return GetObjectReferenceHeight(property.objectReferenceValue, CreateCachedEditorWithContext<ReorderableListDrawer>(property.objectReferenceValue, property.serializedObject.targetObject)) + headerHeight;
            }
            return 0f;
        }

        public static void TryDrawPropertyField(Rect position, SerializedProperty property, GUIContent displayName, bool drawObjectReference = false)
        {
            position.height = GetObjectReferenceHeight(property, drawObjectReference);
            if (TryDrawObjectReference(position, property, displayName, drawObjectReference))
            {
            }
            else
            {
                if (property.hasVisibleChildren && displayName == GUIContent.none)
                {
                    var iterProp = property.Copy();
                    if (iterProp.NextVisible(true))
                    {
                        int depth = iterProp.depth;
                        do
                        {
                            if (depth != iterProp.depth)
                            {
                                break;
                            }
                            var label = new GUIContent(iterProp.displayName);
                            position.yMin += position.height;
                            position.height = GetObjectReferenceHeight(iterProp, drawObjectReference);
                            if (TryDrawObjectReference(position, iterProp, label, drawObjectReference))
                            {
                            }
                            else
                            {
                                position.height = EditorGUI.GetPropertyHeight(iterProp, label, iterProp.isExpanded);
                                EditorGUI.PropertyField(position, iterProp, label, iterProp.isExpanded);
                            }
                        } while (iterProp.NextVisible(false));
                    }
                }
                else
                {
                    position.height = EditorGUI.GetPropertyHeight(property, displayName, property.isExpanded);
                    EditorGUI.PropertyField(position, property, displayName, property.isExpanded);
                }
            }
        }

        public static float GetPropertyFieldHeight(SerializedProperty property, GUIContent displayName, bool drawObjectReference = false)
        {
            var height = GetObjectReferenceHeight(property, drawObjectReference);
            if (height == 0f)
            {
                if (property.hasVisibleChildren && displayName == GUIContent.none)
                {
                    var iterProp = property.Copy();
                    if (iterProp.NextVisible(true))
                    {
                        int depth = iterProp.depth;
                        do
                        {
                            if (depth != iterProp.depth)
                            {
                                break;
                            }
                            var label = new GUIContent(iterProp.displayName);
                            var height2 = GetObjectReferenceHeight(iterProp, drawObjectReference);
                            if (height2 == 0f)
                            {
                                height += EditorGUI.GetPropertyHeight(iterProp, label, iterProp.isExpanded);
                            }
                            else
                            {
                                height += height2;
                            }
                        } while (iterProp.NextVisible(false));
                    }
                }
                else
                {
                    height += EditorGUI.GetPropertyHeight(property, displayName, property.isExpanded);
                }
            }
            return height;
        }

        public static bool TryDrawInspectableObject(Rect position, SerializedProperty property, bool drawObjectReference = false, System.Func<Rect, GUIContent, InspectableObject, bool> elementHeaderHandler = null, System.Action<Rect, InspectableObject> elementFooterHandler = null)
        {
            var data = property.GetInspectableObjectData();
            if (data != null)
            {
                var inspectableObject = InspectableObject.CreateInstance(property, data);
                if (inspectableObject != null)
                {
                    var iterProp = inspectableObject.GetIterator();
                    var depth = iterProp.Depth;
                    var index = 0;

                    position.height = 0f;
                    do
                    {
                        if (depth != iterProp.Depth || (index > 0 && !inspectableObject.IsExpanded))
                        {
                            break;
                        }

                        var displayName = new GUIContent(iterProp.DisplayName);

                        if (index == 0)
                        {
                            position.height = GetPropertyHeight(iterProp, GUIContent.none, iterProp.IsExpanded);
                            PropertyField(position, iterProp, GUIContent.none, iterProp.IsExpanded);
                            if (elementHeaderHandler != null)
                            {
                                elementHeaderHandler(position, displayName, inspectableObject);
                            }
                            else
                            {
                                TryDrawDefaultElementHeader(position, displayName, inspectableObject);
                            }
                        }

                        position.yMin += position.height;

                        EditorGUI.indentLevel++;
                        position.height = GetObjectReferenceHeight(iterProp, drawObjectReference);
                        if (TryDrawObjectReference(position, iterProp, displayName, drawObjectReference))
                        {
                        }
                        else if (index > 0)
                        {
                            position.height = GetPropertyHeight(iterProp, displayName, iterProp.IsExpanded);
                            PropertyField(position, iterProp, displayName, iterProp.IsExpanded);
                        }
                        EditorGUI.indentLevel--;
                        index++;
                    } while (iterProp.NextVisible(false));

                    elementFooterHandler?.Invoke(position, inspectableObject);
                    inspectableObject.ApplyModifiedProperties();
                    return true;
                }
            }
            return false;
        }

        public static float GetInspectableObjectHeight(SerializedProperty property, bool drawObjectReference = false)
        {
            var height = 0f;
            var height2 = 0f;
            var data = property.GetInspectableObjectData();
            if (data != null)
            {
                var inspectableObject = InspectableObject.CreateInstance(property, data);
                if (inspectableObject != null)
                {
                    var iterProp = inspectableObject.GetIterator();
                    int depth = iterProp.Depth;
                    do
                    {
                        if (depth != iterProp.Depth)
                        {
                            break;
                        }
                        height2 = GetObjectReferenceHeight(iterProp, drawObjectReference);
                        if (height2 == 0f)
                        {
                            height += GetPropertyHeight(iterProp, new GUIContent(iterProp.DisplayName), iterProp.IsExpanded);
                        }
                        else
                        {
                            height += height2;
                        }
                        if (!inspectableObject.IsExpanded)
                        {
                            break;
                        }
                    } while (iterProp.NextVisible(false));
                }
                else
                {
                    height += EditorGUIUtility.singleLineHeight;
                }
            }
            return height;
        }

        #endregion
    }
}
