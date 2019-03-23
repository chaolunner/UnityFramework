using System.Reflection;

namespace UniEasy.Editor
{
    public class EventCommandNamesHelper
    {
        #region Static Fields

        private static FieldInfo deleteFieldInfo;
        private static FieldInfo softDeleteFieldInfo;
        private static FieldInfo duplicateFieldInfo;

        #endregion

        #region Static Properties

        public static string Delete
        {
            get
            {
                if (deleteFieldInfo == null)
                {
                    deleteFieldInfo = TypeHelper.EventCommandNamesType.GetField("Delete", BindingFlags.Instance | BindingFlags.Public);
                }
                if (deleteFieldInfo != null)
                {
                    return deleteFieldInfo.GetValue(null).ToString();
                }
                return null;
            }
        }

        public static string SoftDelete
        {
            get
            {
                if (softDeleteFieldInfo == null)
                {
                    softDeleteFieldInfo = TypeHelper.EventCommandNamesType.GetField("SoftDelete", BindingFlags.Instance | BindingFlags.Public);
                }
                if (softDeleteFieldInfo != null)
                {
                    return softDeleteFieldInfo.GetValue(null).ToString();
                }
                return null;
            }
        }

        public static string Duplicate
        {
            get
            {
                if (duplicateFieldInfo == null)
                {
                    duplicateFieldInfo = TypeHelper.EventCommandNamesType.GetField("Duplicate", BindingFlags.Instance | BindingFlags.Public);
                }
                if (duplicateFieldInfo != null)
                {
                    return duplicateFieldInfo.GetValue(null).ToString();
                }
                return null;
            }
        }

        #endregion
    }
}
