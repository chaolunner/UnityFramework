using System.Reflection;
using UnityEditor;

namespace UniEasy.Editor
{
    public class LocalizationDatabaseHelper
    {
        #region Static Fields

        private static MethodInfo getLocalizedStringMethodInfo;

        #endregion

        #region Static Methods

        public static string GetLocalizedString(string original)
        {
            if (getLocalizedStringMethodInfo == null)
            {
                getLocalizedStringMethodInfo = TypeHelper.LocalizationDatabaseType.GetMethod("GetLocalizedString", BindingFlags.Static | BindingFlags.Public);
            }
            if (getLocalizedStringMethodInfo != null)
            {
                return (string)getLocalizedStringMethodInfo.Invoke(null, new object[] { original });
            }
            return null;
        }

        #endregion
    }
}
