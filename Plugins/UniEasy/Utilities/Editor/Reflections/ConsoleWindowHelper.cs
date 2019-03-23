using System.Reflection;

namespace UniEasy.Editor
{
    public class ConsoleWindowHelper
    {
        #region Static Fields

        private static object consoleWindow;
        private static object listView;
        private static FieldInfo consoleWindowFieldInfo;
        private static FieldInfo listViewFieldInfo;
        private static FieldInfo listViewCurrentRow;
        private static FieldInfo logEntryCondition;
        private static MethodInfo getEntryInternal;
        private static MethodInfo setActiveEntry;

        #endregion

        #region Static Properties

        protected static object ConsoleWindow
        {
            get
            {
                if (consoleWindowFieldInfo == null)
                {
                    consoleWindowFieldInfo = TypeHelper.ConsoleWindowType.GetField("ms_ConsoleWindow", BindingFlags.Static | BindingFlags.NonPublic);
                }
                if (consoleWindowFieldInfo != null)
                {
                    consoleWindow = consoleWindowFieldInfo.GetValue(null);
                }
                return consoleWindow;
            }
        }

        public static object ListView
        {
            get
            {
                if (listViewFieldInfo == null)
                {
                    listViewFieldInfo = TypeHelper.ConsoleWindowType.GetField("m_ListView", BindingFlags.Instance | BindingFlags.NonPublic);
                }
                if (listViewFieldInfo != null)
                {
                    listView = listViewFieldInfo.GetValue(ConsoleWindow);
                }
                return listView;
            }
        }

        #endregion

        #region Methods

        public static int GetCurrentRow()
        {
            if (ListView != null && listViewCurrentRow == null)
            {
                listViewCurrentRow = listViewFieldInfo.FieldType.GetField("row", BindingFlags.Instance | BindingFlags.Public);
            }
            if (ListView != null && listViewCurrentRow != null)
            {
                return (int)listViewCurrentRow.GetValue(ListView);
            }
            return 0;
        }

        public static void GetEntryInternal(int row, object logEntity)
        {
            if (getEntryInternal == null)
            {
                getEntryInternal = TypeHelper.LogEntriesType.GetMethod("GetEntryInternal", BindingFlags.Static | BindingFlags.Public);
            }
            if (getEntryInternal != null)
            {
                getEntryInternal.Invoke(null, new object[] { row, logEntity });
            }
        }

        public static void SetActiveEntry(object logEntity)
        {
            if (setActiveEntry == null)
            {
                setActiveEntry = TypeHelper.ConsoleWindowType.GetMethod("SetActiveEntry", BindingFlags.Instance | BindingFlags.Public);
            }
            if (setActiveEntry != null)
            {
                setActiveEntry.Invoke(ConsoleWindow, new object[] { logEntity });
            }
        }

        public static string GetCondition(object logEntity)
        {
            if (logEntryCondition == null)
            {
                logEntryCondition = TypeHelper.LogEntryType.GetField("condition", BindingFlags.Instance | BindingFlags.Public);
            }
            if (logEntryCondition != null)
            {
                return logEntryCondition.GetValue(logEntity).ToString();
            }
            return null;
        }

        #endregion
    }
}
