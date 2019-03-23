using CustomMenu = UnityEngine.ContextMenu;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using System.Linq;
using System;

namespace UniEasy.Editor
{
    public class EasyEditor : UnityEditor.Editor
    {
        #region Static Fields

        public static List<Type> Types;
        public static List<Type> EditorTypes;
        public static Dictionary<CustomMenu, MethodInfo> ContextMenuMethods = new Dictionary<CustomMenu, MethodInfo>();

        private static EditorWindow focusedWindow;
        private static List<DelayCallData> delayCalls;
        private static event Action windowChangeCalls;

        #endregion

        #region Static Methods

        [InitializeOnLoadMethod]
        static void StaticInitialize()
        {
            Types = new List<Type>();
            EditorTypes = new List<Type>();
            delayCalls = new List<DelayCallData>();

            if (AssemblyHelper.CSharp != null)
            {
                Types.AddRange(AssemblyHelper.CSharp.GetTypes().Distinct());
            }
            if (AssemblyHelper.CSharpFirstpass != null)
            {
                Types.AddRange(AssemblyHelper.CSharpFirstpass.GetTypes().Distinct());
            }
            if (AssemblyHelper.CSharpEditor != null)
            {
                EditorTypes.AddRange(AssemblyHelper.CSharpEditor.GetTypes().Distinct());
            }
            if (AssemblyHelper.CSharpEditorFirstpass != null)
            {
                EditorTypes.AddRange(AssemblyHelper.CSharpEditorFirstpass.GetTypes().Distinct());
            }

            foreach (var type in EditorTypes)
            {
                var methods = type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);

                foreach (var method in methods)
                {
                    foreach (var attr in method.AllAttributes<CustomMenu>())
                    {
                        ContextMenuMethods.Add(attr, method);
                    }
                }
            }

            // HACK: No idea of a good solution to deal with the reset GUI state.
            // Maybe still will have some issues, because the GUI state was not reset at the right time.
            OnWindowChange(() =>
            {
                EasyGUIUtility.ResetGUIState();
            });

            EditorApplication.update += Update;
        }

        private static void Update()
        {
            if (delayCalls.Count > 0)
            {
                int r = 0;
                int count = delayCalls.Count;
                for (int i = 0; i < count; i++)
                {
                    DelayCallData call = delayCalls[i - r];
                    if (UnityEngine.Time.realtimeSinceStartup - call.StartTime >= call.Delay)
                    {
                        call.Callback.Invoke();
                        delayCalls.RemoveAt(i - r);
                        r++;
                    }
                }
            }
            if (windowChangeCalls != null && EditorWindow.focusedWindow != focusedWindow)
            {
                windowChangeCalls();
                focusedWindow = EditorWindow.focusedWindow;
            }
        }

        public static void DelayFrame(DelayCallData.CallbackFunction callback, int frameCount)
        {
            delayCalls.Add(new DelayCallData(callback, UnityEngine.Time.realtimeSinceStartup, frameCount * UnityEngine.Time.fixedDeltaTime));
        }

        public static void OnWindowChange(Action action)
        {
            windowChangeCalls += action;
        }

        #endregion
    }
}
