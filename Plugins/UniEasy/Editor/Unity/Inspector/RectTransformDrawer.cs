using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using System.Collections.Generic;

namespace UniEasy.Editor
{
    [CanEditMultipleObjects, CustomEditor(typeof(RectTransform), true)]
    public class RectTransformDrawer : DecoratorDrawer
    {
        Dictionary<string, SerializedProperty> SerializedProperties;

        public RectTransformDrawer() : base("RectTransformEditor")
        {
        }

        void OnEnable()
        {
            SerializedProperties = new Dictionary<string, SerializedProperty>();
            SerializedProperties.Add("m_AnchoredPosition", serializedObject.FindProperty("m_AnchoredPosition"));
            SerializedProperties.Add("m_LocalPosition", serializedObject.FindProperty("m_LocalPosition"));
            SerializedProperties.Add("m_SizeDelta", serializedObject.FindProperty("m_SizeDelta"));
            SerializedProperties.Add("m_AnchorMin", serializedObject.FindProperty("m_AnchorMin"));
            SerializedProperties.Add("m_AnchorMax", serializedObject.FindProperty("m_AnchorMax"));
            SerializedProperties.Add("m_Pivot", serializedObject.FindProperty("m_Pivot"));
            SerializedProperties.Add("m_LocalRotation", serializedObject.FindProperty("m_LocalRotation"));
            SerializedProperties.Add("m_LocalScale", serializedObject.FindProperty("m_LocalScale"));
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            serializedObject.Update();
            DrawCustomInspector();
            serializedObject.ApplyModifiedProperties();
        }

        void DrawCustomInspector()
        {
            GUILayoutOption option = GUILayout.MinWidth(82f);
            GUILayout.BeginHorizontal();
            {
                bool reset = GUILayout.Button("Reset", option);
                bool copy = GUILayout.Button("Copy", option);
                bool paste = GUILayout.Button("Paste", option);

                if (reset)
                {
                    SerializedProperties["m_AnchoredPosition"].vector2Value = Vector2.zero;
                    SerializedProperties["m_LocalPosition"].vector3Value = Vector3.zero;
                    SerializedProperties["m_SizeDelta"].vector2Value = Vector2.zero;
                    SerializedProperties["m_AnchorMin"].vector2Value = new Vector2(.5f, .5f);
                    SerializedProperties["m_AnchorMax"].vector2Value = new Vector2(.5f, .5f);
                    SerializedProperties["m_Pivot"].vector2Value = new Vector2(.5f, .5f);
                    SerializedProperties["m_LocalRotation"].quaternionValue = Quaternion.identity;
                    SerializedProperties["m_LocalScale"].vector3Value = Vector3.one;
                }
                if (copy)
                {
                    ComponentUtility.CopyComponent(serializedObject.targetObject as Component);
                }
                if (paste)
                {
                    foreach (var targetObject in serializedObject.targetObjects)
                    {
                        ComponentUtility.PasteComponentValues(targetObject as Component);
                    }
                }
            }
            GUILayout.EndHorizontal();
        }
    }
}
