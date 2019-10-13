using System.Collections.Generic;
using UnityEditor.IMGUI.Controls;
using UnityEditor;
using UnityEngine;

namespace UniEasy.Editor
{
    public class EasyGUI
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
            ScriptAttributeUtility.s_DrawerStack.Clear();
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

        public static Gradient GradientField(GUIContent label, Rect position, Gradient gradient)
        {
            return EditorGUIHelper.GradientField(label, position, gradient);
        }

        public static bool PropertyField(Rect position, SerializedProperty property, List<PropertyAttribute> attributes)
        {
            return PropertyField(position, property, null, false, attributes);
        }

        public static bool PropertyField(Rect position, SerializedProperty property, GUIContent label, List<PropertyAttribute> attributes)
        {
            return PropertyField(position, property, label, false, attributes);
        }

        public static bool PropertyField(Rect position, SerializedProperty property, GUIContent label, bool includeChildren, List<PropertyAttribute> attributes)
        {
            return PropertyFieldInternal(position, property, label, includeChildren, attributes);
        }

        public static bool PropertyFieldInternal(Rect position, SerializedProperty property, GUIContent label, bool includeChildren, List<PropertyAttribute> attributes)
        {
            return ScriptAttributeUtility.GetHandler(property, attributes).OnGUI(position, property, label, includeChildren);
        }

        public static bool DefaultPropertyField(Rect position, SerializedProperty property, GUIContent label)
        {
            return EditorGUIHelper.DefaultPropertyField(position, property, label);
        }

        public static float GetSinglePropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUIHelper.GetSinglePropertyHeight(property, label);
        }

        public static float GetPropertyHeight(SerializedProperty property, List<PropertyAttribute> attributes, GUIContent label = null, bool includeChildren = true)
        {
            return ScriptAttributeUtility.GetHandler(property, attributes).GetHeight(property, label, includeChildren);
        }

        #endregion
    }
}
