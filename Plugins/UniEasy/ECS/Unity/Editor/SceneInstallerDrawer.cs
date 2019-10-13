using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using UnityEditor;
using UnityEngine;
using UniEasy.ECS;

namespace UniEasy.Editor
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(SceneInstaller), true)]
    public class SceneInstallerDrawer : ReorderableListDrawer
    {
        private SceneInstaller installer;
        private bool autoUpdate;

        private const int MaxWidth = 100;
        private const int DelaySaveSceneFrameCount = 2;
        private const string PreLoadingSystems = "Pre Loading Systems";
        private const string SetAutoUpdate = "Set Auto Update {0}";
        private const string ForceUpdate = "Force Update";
        private const string AutoUpdate = "Auto Update";

        [InitializeOnLoadMethod]
        static void StaticInitialize()
        {
            EditorSceneManager.sceneSaving += OnSceneSaving;
        }

        public override void OnEnable()
        {
            base.OnEnable();

            installer = target as SceneInstaller;
        }

        protected override void DrawInspector()
        {
            base.DrawInspector();

            EditorGUILayout.BeginHorizontal();
            EditorGUI.BeginChangeCheck();
            autoUpdate = EditorGUILayout.ToggleLeft(AutoUpdate, installer.AutoUpdate, GUILayout.MaxWidth(MaxWidth));
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(installer, string.Format(SetAutoUpdate, autoUpdate));
                installer.AutoUpdate = autoUpdate;
                EditorUtility.SetDirty(installer);
            }
            if (GUILayout.Button(ForceUpdate))
            {
                Undo.RecordObject(installer, PreLoadingSystems);
                installer.PreLoadingSystems();
                EditorUtility.SetDirty(installer);
            }
            EditorGUILayout.EndHorizontal();
        }

        static void OnSceneSaving(Scene scene, string path)
        {
            foreach (var root in scene.GetRootGameObjects())
            {
                var installer = root.GetComponent<SceneInstaller>();
                if (installer != null && installer.AutoUpdate)
                {
                    Undo.RecordObject(installer, PreLoadingSystems);
                    installer.PreLoadingSystems();
                    EditorUtility.SetDirty(installer);
                }
            }
            // I don't want to save scene twice, so i need to wait for scene detected dirty and save scene again
            DelayFrame(() =>
            {
                if (scene.isDirty)
                {
                    EditorSceneManager.SaveScene(scene);
                }
            }, DelaySaveSceneFrameCount);
        }
    }
}
