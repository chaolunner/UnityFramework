using System.Reflection;
using UnityEditor;

namespace UniEasy.Editor
{
    public class SerializedObjectHelper
    {
        #region Static Fields

        private static PropertyInfo inspectorModePropertyInfo;
        private static string InspectorModeStr = "inspectorMode";

        #endregion

        #region Static Methods

        public static InspectorMode GetInspectorMode(SerializedObject serializedObject)
        {
            if (inspectorModePropertyInfo == null)
            {
                inspectorModePropertyInfo = typeof(SerializedObject).GetProperty(InspectorModeStr, BindingFlags.NonPublic);
            }
            if (inspectorModePropertyInfo != null && serializedObject != null)
            {
                return (InspectorMode)inspectorModePropertyInfo.GetValue(serializedObject, null);
            }
            return UnityEditor.InspectorMode.Normal;
        }

        #endregion
    }
}
