using System.Collections.Generic;
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
        public string Path;
        public string Type;
        public SerializedObject Owner;
        public List<RuntimeNativeSerializedProperty> Properties;
        public List<RuntimeNativeSerializedProperty> VisibleProperties;

        private int instanceID;

        #endregion

        #region Constructors

        public RuntimeSerializedObject(SerializedProperty serializedProperty, object target, int id)
        {
            var type = target.GetType();
            instanceID = id;
            Target = target;
            Name = type.Name;
            Type = type.ToString();
            Path = serializedProperty.propertyPath;
            Owner = serializedProperty.serializedObject;
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

        public SerializedProperty OwnerProperty
        {
            get
            {
                return Owner.FindProperty(Path);
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
            bool result = HasModifiedProperties;
            OwnerProperty.stringValue = RuntimeObject.ToJson(Target);
            EditorUtility.SetDirty(Owner.targetObject);
            Owner.ApplyModifiedProperties();
            Owner.UpdateIfRequiredOrScript();
            HasModifiedProperties = false;
            return result;
        }

        public void ForceReloadProperties()
        {
            RuntimeSerializedProperty prop = GetIterator();
            while (prop.Next(true))
            {
                prop.Value = prop.Value;
            }
            ApplyModifiedProperties();
        }

        #endregion
    }
}
