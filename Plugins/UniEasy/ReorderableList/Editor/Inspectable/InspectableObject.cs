using UnityEditor.SceneManagement;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEditor;

namespace UniEasy.Editor
{
    public class InspectableObject
    {
        #region Fields

        public object Object;
        public bool IsExpanded;
        public string Name;
        public string Path;
        public string Type;
        public SerializedObject SerializedObject;
        public List<InspectablePropertyBase> Properties;
        public List<InspectablePropertyBase> VisibleProperties;

        private int instanceID;
        private static readonly Dictionary<int, InspectableObject> inspectableObjectIndex = new Dictionary<int, InspectableObject>();

        #endregion

        #region Constructors

        public InspectableObject(SerializedProperty serializedProperty, object o)
        {
            instanceID = GetHashCode(serializedProperty, o);
            Object = o;
            Name = o.GetType().Name;
            Type = o.GetType().ToString();
            Path = serializedProperty.propertyPath;
            SerializedObject = serializedProperty.serializedObject;
            Properties = new List<InspectablePropertyBase>();
            VisibleProperties = new List<InspectablePropertyBase>();

            var header = new InspectablePropertyBase(this, null, typeof(System.Nullable), Name, Name, 0, null, null);
            Properties.Add(header);
            VisibleProperties.Add(header);

            var fields = Object.GetType().GetAllInstanceFieldsFromCached();
            if (fields != null)
            {
                foreach (var field in fields)
                {
                    var property = new InspectablePropertyBase(this, field, field.FieldType, field.Name, field.Name, 0, () =>
                    {
                        return field.GetValue(Object);
                    }, (val) =>
                    {
                        field.SetValue(Object, val);
                        HasModifiedProperties = true;
                    });
                    Properties.Add(property);
                    if (field.IsVisible())
                    {
                        VisibleProperties.Add(property);
                    }
                }
            }

            inspectableObjectIndex.Add(instanceID, this);
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

        public SerializedProperty ParentProperty
        {
            get
            {
                return SerializedObject.FindProperty(Path);
            }
        }

        #endregion

        #region Methods

        public InspectableProperty FindProperty(string propertyPath)
        {
            InspectableProperty property = GetIterator();
            return property.FindProperty(propertyPath);
        }

        public InspectableProperty GetIterator()
        {
            return new InspectableProperty(Properties[0]);
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
            ParentProperty.SetInspectableObjectData(JsonUtility.ToJson(Object));
            ParentProperty.serializedObject.ApplyModifiedProperties();
            EditorSceneManager.MarkAllScenesDirty();
            HasModifiedProperties = false;
            return result;
        }

        #endregion

        #region Static Methods

        public static int GetHashCode(SerializedProperty serializedProperty, object o)
        {
            return serializedProperty.serializedObject.targetObject.GetInstanceID() ^ o.GetHashCode();
        }

        public static InspectableObject LoadFromCache(int instanceID)
        {
            if (inspectableObjectIndex.ContainsKey(instanceID))
            {
                return inspectableObjectIndex[instanceID];
            }
            return null;
        }

        public static InspectableObject CreateInstance(SerializedProperty serializedProperty)
        {
            var data = serializedProperty.GetInspectableObjectData();
            return CreateInstance(serializedProperty, data);
        }

        public static InspectableObject CreateInstance(SerializedProperty serializedProperty, InspectableObjectData data)
        {
            if (data != null)
            {
                var o = data.CreateInstance();
                if (o != null)
                {
                    var inspectableObject = LoadFromCache(GetHashCode(serializedProperty, o));
                    if (inspectableObject == null)
                    {
                        inspectableObject = new InspectableObject(serializedProperty, o);
                    }
                    inspectableObject.SerializedObject = serializedProperty.serializedObject;
                    return inspectableObject;
                }
            }
            return null;
        }

        #endregion
    }
}
