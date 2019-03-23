using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace UniEasy.Editor
{
    public class EditorGUIHelper
    {
        #region Static Fields

        private static PropertyInfo indentPropertyInfo;
        private static PropertyInfo isCollectingTooltipsPropertyInfo;
        private static MethodInfo checkForCrossSceneReferencingMethodInfo;
        private static MethodInfo arraySizeFieldMethodInfo;
        private static MethodInfo getGameObjectFromObjectMethodInfo;
        private static MethodInfo endEditingActiveTextFieldMethodInfo;
        private static MethodInfo labelHasContentMethodInfo;
        private static MethodInfo hasVisibleChildFieldsMethodInfo;
        private static MethodInfo getSinglePropertyHeightMethodInfo;
        private static MethodInfo defaultPropertyFieldMethodInfo;
        private static MethodInfo gradientFieldMethodInfo;

        #endregion

        #region Static Properties

        public static float Indent
        {
            get
            {
                if (indentPropertyInfo == null)
                {
                    indentPropertyInfo = typeof(EditorGUI).GetProperty("indent", BindingFlags.Static | BindingFlags.NonPublic);
                }
                if (indentPropertyInfo != null)
                {
                    return (float)indentPropertyInfo.GetValue(null, null);
                }
                return 0f;
            }
        }

        public static bool IsCollectingTooltips
        {
            get
            {
                if (isCollectingTooltipsPropertyInfo == null)
                {
                    isCollectingTooltipsPropertyInfo = typeof(EditorGUI).GetProperty("isCollectingTooltips", BindingFlags.Static | BindingFlags.NonPublic);
                }
                if (isCollectingTooltipsPropertyInfo != null)
                {
                    return (bool)isCollectingTooltipsPropertyInfo.GetValue(null, null);
                }
                return false;
            }
        }

        #endregion

        #region Static Methods

        public static bool CheckForCrossSceneReferencing(Object obj1, Object obj2)
        {
            if (checkForCrossSceneReferencingMethodInfo == null)
            {
                checkForCrossSceneReferencingMethodInfo = typeof(EditorGUI).GetMethod("CheckForCrossSceneReferencing", BindingFlags.Static | BindingFlags.NonPublic);
            }
            if (checkForCrossSceneReferencingMethodInfo != null)
            {
                return (bool)checkForCrossSceneReferencingMethodInfo.Invoke(null, new object[] { obj1, obj2 });
            }
            return false;
        }

        public static int GetArraySizeField(Rect position, GUIContent label, int value, GUIStyle style)
        {
            if (arraySizeFieldMethodInfo == null)
            {
                arraySizeFieldMethodInfo = typeof(EditorGUI).GetMethod("ArraySizeField", BindingFlags.Static | BindingFlags.NonPublic);
            }
            if (arraySizeFieldMethodInfo != null)
            {
                return (int)arraySizeFieldMethodInfo.Invoke(null, new object[] { position, label, value, style });
            }
            return 0;
        }

        public static GameObject GetGameObjectFromObject(Object obj)
        {
            if (getGameObjectFromObjectMethodInfo == null)
            {
                getGameObjectFromObjectMethodInfo = typeof(EditorGUI).GetMethod("GetGameObjectFromObject", BindingFlags.Static | BindingFlags.NonPublic);
            }
            if (getGameObjectFromObjectMethodInfo != null)
            {
                return (GameObject)getGameObjectFromObjectMethodInfo.Invoke(null, new object[] { obj });
            }
            return null;
        }

        public static void EndEditingActiveTextField()
        {
            if (endEditingActiveTextFieldMethodInfo == null)
            {
                endEditingActiveTextFieldMethodInfo = typeof(EditorGUI).GetMethod("EndEditingActiveTextField", BindingFlags.Static | BindingFlags.NonPublic);
            }
            if (endEditingActiveTextFieldMethodInfo != null)
            {
                endEditingActiveTextFieldMethodInfo.Invoke(null, null);
            }
        }

        public static bool LabelHasContent(GUIContent label)
        {

            if (labelHasContentMethodInfo == null)
            {
                labelHasContentMethodInfo = typeof(EditorGUI).GetMethod("LabelHasContent", BindingFlags.Static | BindingFlags.NonPublic);
            }
            if (labelHasContentMethodInfo != null)
            {
                return (bool)labelHasContentMethodInfo.Invoke(null, new object[] { label });
            }
            return false;
        }

        public static bool HasVisibleChildFields(SerializedProperty property)
        {

            if (hasVisibleChildFieldsMethodInfo == null)
            {
                hasVisibleChildFieldsMethodInfo = typeof(EditorGUI).GetMethod("HasVisibleChildFields", BindingFlags.Static | BindingFlags.NonPublic);
            }
            if (hasVisibleChildFieldsMethodInfo != null)
            {
                return (bool)hasVisibleChildFieldsMethodInfo.Invoke(null, new object[] { property });
            }
            return false;
        }

        public static float GetSinglePropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (getSinglePropertyHeightMethodInfo == null)
            {
                getSinglePropertyHeightMethodInfo = typeof(EditorGUI).GetMethod("GetSinglePropertyHeight", BindingFlags.Static | BindingFlags.NonPublic);
            }
            if (getSinglePropertyHeightMethodInfo != null)
            {
                return (float)getSinglePropertyHeightMethodInfo.Invoke(null, new object[] { property, label });
            }
            return 0;
        }

        public static bool DefaultPropertyField(Rect position, SerializedProperty property, GUIContent label)
        {

            if (defaultPropertyFieldMethodInfo == null)
            {
                defaultPropertyFieldMethodInfo = typeof(EditorGUI).GetMethod("DefaultPropertyField", BindingFlags.Static | BindingFlags.NonPublic);
            }
            if (defaultPropertyFieldMethodInfo != null)
            {
                return (bool)defaultPropertyFieldMethodInfo.Invoke(null, new object[] { position, property, label });
            }
            return false;
        }

        public static Gradient GradientField(GUIContent label, Rect position, Gradient gradient)
        {
            if (gradientFieldMethodInfo == null)
            {
                gradientFieldMethodInfo = typeof(EditorGUI).GetMethod("GradientField", BindingFlags.Static | BindingFlags.NonPublic, null, new System.Type[] { typeof(GUIContent), typeof(Rect), typeof(Gradient) }, null);
            }
            if (gradientFieldMethodInfo != null)
            {
                if (gradient == null)
                {
                    gradient = new Gradient();
                }
                return (Gradient)gradientFieldMethodInfo.Invoke(null, new object[] { label, position, gradient });
            }
            return null;
        }

        #endregion
    }
}
