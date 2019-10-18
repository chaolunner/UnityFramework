using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace UniEasy.Editor
{
    public class TargetChoiceHandler
    {
        #region Static Fields

        private static Dictionary<string, RuntimeSerializedObject> Clipboard = new Dictionary<string, RuntimeSerializedObject>();
        private const string StringType = "string";

        #endregion

        #region Static Methods

        public static void RevertPrefabPropertyOverride(object userData)
        {
            SerializedProperty parentProperty = userData as SerializedProperty;
#if UNITY_2018_2_OR_NEWER
            Object prefab = PrefabUtility.GetCorrespondingObjectFromSource(parentProperty.serializedObject.targetObject);
#else
            Object prefab = PrefabUtility.GetPrefabParent(parentProperty.serializedObject.targetObject);
#endif
            SerializedObject serializedObject = new SerializedObject(prefab);
            SerializedProperty prop = serializedObject.FindProperty(parentProperty.propertyPath);

            parentProperty.arraySize = prop.arraySize;
            for (int i = 0; i < parentProperty.arraySize; i++)
            {
                var obj1 = RuntimeSerializedObjectCache.GetRuntimeSerializedObject(parentProperty.GetArrayElementAtIndex(i));
                var obj2 = RuntimeSerializedObjectCache.GetRuntimeSerializedObject(prop.GetArrayElementAtIndex(i), RuntimeObject.FromJson(prop.GetArrayElementAtIndex(i).stringValue));
                obj1.Target = obj2.Target;
                obj1.ForceReloadProperties();
            }

            EditorUtilityHelper.ForceReloadInspectors();
        }

        public static void CopyAllComponents(object userData)
        {
            SerializedProperty serializedProperty = userData as SerializedProperty;
            if (serializedProperty != null && serializedProperty.isArray && serializedProperty.arrayElementType == StringType)
            {
                Clipboard.Clear();
                for (int i = 0; i < serializedProperty.arraySize; i++)
                {
                    var obj = RuntimeSerializedObjectCache.GetRuntimeSerializedObject(serializedProperty.GetArrayElementAtIndex(i));

                    if (Clipboard.ContainsKey(obj.Type))
                    {
                        Clipboard[obj.Type] = obj;
                    }
                    else
                    {
                        Clipboard.Add(obj.Type, obj);
                    }
                }
            }
        }

        public static bool CanPaste(SerializedProperty property)
        {
            return (property != null && property.isArray && property.arrayElementType == StringType && Clipboard.Count > 0);
        }

        public static void PasteAllComponents(object userData)
        {
            SerializedProperty serializedProperty = userData as SerializedProperty;
            if (CanPaste(serializedProperty))
            {
                List<string> pastedTypes = new List<string>();
                for (int i = 0; i < serializedProperty.arraySize; i++)
                {
                    var obj = RuntimeSerializedObjectCache.GetRuntimeSerializedObject(serializedProperty.GetArrayElementAtIndex(i));

                    if (Clipboard.ContainsKey(obj.Type))
                    {
                        obj.Target = Clipboard[obj.Type].Target;
                        obj.ForceReloadProperties();
                        pastedTypes.Add(obj.Type);
                    }
                }

                foreach (var kvp in Clipboard)
                {
                    if (pastedTypes.Contains(kvp.Key)) { continue; }
                    serializedProperty.arraySize++;
                    var prop = serializedProperty.GetArrayElementAtIndex(serializedProperty.arraySize - 1);
                    var obj = RuntimeSerializedObjectCache.GetRuntimeSerializedObject(prop);
                    obj.Target = Clipboard[obj.Type].Target;
                    obj.ForceReloadProperties();
                }

                EditorUtilityHelper.ForceReloadInspectors();
            }
        }

        #endregion
    }
}