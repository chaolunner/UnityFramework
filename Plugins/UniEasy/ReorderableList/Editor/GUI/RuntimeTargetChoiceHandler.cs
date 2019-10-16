using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace UniEasy.Editor
{
    public class RuntimeTargetChoiceHandler
    {
        #region Static Fields

        private static Dictionary<string, RuntimeSerializedObject> Clipboard = new Dictionary<string, RuntimeSerializedObject>();
        private static string Data;

        private const string NullableStr = "System.Nullable";

        #endregion

        #region Static Methods

        public static void DuplicateArrayElement(object userData)
        {
            RuntimeSerializedProperty runtimeSerializedProperty = userData as RuntimeSerializedProperty;
            runtimeSerializedProperty.DuplicateCommand();
            EditorUtilityHelper.ForceReloadInspectors();
        }

        public static void DeleteArrayElement(object userData)
        {
            RuntimeSerializedProperty runtimeSerializedProperty = userData as RuntimeSerializedProperty;
            runtimeSerializedProperty.DeleteCommand();
            EditorUtilityHelper.ForceReloadInspectors();
        }

        public static void RevertPrefabPropertyOverride(object userData)
        {
            RuntimeSerializedProperty runtimeSerializedProperty = userData as RuntimeSerializedProperty;
            SerializedProperty serializedProperty = runtimeSerializedProperty.RuntimeSerializedObject.OwnerProperty;
            int index;
            var parentProperty = serializedProperty.GetBelongArrayAndIndex(out index);
            if (index >= 0)
            {
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
        }

        public static void CopyComponent(object userData)
        {
            RuntimeSerializedProperty runtimeSerializedProperty = userData as RuntimeSerializedProperty;
            var type = runtimeSerializedProperty.RuntimeSerializedObject.Type;
            Data = RuntimeObject.ToJson(runtimeSerializedProperty.RuntimeSerializedObject.Target);
            if (Clipboard.ContainsKey(type))
            {
                Clipboard[type] = runtimeSerializedProperty.RuntimeSerializedObject;
            }
            else
            {
                Clipboard.Add(type, runtimeSerializedProperty.RuntimeSerializedObject);
            }
        }

        public static bool CanPasteAsNew(RuntimeSerializedProperty property)
        {
            return property != null && !string.IsNullOrEmpty(Data);
        }

        public static bool CanPaste(RuntimeSerializedProperty property)
        {
            return property != null && Clipboard.ContainsKey(property.RuntimeSerializedObject.Type);
        }

        public static void PasteComponentAsNew(object userData)
        {
            RuntimeSerializedProperty runtimeSerializedProperty = userData as RuntimeSerializedProperty;
            SerializedProperty serializedProperty = runtimeSerializedProperty.RuntimeSerializedObject.OwnerProperty;
            int index;
            var parentProperty = serializedProperty.GetBelongArrayAndIndex(out index);
            if (index >= 0)
            {
                parentProperty.arraySize++;
                parentProperty.GetArrayElementAtIndex(parentProperty.arraySize - 1).stringValue = Data;
                parentProperty.serializedObject.ApplyModifiedProperties();
            }
            EditorUtilityHelper.ForceReloadInspectors();
        }

        public static void PasteComponentValues(object userData)
        {
            RuntimeSerializedProperty runtimeSerializedProperty = userData as RuntimeSerializedProperty;

            runtimeSerializedProperty.RuntimeSerializedObject.Target = Clipboard[runtimeSerializedProperty.RuntimeSerializedObject.Type].Target;
            runtimeSerializedProperty.RuntimeSerializedObject.ForceReloadProperties();
            EditorUtilityHelper.ForceReloadInspectors();
        }

        #endregion
    }
}
