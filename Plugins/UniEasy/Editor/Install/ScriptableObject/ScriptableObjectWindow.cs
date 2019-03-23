using System.Collections.Generic;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using UnityEditor;
using System.Linq;
using System;

namespace UniEasy.Editor
{
    /// <summary>
    /// Create ScriptableObject Window.
    /// </summary>
    public class ScriptableObjectWindow : EditorWindow
    {
        private int selectedNamespaceIndex;
        private int selectedTypeIndex;
        private string searchCondition = "";
        private Type currentSelectedType;
        private UnityEngine.Object currentSelectedObject;
        private SearchField searchField;

        private static string NoneStr = "None";
        private static List<Type> ScriptableTypes = new List<Type>();
        private static Dictionary<string, Type[]> ScriptableDictionary = new Dictionary<string, Type[]>();

        [MenuItem("Window/UniEasy/ScriptableObject Window")]
        [MenuItem("Assets/Create/Custom Script/ScriptableObject Window", false, 62)]
        public static void OpenScriptableObjectWindow()
        {
            Initialize();

            var window = GetWindow<ScriptableObjectWindow>(false, "Scriptable", true);
            window.ShowPopup();
        }

        static void Initialize()
        {
            // Get all classes derived from ScriptableObject
            if (AssemblyHelper.CSharp != null)
            {
                ScriptableTypes.AddRange(AssemblyHelper.CSharp.GetTypes().Where(t => t.IsSubclassOf(typeof(ScriptableObject)) && !t.IsGenericType).Distinct());
            }
            if (AssemblyHelper.CSharpFirstpass != null)
            {
                ScriptableTypes.AddRange(AssemblyHelper.CSharpFirstpass.GetTypes().Where(t => t.IsSubclassOf(typeof(ScriptableObject)) && !t.IsGenericType).Distinct());
            }

            ScriptableDictionary = ScriptableTypes.Select(t => t.Namespace ?? NoneStr).Distinct().ToDictionary(k => k, v => ScriptableTypes.Where(t => (t.Namespace ?? NoneStr) == v).ToArray());
        }

        void OnEnable()
        {
            Initialize();

            if (searchField == null) { searchField = new SearchField(); }
        }

        public void OnGUI()
        {
            GUILayout.Label("Create a ScriptableObject Asset");
            searchCondition = searchField.OnGUI(EditorGUILayout.GetControlRect(), searchCondition);
            // Select depend on namespace
            string[] keys;
            if (!string.IsNullOrEmpty(searchCondition))
            {
                var key0 = ScriptableTypes.Where(t => t.FullName.Length >= searchCondition.Length && t.FullName.StartsWith(searchCondition, true, null)).Select(t => t.Namespace ?? NoneStr);
                var key1 = ScriptableTypes.Where(t => !string.IsNullOrEmpty(t.Namespace) && t.Namespace.Length >= searchCondition.Length && t.Namespace.StartsWith(searchCondition, true, null)).Select(t => t.Namespace ?? NoneStr);
                var key2 = ScriptableTypes.Where(t => t.Name.Length >= searchCondition.Length && t.Name.StartsWith(searchCondition, true, null)).Select(t => t.Namespace ?? NoneStr);
                keys = key0.Union(key1).Union(key2).Distinct().ToArray();
            }
            else
            {
                keys = ScriptableTypes.Select(t => t.Namespace ?? NoneStr).Distinct().ToArray();
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
                var types = ScriptableDictionary[currentKey];
                var name0 = types.Where(t => t.FullName.Length >= searchCondition.Length && t.FullName.StartsWith(searchCondition, true, null)).Select(v => v.Name);
                var name1 = types.Where(t => t.Name.Length >= searchCondition.Length && t.Name.StartsWith(searchCondition, true, null)).Select(v => v.Name);
                names = name0.Union(name1).ToArray();
            }
            else
            {
                names = ScriptableDictionary[currentKey].Select(t => t.Name).ToArray();
            }
            // If can not search any scriptableobject Name
            if (names == null || names.Length <= 0)
            {
                return;
            }

            selectedTypeIndex = Mathf.Clamp(selectedTypeIndex, 0, names.Length - 1);
            selectedTypeIndex = EditorGUILayout.Popup(selectedTypeIndex, names);

            var type = ScriptableDictionary[currentKey].Where(t => t.Name == names[selectedTypeIndex]).FirstOrDefault();
            var isDirty = currentSelectedType == type ? false : true;

            if (isDirty)
            {
                var guids = AssetDatabase.FindAssets("t:Object");
                var objs = guids.Select(guid => AssetDatabase.LoadAssetAtPath<MonoScript>(AssetDatabase.GUIDToAssetPath(guid))).Where(o => o != null);
                currentSelectedObject = objs.Where(o => o != null && o.GetClass() == type).Select(o => o as UnityEngine.Object).FirstOrDefault();
                currentSelectedType = type;
            }
            EditorGUILayout.ObjectField(currentSelectedObject, typeof(MonoScript), false);

            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Create"))
            {
                ScriptableObjectUtility.CreateAssetWithSavePrompt(names[selectedTypeIndex], type);
            }
        }
    }
}
