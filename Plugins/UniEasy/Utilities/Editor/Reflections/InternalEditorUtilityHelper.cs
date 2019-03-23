using System.Reflection;

namespace UniEasy.Editor
{
    public class InternalEditorUtilityHelper
    {
        #region Static Fields

        private static MethodInfo openFileAtLineExternal;

        #endregion

        #region Static Methods

        public static void OpenFileAtLineExternal(string filename, int line)
        {
            if (openFileAtLineExternal == null)
            {
                openFileAtLineExternal = TypeHelper.InternalEditorUtilityType.GetMethod("OpenFileAtLineExternal");
            }
            if (openFileAtLineExternal != null)
            {
                openFileAtLineExternal.Invoke(null, new object[] { filename, line });
            }
        }

        #endregion
    }
}
