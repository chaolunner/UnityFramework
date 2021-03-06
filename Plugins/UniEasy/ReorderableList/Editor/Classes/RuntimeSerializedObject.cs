﻿using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace UniEasy.Editor
{
    public class RuntimeSerializedObject
    {
        #region Fields

        public object Target;
        public bool IsExpanded;
        public string Name;
        public string Type;
        public List<RuntimeNativeSerializedProperty> Properties;
        public List<RuntimeNativeSerializedProperty> VisibleProperties;

        private object owner;
        private string path;
        private int instanceID;
        private const string CombineNameAndID = "{0} [InstanceID : {1}]";

        #endregion

        #region Constructors

        public RuntimeSerializedObject(object obj, object target, int id)
        {
            var type = target.GetType();
            if (obj is RuntimeSerializedProperty)
            {
                var runtimeSerializedProperty = obj as RuntimeSerializedProperty;
                owner = runtimeSerializedProperty.RuntimeSerializedObject;
                path = runtimeSerializedProperty.PropertyPath;
            }
            else if (obj is SerializedProperty)
            {
                var serializedProperty = obj as SerializedProperty;
                owner = serializedProperty.serializedObject;
                path = serializedProperty.propertyPath;
            }
            else
            {
                Debug.LogError(obj + "it must be RuntimeSerializedProperty type or SerializedProperty type!");
            }
            instanceID = id;
            Target = target;
            Name = string.Format(CombineNameAndID, type.Name, instanceID);
            Type = type.ToString();
            Properties = new List<RuntimeNativeSerializedProperty>();
            VisibleProperties = new List<RuntimeNativeSerializedProperty>();

            var header = new RuntimeNativeSerializedProperty(this, null, typeof(System.Nullable), Name, Name, 0, null, null);
            Properties.Add(header);
            VisibleProperties.Add(header);

            var fields = type.GetAllInstanceFieldsFromCached();
            if (fields != null)
            {
                foreach (var field in fields)
                {
                    var property = new RuntimeNativeSerializedProperty(this, field, field.FieldType, field.Name, field.Name, 0, () =>
                    {
                        return field.GetValue(Target);
                    }, (val) =>
                    {
                        field.SetValue(Target, val);
                        HasModifiedProperties = true;
                    });
                    Properties.Add(property);
                    if (field.IsVisible())
                    {
                        VisibleProperties.Add(property);
                    }
                }
            }
        }

        #endregion

        #region Properties

        public bool HasModifiedProperties { get; protected set; }

        public int ChildCount
        {
            get
            {
                return Properties.Count;
            }
        }

        public int VisableChildCount
        {
            get
            {
                return VisibleProperties.Count;
            }
        }

        public SerializedObject SerializedObject
        {
            get
            {
                if (owner is RuntimeSerializedObject)
                {
                    return (owner as RuntimeSerializedObject).SerializedObject;
                }
                return owner as SerializedObject;
            }
            set
            {
                if (owner is RuntimeSerializedObject)
                {
                    (owner as RuntimeSerializedObject).SerializedObject = value;
                }
                else
                {
                    owner = value;
                }
            }
        }

        public Object TargetObject
        {
            get
            {
                return SerializedObject.targetObject;
            }
        }

        public object ParentProperty
        {
            get
            {
                if (owner is RuntimeSerializedObject)
                {
                    return (owner as RuntimeSerializedObject).FindProperty(path);
                }
                return SerializedObject.FindProperty(path);
            }
        }

        #endregion

        #region Methods

        public RuntimeSerializedProperty FindProperty(string propertyPath)
        {
            RuntimeSerializedProperty property = GetIterator();
            return property.FindProperty(propertyPath);
        }

        public RuntimeSerializedProperty GetIterator()
        {
            return new RuntimeSerializedProperty(Properties[0]);
        }

        public int GetInstanceID()
        {
            return instanceID;
        }

        public bool ApplyModifiedProperties()
        {
            if (!Application.isPlaying && HasModifiedProperties)
            {
                UpdateIfRequiredOrScript();
                return true;
            }
            return false;
        }

        public bool UpdateIfRequiredOrScript()
        {
            if (owner is RuntimeSerializedObject)
            {
                var parentProperty = ParentProperty as RuntimeSerializedProperty;
                parentProperty.StringValue = RuntimeObject.ToJson(Target);
                parentProperty.RuntimeSerializedObject.UpdateIfRequiredOrScript();
                Debug.Log("UpdateIfRequiredOrScript [InstanceID : " + parentProperty.RuntimeSerializedObject.GetInstanceID() + "]");
            }
            else if (owner is SerializedObject)
            {
                var parentProperty = ParentProperty as SerializedProperty;
                parentProperty.stringValue = RuntimeObject.ToJson(Target);
                EditorUtility.SetDirty(TargetObject);
                parentProperty.serializedObject.ApplyModifiedProperties();
                parentProperty.serializedObject.UpdateIfRequiredOrScript();
                Debug.Log("UpdateIfRequiredOrScript [InstanceID : " + parentProperty.serializedObject.GetHashCode() + "]");
            }
            HasModifiedProperties = false;
            return true;
        }

        public void ForceReloadProperties()
        {
            RuntimeSerializedProperty prop = GetIterator();
            while (prop.Next(true))
            {
                prop.Value = prop.Value;
            }
            UpdateIfRequiredOrScript();
        }

        #endregion
    }
}
