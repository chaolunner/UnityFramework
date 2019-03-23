using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace UniEasy.Editor
{
    public class TargetChoiceHandler
    {
        private static Dictionary<string, InspectableObjectData> clipboard = new Dictionary<string, InspectableObjectData>();
        private static List<InspectableObjectData> datas = new List<InspectableObjectData>();

        private static string NullableStr = "System.Nullable";

        public static void DuplicateArrayElement(object userData)
        {
            InspectableProperty inspectableProperty = (InspectableProperty)userData;
            inspectableProperty.DuplicateCommand();
        }

        public static void DeleteArrayElement(object userData)
        {
            InspectableProperty inspectableProperty = (InspectableProperty)userData;
            inspectableProperty.DeleteCommand();
        }

        public static void SetPrefabOverride(object userData)
        {
            InspectableProperty inspectableProperty = userData as InspectableProperty;
            if (inspectableProperty != null && inspectableProperty.Type != NullableStr)
            {
#if UNITY_2018_2_OR_NEWER
                Object prefab = PrefabUtility.GetCorrespondingObjectFromSource(inspectableProperty.InspectableObject.SerializedObject.targetObject);
#else
                Object prefab = PrefabUtility.GetPrefabParent(inspectableProperty.InspectableObject.SerializedObject.targetObject);
#endif
                SerializedObject serializedObject = new SerializedObject(prefab);
                SerializedProperty serializedProperty = serializedObject.FindProperty(inspectableProperty.InspectableObject.Path);
                InspectableObject inspectableObject = InspectableObject.CreateInstance(serializedProperty);
                inspectableProperty.Value = inspectableObject.FindProperty(inspectableProperty.PropertyPath).Value;
                inspectableProperty.InspectableObject.ApplyModifiedProperties();
            }
            else
            {
                InspectableObject inspectableObject = inspectableProperty == null ? userData as InspectableObject : inspectableProperty.InspectableObject;
                if (inspectableObject != null)
                {
                    InspectableProperty prop = inspectableObject.GetIterator();
                    while (prop.Next(true))
                    {
                        SetPrefabOverride(prop);
                    }
                }
                else
                {
                    SerializedProperty serializedProperty = userData as SerializedProperty;
                    if (serializedProperty != null && serializedProperty.IsInspectableObjectDataArrayOrList())
                    {
                        for (int i = 0; i < serializedProperty.arraySize; i++)
                        {
                            SerializedProperty prop = serializedProperty.GetArrayElementAtIndex(i);
                            if (prop.IsInspectableObjectData())
                            {
                                SetPrefabOverride(InspectableObject.CreateInstance(prop));
                            }
                        }
                    }
                }
            }
        }

        public static void CopyComponent(object userData)
        {
            InspectableProperty inspectableProperty = (InspectableProperty)userData;
            datas.Clear();
            datas.Add(inspectableProperty.InspectableObject.ParentProperty.GetInspectableObjectData());
            if (clipboard.ContainsKey(datas[0].Type))
            {
                clipboard[datas[0].Type] = datas[0];
            }
            else
            {
                clipboard.Add(datas[0].Type, datas[0]);
            }
        }

        public static bool CanPasteAsNew(object obj)
        {
            return obj != null && datas.Count > 0 && !string.IsNullOrEmpty(datas[0].Type) && !string.IsNullOrEmpty(datas[0].Data);
        }

        public static bool CanPaste(InspectableProperty property)
        {
            return clipboard.ContainsKey(property.InspectableObject.Type);
        }

        public static void PasteComponentAsNew(object userData)
        {
            InspectableObject inspectableObject = userData as InspectableObject;
            SerializedProperty serializedProperty = null;
            if (inspectableObject == null)
            {
                serializedProperty = userData as SerializedProperty;
            }
            else
            {
                serializedProperty = inspectableObject.SerializedObject.GetIterator();
                serializedProperty.NextVisible(true);
            }
            do
            {
                if (serializedProperty.IsInspectableObjectDataArrayOrList())
                {
                    serializedProperty.arraySize++;
                    serializedProperty.GetArrayElementAtIndex(serializedProperty.arraySize - 1).SetInspectableObjectData(datas[0]);
                    break;
                }
            } while (serializedProperty.NextVisible(false));
            serializedProperty.serializedObject.ApplyModifiedProperties();
        }

        public static void PasteComponentValues(object userData)
        {
            InspectableProperty inspectableProperty = (InspectableProperty)userData;
            InspectableObjectData data = clipboard[inspectableProperty.InspectableObject.Type];
            JsonUtility.FromJsonOverwrite(data.Data, inspectableProperty.InspectableObject.Object);
            inspectableProperty.InspectableObject.ParentProperty.SetInspectableObjectData(data);
            inspectableProperty.InspectableObject.SerializedObject.ApplyModifiedProperties();
        }

        public static void CopyAllComponents(object userData)
        {
            SerializedProperty serializedProperty = (SerializedProperty)userData;
            if (serializedProperty != null && serializedProperty.IsInspectableObjectDataArrayOrList())
            {
                for (int i = 0; i < serializedProperty.arraySize; i++)
                {
                    datas.Add(serializedProperty.GetArrayElementAtIndex(i).GetInspectableObjectData());
                }
            }
        }

        public static bool CanPaste(SerializedProperty property)
        {
            return (property != null && property.IsInspectableObjectDataArrayOrList() && datas.Count > 0);
        }

        public static void PasteAllComponents(object userData)
        {
            SerializedProperty serializedProperty = (SerializedProperty)userData;
            if (serializedProperty != null && serializedProperty.IsInspectableObjectDataArrayOrList())
            {
                while (serializedProperty.arraySize < datas.Count)
                {
                    serializedProperty.arraySize++;
                }
                for (int i = 0; i < datas.Count; i++)
                {
                    serializedProperty.GetArrayElementAtIndex(i).SetInspectableObjectData(datas[i]);
                }
                serializedProperty.serializedObject.ApplyModifiedProperties();
            }
        }
    }
}
