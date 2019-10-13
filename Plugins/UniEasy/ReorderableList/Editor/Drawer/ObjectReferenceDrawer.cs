using UnityEditor.SceneManagement;
using UnityEditor;
using UnityEngine;

namespace UniEasy.Editor
{
    [CustomPropertyDrawer(typeof(ObjectReferenceAttribute))]
    public class ObjectReferenceDrawer : PropertyDrawer
    {
        private const string M_ScriptStr = "m_Script";

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (TryDrawObjectReference(position, property, label))
            {
            }
            else
            {
                EditorGUI.PropertyField(position, property, label);
            }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return GetObjectReferenceHeight(property, label);
        }

        public static bool TryDrawObjectReference(Rect position, SerializedProperty property, GUIContent label)
        {
            if (property != null && property.propertyType == SerializedPropertyType.ObjectReference)
            {
                position.height = EditorGUI.GetPropertyHeight(property, label, false);
                if (TryDrawObjectReference(position, property.objectReferenceValue, EasyGUI.CreateCachedEditorWithContext<ReorderableListDrawer>(property.objectReferenceValue, property.serializedObject.targetObject)))
                {
                    EditorGUI.PropertyField(position, property, label, false);
                    return true;
                }
            }
            return false;
        }

        private static bool TryDrawObjectReference(Rect position, Object o, ReorderableListDrawer drawer = null)
        {
            var result = false;
            if (o != null)
            {
                var objectData = o.GetSerializedObjectData();
                var headerPosition = new Rect(position);

                headerPosition.yMin = headerPosition.yMax - EditorGUIUtility.singleLineHeight;

                if (drawer != null)
                {
                    drawer.IgnoreHeader = true;
                    if (objectData.Foldout)
                    {
                        drawer.serializedObject.Update();
                        var listPosition = new Rect(headerPosition);
                        listPosition.xMin += 15;
                        listPosition.y += EditorGUIUtility.singleLineHeight;
                        drawer.DrawPropertiesAll(listPosition);
                        drawer.serializedObject.ApplyModifiedProperties();
                    }
                    result = true;
                }
                else
                {
                    var iterProp = objectData.Object.GetIterator();

                    position.yMin = headerPosition.yMax;
                    position.height = 0f;
                    EditorGUI.BeginChangeCheck();
                    if (iterProp.NextVisible(true))
                    {
                        EditorGUI.indentLevel++;
                        int depth = iterProp.depth;
                        do
                        {
                            if (depth != iterProp.depth)
                            {
                                break;
                            }
                            if (iterProp.name.Equals(M_ScriptStr))
                            {
                                continue;
                            }
                            if (objectData.Foldout)
                            {
                                var displayName = new GUIContent(iterProp.displayName);
                                position.yMin += position.height;
                                position.height = EasyGUI.GetPropertyHeight(iterProp, null, displayName, iterProp.isExpanded);
                                EasyGUI.PropertyField(position, iterProp, displayName, iterProp.isExpanded, null);
                            }
                            result = true;
                        } while (iterProp.NextVisible(false));
                        EditorGUI.indentLevel--;
                    }
                    if (EditorGUI.EndChangeCheck())
                    {
                        objectData.Object.ApplyModifiedProperties();
                        EditorSceneManager.MarkAllScenesDirty();
                    }
                }

                if (result)
                {
                    var indentLevel = EditorGUI.indentLevel;
                    headerPosition.xMin += EasyGUI.Indent;
                    headerPosition.width = 5;
                    EditorGUI.indentLevel = 0;
                    objectData.Foldout = EditorGUI.Foldout(headerPosition, objectData.Foldout, GUIContent.none, false);
                    EditorGUI.indentLevel = indentLevel;
                }
            }
            return result;
        }

        public static float GetObjectReferenceHeight(SerializedProperty property, GUIContent label)
        {
            if (property != null && property.propertyType == SerializedPropertyType.ObjectReference)
            {
                var headerHeight = EditorGUI.GetPropertyHeight(property, label, false) - EditorGUIUtility.singleLineHeight;
                return GetObjectReferenceHeight(property.objectReferenceValue, EasyGUI.CreateCachedEditorWithContext<ReorderableListDrawer>(property.objectReferenceValue, property.serializedObject.targetObject)) + headerHeight;
            }
            return EditorGUI.GetPropertyHeight(property, label, false);
        }

        public static float GetObjectReferenceHeight(Object o, ReorderableListDrawer drawer = null)
        {
            var height = EditorGUIUtility.singleLineHeight + 3f;
            if (o != null)
            {
                var objectData = o.GetSerializedObjectData();
                if (drawer != null)
                {
                    drawer.IgnoreHeader = true;
                    if (objectData.Foldout)
                    {
                        height += drawer.GetPropertiesAllHeights();
                    }
                }
                else
                {
                    var iterProp = objectData.Object.GetIterator();

                    if (iterProp.NextVisible(true))
                    {
                        int depth = iterProp.depth;
                        do
                        {
                            if (depth != iterProp.depth)
                            {
                                break;
                            }
                            if (iterProp.name.Equals(M_ScriptStr))
                            {
                                continue;
                            }
                            if (objectData.Foldout)
                            {
                                height += EasyGUI.GetPropertyHeight(iterProp, null, new GUIContent(iterProp.displayName), iterProp.isExpanded);
                            }
                        } while (iterProp.NextVisible(false));
                    }
                }
            }
            return height;
        }
    }
}
