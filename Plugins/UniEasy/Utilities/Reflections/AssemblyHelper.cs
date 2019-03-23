using System.Reflection;
using System;

namespace UniEasy
{
    public class AssemblyHelper
    {
        private static Assembly cSharpFirstpass;

        public static Assembly CSharpFirstpass
        {
            get
            {
                if (cSharpFirstpass == null)
                {
                    foreach (var a in AppDomain.CurrentDomain.GetAssemblies())
                    {
                        if (a.GetName().Name == "Assembly-CSharp-firstpass")
                        {
                            cSharpFirstpass = a;
                        }
                    }
                }
                return cSharpFirstpass;
            }
        }

        private static Assembly cSharp;

        public static Assembly CSharp
        {
            get
            {
                if (cSharp == null)
                {
                    foreach (var a in AppDomain.CurrentDomain.GetAssemblies())
                    {
                        if (a.GetName().Name == "Assembly-CSharp")
                        {
                            cSharp = a;
                        }
                    }
                }
                return cSharp;
            }
        }

        private static Assembly cSharpEditorFirstpass;

        public static Assembly CSharpEditorFirstpass
        {
            get
            {
                if (cSharpEditorFirstpass == null)
                {
                    foreach (var a in AppDomain.CurrentDomain.GetAssemblies())
                    {
                        if (a.GetName().Name == "Assembly-CSharp-Editor-firstpass")
                        {
                            cSharpEditorFirstpass = a;
                        }
                    }
                }
                return cSharpEditorFirstpass;
            }
        }

        private static Assembly cSharpEditor;

        public static Assembly CSharpEditor
        {
            get
            {
                if (cSharpEditor == null)
                {
                    foreach (var a in AppDomain.CurrentDomain.GetAssemblies())
                    {
                        if (a.GetName().Name == "Assembly-CSharp-Editor")
                        {
                            cSharpEditor = a;
                        }
                    }
                }
                return cSharpEditor;
            }
        }

#if UNITY_EDITOR
        private static Assembly editorWindow;

        public static Assembly EditorWindow
        {
            get
            {
                if (editorWindow == null)
                {
                    editorWindow = Assembly.GetAssembly(typeof(UnityEditor.EditorWindow));
                }
                return editorWindow;
            }
        }

        private static Assembly sceneView;

        public static Assembly SceneView
        {
            get
            {
                if (sceneView == null)
                {
                    sceneView = Assembly.GetAssembly(typeof(UnityEditor.SceneView));
                }
                return sceneView;
            }
        }

        private static Assembly unityEditor;

        public static Assembly UnityEditor
        {
            get
            {
                if (unityEditor == null)
                {
                    unityEditor = Assembly.Load(new AssemblyName("UnityEditor"));
                }
                return unityEditor;
            }
        }

        private static Assembly unityEngine;

        public static Assembly UnityEngine
        {
            get
            {
                if (unityEngine == null)
                {
                    unityEngine = Assembly.Load(new AssemblyName("UnityEngine"));
                }
                return unityEngine;
            }
        }

        public static Type[] GetTypesFromAssembly(Assembly assembly)
        {
            Type[] result;
            if (assembly == null)
            {
                result = new Type[0];
            }
            else
            {
                try
                {
                    result = assembly.GetTypes();
                }
                catch (ReflectionTypeLoadException)
                {
                    result = new Type[0];
                }
            }
            return result;
        }
#endif
    }
}
