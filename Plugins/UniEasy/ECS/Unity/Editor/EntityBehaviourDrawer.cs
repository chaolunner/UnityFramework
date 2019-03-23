using UnityEditor;
using UnityEngine;
using UniEasy.ECS;

namespace UniEasy.Editor
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(EntityBehaviour), true)]
    public class EntityBehaviourDrawer : ReorderableListDrawer
    {
        private EntityBehaviour entityBehaviour;
        private SerializedObject entityObject;
        private SerializedProperty componentProperty;
        private int componentsCount;
        private bool autoUpdate;

        private static int MaxWidth = 100;
        private static string PreLoadingComponents = "Pre Loading Components";
        private static string SetAutoUpdate = "Set Auto Update {0}";
        private static string ForceUpdate = "Force Update";
        private static string AutoUpdate = "Auto Update";
        private static string M_Component = "m_Component";

        public override void OnEnable()
        {
            base.OnEnable();

            entityBehaviour = target as EntityBehaviour;
            entityObject = new SerializedObject(entityBehaviour.gameObject);
            if (entityObject != null)
            {
                componentProperty = entityObject.FindProperty(M_Component);
            }
        }

        protected override void DrawInspector()
        {
            base.DrawInspector();

            EditorGUILayout.BeginHorizontal();
            EditorGUI.BeginChangeCheck();
            autoUpdate = EditorGUILayout.ToggleLeft(AutoUpdate, entityBehaviour.AutoUpdate, GUILayout.MaxWidth(MaxWidth));
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(entityBehaviour, string.Format(SetAutoUpdate, autoUpdate));
                entityBehaviour.AutoUpdate = autoUpdate;
                EditorUtility.SetDirty(entityBehaviour);
            }
            if (GUILayout.Button(ForceUpdate))
            {
                Undo.RecordObject(entityBehaviour, PreLoadingComponents);
                entityBehaviour.PreLoadingComponents();
                EditorUtility.SetDirty(entityBehaviour);
            }
            EditorGUILayout.EndHorizontal();

            if (entityBehaviour.AutoUpdate && componentProperty != null)
            {
                if (componentsCount != componentProperty.arraySize)
                {
                    Undo.RecordObject(entityBehaviour, PreLoadingComponents);
                    entityBehaviour.PreLoadingComponents();
                    EditorUtility.SetDirty(entityBehaviour);
                    componentsCount = componentProperty.arraySize;
                }
            }
        }
    }
}
