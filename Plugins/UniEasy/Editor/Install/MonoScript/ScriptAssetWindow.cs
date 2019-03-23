using System.Collections.Generic;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using UnityEditor;
using System.Linq;
using System;

namespace UniEasy.Editor
{
    /// <summary>
    /// Create a MonoScript Window.
    /// </summary>
    public class ScriptAssetWindow : EditorWindow
    {
        private int selectedTypeIndex = 0;
        private int selectedNamespaceIndex = 0;
        private string searchCondition = "";
        private Vector2 scrollPos = Vector2.zero;
        private Type currentSelectedType;
        private UnityEngine.Object currentSelectedObject;
        private SearchField searchField;

        private static string NoneStr = "None";
        private static List<Type> MonoScriptTypes = new List<Type>();
        private static Dictionary<string, Type[]> MonoScriptDictionary = new Dictionary<string, Type[]>();
        private static Dictionary<Type, IScriptAssetInstaller> Installers = new Dictionary<Type, IScriptAssetInstaller>();

        [MenuItem("Window/UniEasy/MonoScript Template Window")]
        [MenuItem("Assets/Create/Custom Script/MonoScript Template Window", false, 61)]
        static public void OpenScriptAssetWindow()
        {
            Initialize();

            var window = GetWindow<ScriptAssetWindow>(false, "MonoScript", true);
            window.ShowPopup();
        }

        static void Initialize()
        {
            // Get all classes derived from ScriptAssetInstallerBase
            if (AssemblyHelper.CSharpEditor != null)
            {
                MonoScriptTypes.AddRange(AssemblyHelper.CSharpEditor.GetTypes().Where(t => t.IsSubclassOf(typeof(ScriptAssetInstallerBase)) && !t.IsGenericType));
            }
            if (AssemblyHelper.CSharpEditorFirstpass != null)
            {
                MonoScriptTypes.AddRange(AssemblyHelper.CSharpEditorFirstpass.GetTypes().Where(t => t.IsSubclassOf(typeof(ScriptAssetInstallerBase)) && !t.IsGenericType));
            }

            MonoScriptDictionary = MonoScriptTypes.Select(t => t.Namespace ?? NoneStr).Distinct().ToDictionary(k => k, v => MonoScriptTypes.Where(t => (t.Namespace ?? NoneStr) == v).ToArray());
        }

        void OnEnable()
        {
            Initialize();

            if (searchField == null) { searchField = new SearchField(); }
        }

        public void OnGUI()
        {
            GUILayout.Label("Select a Template to Create the Script");
            searchCondition = searchField.OnGUI(EditorGUILayout.GetControlRect(), searchCondition);
            // Select depend on namespace
            string[] keys;
            if (!string.IsNullOrEmpty(searchCondition))
            {
                var key0 = MonoScriptTypes.Where(t => t.FullName.Length >= searchCondition.Length && t.FullName.StartsWith(searchCondition, true, null)).Select(t => t.Namespace ?? NoneStr);
                var key1 = MonoScriptTypes.Where(t => t.Namespace.Length >= searchCondition.Length && t.Namespace.StartsWith(searchCondition, true, null)).Select(t => t.Namespace ?? NoneStr);
                var key2 = MonoScriptTypes.Where(t => t.Name.Length >= searchCondition.Length && t.Name.StartsWith(searchCondition, true, null)).Select(t => t.Namespace ?? NoneStr);
                keys = key0.Union(key1).Union(key2).Distinct().ToArray();
            }
            else
            {
                keys = MonoScriptTypes.Select(t => t.Namespace ?? NoneStr).Distinct().ToArray();
            }

            // If can not search any scriptableobject Namespace
            if (keys == null || keys.Length <= 0)
            {
                return;
            }

            selectedNamespaceIndex = Mathf.Clamp(selectedNamespaceIndex, 0, keys.Length - 1);
            selectedNamespaceIndex = EditorGUILayout.Popup(selectedNamespaceIndex, keys);
            var currentKey = keys[selectedNamespaceIndex];

            // Select scriptableobject class
            string[] names;
            if (!string.IsNullOrEmpty(searchCondition))
            {
                var types = MonoScriptDictionary[currentKey];
                var name0 = types.Where(t => t.FullName.Length >= searchCondition.Length && t.FullName.StartsWith(searchCondition, true, null)).Select(v => v.Name);
                var name1 = types.Where(t => t.Name.Length >= searchCondition.Length && t.Name.StartsWith(searchCondition, true, null)).Select(v => v.Name);
                names = name0.Union(name1).ToArray();
            }
            else
            {
                names = MonoScriptDictionary[currentKey].Select(t => t.Name).ToArray();
            }
            // If can not search any scriptableobject Name
            if (names == null || names.Length <= 0)
            {
                return;
            }

            selectedTypeIndex = Mathf.Clamp(selectedTypeIndex, 0, names.Length - 1);
            selectedTypeIndex = EditorGUILayout.Popup(selectedTypeIndex, names);

            var type = MonoScriptDictionary[currentKey].Where(t => t.Name == names[selectedTypeIndex]).FirstOrDefault();
            var isDirty = currentSelectedType == type ? false : true;

            if (isDirty)
            {
                var guids = AssetDatabase.FindAssets("t:Object");
                var objs = guids.Select(guid => AssetDatabase.LoadAssetAtPath<MonoScript>(AssetDatabase.GUIDToAssetPath(guid))).Where(o => o != null);
                currentSelectedObject = objs.Where(o => o != null && o.GetClass() == type).Select(o => o as UnityEngine.Object).FirstOrDefault();
                currentSelectedType = type;
            }

            IScriptAssetInstaller installer = null;
            if (!Installers.TryGetValue(type, out installer))
            {
                installer = (IScriptAssetInstaller)Activator.CreateInstance(type);
                Installers.Add(type, installer);
            }
            if (installer == null)
            {
                return;
            }

            scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
            EditorGUILayout.ObjectField(currentSelectedObject, typeof(MonoScript), false);
            EditorGUILayout.TextArea(installer.GetContents());
            EditorGUILayout.EndScrollView();

            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Create"))
            {
                installer.Create();
            }
        }
    }
}
