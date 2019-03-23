using System.Reflection;
using UnityEngine;

namespace UniEasy
{
    public class GUIUtilityHelper
    {
        #region Static Fields

        private static MethodInfo getPermanentControlIDMethodInfo;

        #endregion

        #region Static Methods

        public static int GetPermanentControlID()
        {
            if (getPermanentControlIDMethodInfo == null)
            {
                getPermanentControlIDMethodInfo = typeof(GUIUtility).GetMethod("GetPermanentControlID", BindingFlags.Static | BindingFlags.NonPublic);
            }
            if (getPermanentControlIDMethodInfo != null)
            {
                return (int)getPermanentControlIDMethodInfo.Invoke(null, null);
            }
            return 0;
        }

        #endregion
    }
}
