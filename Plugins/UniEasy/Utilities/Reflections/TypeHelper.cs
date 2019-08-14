using System.Reflection;
using System;

namespace UniEasy
{
    public static partial class TypeHelper
    {
        public static Type GetType(string filePath, string typeName)
        {
            var assembly = Assembly.LoadFile(filePath);
            return assembly.GetType(typeName);
        }

        public static Type GetType(AssemblyName assemblyName, string typeName)
        {
            var assembly = Assembly.Load(assemblyName);
            return assembly.GetType(typeName);
        }

        private static Type sceneHierarchyWindow;

        static public Type SceneHierarchyWindow
        {
            get
            {
                if (sceneHierarchyWindow == null)
                {
                    var assemblyName = new AssemblyName("UnityEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null");
                    sceneHierarchyWindow = GetType(assemblyName, "UnityEditor.SceneHierarchyWindow");
                }
                return sceneHierarchyWindow;
            }
        }

#if UNITY_2018_3_OR_NEWER
        private static Type sceneHierarchy;

        static public Type SceneHierarchy
        {
            get
            {
                if (sceneHierarchy == null)
                {
                    var assemblyName = new AssemblyName("UnityEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null");
                    sceneHierarchy = GetType(assemblyName, "UnityEditor.SceneHierarchy");
                }
                return sceneHierarchy;
            }
        }
#endif

        private static Type inspectorWindow;

        static public Type InspectorWindow
        {
            get
            {
                if (inspectorWindow == null)
                {
                    var assemblyName = new AssemblyName("UnityEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null");
                    inspectorWindow = GetType(assemblyName, "UnityEditor.InspectorWindow");
                }
                return inspectorWindow;
            }
        }

        private static Type monoRuntimeType;

        public static Type MonoRuntimeType
        {
            get
            {
                if (monoRuntimeType == null)
                {
                    monoRuntimeType = Type.GetType("Mono.Runtime");
                }
                return monoRuntimeType;
            }
        }

        public static bool HasGenericParameter(Type type)
        {
            if (type.IsGenericTypeDefinition) return true;
            if (type.IsGenericParameter) return true;
            if (type.IsByRef || type.IsArray)
            {
                return HasGenericParameter(type.GetElementType());
            }
            if (type.IsGenericType)
            {
                foreach (var typeArg in type.GetGenericArguments())
                {
                    if (HasGenericParameter(typeArg))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public static bool IsSameOrSubNamespace(Type type, string compareNamespace)
        {
            if (type.Namespace == null && string.IsNullOrEmpty(compareNamespace)) { return true; }
            return type.Namespace != null && (type.Namespace == compareNamespace || type.Namespace.StartsWith(compareNamespace + "."));
        }

        public static bool TypeHasRef(Type type, string refNamespace)
        {
            if (IsSameOrSubNamespace(type, refNamespace))
            {
                return true;
            }
            if (type.IsNested)
            {
                return TypeHasRef(type.DeclaringType, refNamespace);
            }
            if (type.IsByRef || type.IsArray)
            {
                return TypeHasRef(type.GetElementType(), refNamespace);
            }
            if (type.IsGenericType)
            {
                foreach (var typeArg in type.GetGenericArguments())
                {
                    if (TypeHasRef(typeArg, refNamespace))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

#if UNITY_EDITOR
        private static Type internalEditorUtilityType;

        public static Type InternalEditorUtilityType
        {
            get
            {
                if (internalEditorUtilityType == null)
                {
                    internalEditorUtilityType = AssemblyHelper.SceneView.GetType("UnityEditorInternal.InternalEditorUtility");
                }
                return internalEditorUtilityType;
            }
        }

        private static Type consoleWindowType;

        public static Type ConsoleWindowType
        {
            get
            {
                if (consoleWindowType == null)
                {
                    consoleWindowType = AssemblyHelper.EditorWindow.GetType("UnityEditor.ConsoleWindow");
                }
                return consoleWindowType;
            }
        }

        private static Type logEntriesType;

        public static Type LogEntriesType
        {
            get
            {
                if (logEntriesType == null)
                {
                    logEntriesType = AssemblyHelper.EditorWindow.GetType("UnityEditor.LogEntries");
                }
                return logEntriesType;
            }
        }

        private static Type logEntryType;

        public static Type LogEntryType
        {
            get
            {
                if (logEntryType == null)
                {
                    logEntryType = AssemblyHelper.EditorWindow.GetType("UnityEditor.LogEntry");
                }
                return logEntryType;
            }
        }

        private static Type scriptAttributeUtilityType;

        public static Type ScriptAttributeUtilityType
        {
            get
            {
                if (scriptAttributeUtilityType == null)
                {
                    scriptAttributeUtilityType = AssemblyHelper.UnityEditor.GetType("UnityEditor.ScriptAttributeUtility");
                }
                return scriptAttributeUtilityType;
            }
        }

        private static Type propertyHandlerType;

        public static Type PropertyHandlerType
        {
            get
            {
                if (propertyHandlerType == null)
                {
                    propertyHandlerType = AssemblyHelper.UnityEditor.GetType("UnityEditor.PropertyHandler");
                }
                return propertyHandlerType;
            }
        }

        private static Type propertyDrawerType;

        public static Type PropertyDrawerType
        {
            get
            {
                if (propertyDrawerType == null)
                {
                    propertyDrawerType = AssemblyHelper.UnityEditor.GetType("UnityEditor.PropertyDrawer");
                }
                return propertyDrawerType;
            }
        }

        private static Type decoratorDrawerType;

        public static Type DecoratorDrawerType
        {
            get
            {
                if (decoratorDrawerType == null)
                {
                    decoratorDrawerType = AssemblyHelper.UnityEditor.GetType("UnityEditor.DecoratorDrawer");
                }
                return decoratorDrawerType;
            }
        }

        private static Type localizationDatabaseType;

        public static Type LocalizationDatabaseType
        {
            get
            {
                if (localizationDatabaseType == null)
                {
                    localizationDatabaseType = AssemblyHelper.UnityEditor.GetType("UnityEditor.LocalizationDatabase");
                }
                return localizationDatabaseType;
            }
        }

        private static Type editorAssembliesType;

        public static Type EditorAssembliesType
        {
            get
            {
                if (editorAssembliesType == null)
                {
                    editorAssembliesType = AssemblyHelper.UnityEditor.GetType("UnityEditor.EditorAssemblies");
                }
                return editorAssembliesType;
            }
        }

        private static Type spriteUtilityType;

        public static Type SpriteUtilityType
        {
            get
            {
                if (spriteUtilityType == null)
                {
                    spriteUtilityType = AssemblyHelper.UnityEditor.GetType("UnityEditor.SpriteUtility");
                }
                return spriteUtilityType;
            }
        }

        private static Type guiSlideGroupType;

        public static Type GUISlideGroupType
        {
            get
            {
                if (guiSlideGroupType == null)
                {
                    guiSlideGroupType = AssemblyHelper.UnityEditor.GetType("UnityEditor.GUISlideGroup");
                }
                return guiSlideGroupType;
            }
        }

        private static Type eventCommandNamesType;

        public static Type EventCommandNamesType
        {
            get
            {
                if (eventCommandNamesType == null)
                {
                    eventCommandNamesType = AssemblyHelper.UnityEngine.GetType("UnityEngine.EventCommandNames");
                }
                return eventCommandNamesType;
            }
        }
#endif
    }
}
