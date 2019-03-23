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

        private static int MaxWidth = 100;
        private static int DelaySaveSceneFrameCount = 2;
        private static string PreLoadingSystems = "Pre Loading Systems";
        private static string SetAutoUpdate = "Set Auto Update {0}";
        private static string ForceUpdate = "Force Update";
        private static string AutoUpdate = "Auto Update";

        [InitializeOnLoadMethod]
        static void StaticInitialize()
        {
            EditorSceneManager.sceneSaved += OnSceneSaved;
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

        static void OnSceneSaved(Scene scene)
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
