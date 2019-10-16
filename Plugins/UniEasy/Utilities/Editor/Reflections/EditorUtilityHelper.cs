using System.Reflection;
using UnityEditor;

namespace UniEasy.Editor
{
    public class EditorUtilityHelper
    {
        #region Static Fields

        private static MethodInfo forceReloadInspectorsMethodInfo;

        #endregion

        #region Static Methods

        public static void ForceReloadInspectors()
        {
            if (forceReloadInspectorsMethodInfo == null)
            {
                forceReloadInspectorsMethodInfo = typeof(EditorUtility).GetMethod("ForceReloadInspectors", BindingFlags.Static | BindingFlags.NonPublic);
            }
            if (forceReloadInspectorsMethodInfo != null)
            {
                forceReloadInspectorsMethodInfo.Invoke(null, null);
            }
        }

        #endregion
    }
}
