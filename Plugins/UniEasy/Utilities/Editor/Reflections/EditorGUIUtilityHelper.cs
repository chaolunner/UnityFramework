using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace UniEasy.Editor
{
    public class EditorGUIUtilityHelper
    {
        #region Static Fields

        private static FieldInfo s_LastControlIDFieldInfo;
        private static MethodInfo tempContentMethodInfo;
        private static MethodInfo textContentMethodInfo;
        private static MethodInfo getBoldDefaultFontMethodInfo;
        private static MethodInfo setBoldDefaultFontMethodInfo;

        #endregion

        #region Static Properties

        public static int s_LastControlID
        {
            get
            {
                if (s_LastControlIDFieldInfo == null)
                {
                    s_LastControlIDFieldInfo = typeof(EditorGUIUtility).GetField("s_LastControlID", BindingFlags.Static | BindingFlags.NonPublic);
                }
                if (s_LastControlIDFieldInfo != null)
                {
                    return (int)s_LastControlIDFieldInfo.GetValue(null);
                }
                return 0;
            }
        }

        #endregion

        #region Static Methods

        public static GUIContent TempContent(string t)
        {
            if (tempContentMethodInfo == null)
            {
                tempContentMethodInfo = typeof(EditorGUIUtility).GetMethod("TempContent", BindingFlags.Static | BindingFlags.NonPublic, null, new System.Type[] { typeof(string) }, null);
            }
            if (tempContentMethodInfo != null)
            {
                return (GUIContent)tempContentMethodInfo.Invoke(null, new object[] { t });
            }
            return GUIContent.none;
        }

        public static GUIContent TextContent(string textAndTooltip)
        {
            if (textContentMethodInfo == null)
            {
                textContentMethodInfo = typeof(EditorGUIUtility).GetMethod("TextContent", BindingFlags.Static | BindingFlags.NonPublic, null, new System.Type[] { typeof(string) }, null);
            }
            if (textContentMethodInfo != null)
            {
                return (GUIContent)textContentMethodInfo.Invoke(null, new object[] { textAndTooltip });
            }
            return GUIContent.none;
        }

        public static bool GetBoldDefaultFont()
        {
            if (getBoldDefaultFontMethodInfo == null)
            {
                getBoldDefaultFontMethodInfo = typeof(EditorGUIUtility).GetMethod("GetBoldDefaultFont", BindingFlags.Static | BindingFlags.NonPublic);
            }
            if (getBoldDefaultFontMethodInfo != null)
            {
                return (bool)getBoldDefaultFontMethodInfo.Invoke(null, null);
            }
            return false;
        }

        public static void SetBoldDefaultFont(bool isBold)
        {
            if (setBoldDefaultFontMethodInfo == null)
            {
                setBoldDefaultFontMethodInfo = typeof(EditorGUIUtility).GetMethod("SetBoldDefaultFont", BindingFlags.Static | BindingFlags.NonPublic);
            }
            if (setBoldDefaultFontMethodInfo != null)
            {
                setBoldDefaultFontMethodInfo.Invoke(null, new object[] { isBold });
            }
        }

        #endregion
    }
}
