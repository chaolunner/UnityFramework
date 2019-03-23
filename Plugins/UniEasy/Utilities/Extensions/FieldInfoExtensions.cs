using System.Reflection;
using System;

namespace UniEasy
{
    public static partial class FieldInfoExtensions
    {
        #region Static Fields

        private static Type SerializeFieldType = typeof(UnityEngine.SerializeField);
        private static Type HideInInspectorType = typeof(UnityEngine.HideInInspector);

        #endregion

        #region Static Methods

        public static bool IsVisible(this FieldInfo field)
        {
            var serializeFieldAttributes = field.GetCustomAttributes(SerializeFieldType, true);
            var hideInInspectorAttributes = field.GetCustomAttributes(HideInInspectorType, true);
            if (hideInInspectorAttributes != null && hideInInspectorAttributes.Length > 0)
            {
            }
            else if (field.IsPublic || (serializeFieldAttributes != null && serializeFieldAttributes.Length > 0))
            {
                return field.FieldType.IsVisible();
            }
            return false;
        }

        #endregion
    }
}
