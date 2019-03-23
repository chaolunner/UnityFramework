using UnityEditor.Callbacks;
using UnityEditor;
using System.Linq;
using System;

namespace UniEasy.Editor
{
    public class LogUtility : UnityEditor.Editor
    {
        #region Static Fields

        private static string projectPath;
        private static object logEntry;

        #endregion

        #region Properties

        public static object LogEntry
        {
            get
            {
                if (logEntry == null)
                {
                    logEntry = Activator.CreateInstance(TypeHelper.LogEntryType);
                }
                return logEntry;
            }
        }

        public static string ProjectPath
        {
            get
            {
                if (string.IsNullOrEmpty(projectPath))
                {
                    projectPath = System.IO.Path.GetDirectoryName(UnityEngine.Application.dataPath);
                }
                return projectPath;
            }
        }

        #endregion

        #region Methods

        [OnOpenAsset(0)]
        public static bool OnOpenAsset(int instanceID, int line)
        {
            var lineNumber = 1;
            var filename = "";
            var monoScript = EditorUtility.InstanceIDToObject(instanceID) as MonoScript;
            if (monoScript != null && monoScript.GetClass() == typeof(Console.Debugger) && GetDoubleClickLine(out filename, out lineNumber))
            {
                if (lineNumber == -1)
                {
                    InternalEditorUtilityHelper.OpenFileAtLineExternal(ProjectPath + "/" + filename, 1);
                }
                else
                {
                    InternalEditorUtilityHelper.OpenFileAtLineExternal(ProjectPath + "/" + filename, lineNumber);
                }
                return true;
            }
            // didn't find a code file? let unity figure it out
            return false;
        }

        private static bool GetDoubleClickLine(out string filename, out int line)
        {
            line = -1;
            filename = "";
            if (ConsoleWindowHelper.ListView == null)
            {
                return false;
            }
            else
            {
                int row = ConsoleWindowHelper.GetCurrentRow();
                if (row < 0)
                {
                    return false;
                }
                ConsoleWindowHelper.GetEntryInternal(row, LogEntry);
                ConsoleWindowHelper.SetActiveEntry(LogEntry);
                var condition = ConsoleWindowHelper.GetCondition(LogEntry);
                var lines = condition.Split(new char[] {
                    '\n',
                });
                var helpful = lines.Where(l =>
                             !l.StartsWith("UnityEngine.Debug") &&
                             !l.StartsWith("UnityEngine.Logger") &&
                             !l.StartsWith("UniEasy.Console.Debugger") &&
                             !string.IsNullOrEmpty(l)).ToArray();
                if (helpful.Length < 2)
                {
                    return false;
                }
                var content = helpful.GetValue(1).ToString();
                var startIndex = content.IndexOf("(at ") + 4;
                var endIndex = content.IndexOf(")", startIndex);
                if (startIndex == -1 || endIndex == -1)
                {
                    return false;
                }
                var target = content.Substring(startIndex, endIndex - startIndex).Split(new char[] {
                    ':',
                });
                filename = target[0];
                int.TryParse(target[1], out line);
                return true;
            }
        }

        #endregion
    }
}
