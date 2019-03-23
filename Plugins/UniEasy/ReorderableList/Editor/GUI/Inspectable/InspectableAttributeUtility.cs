using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEditor;
using System.Linq;
using System;

namespace UniEasy.Editor
{
    public class InspectableAttributeUtility
    {
        private struct DrawerKeySet
        {
            public Type Drawer;
            public Type Type;
        }

        // Internal API members
        internal static Stack<InspectableDrawer> s_DrawerStack = new Stack<InspectableDrawer>();
        private static Dictionary<Type, DrawerKeySet> s_DrawerTypeForType = null;
        private static Dictionary<string, List<PropertyAttribute>> s_BuiltinAttributes = null;
        private static InspectableHandler s_SharedNullHandler = new InspectableHandler();
        private static InspectableHandler s_NextHandler = new InspectableHandler();

        private static InspectableHandlerCache s_GlobalCache = new InspectableHandlerCache();
        private static InspectableHandlerCache s_CurrentCache = null;

        public static InspectableHandlerCache InspectableHandlerCache
        {
            get
            {
                return s_CurrentCache ?? s_GlobalCache;
            }
            set
            { s_CurrentCache = value; }
        }

        public static void ClearGlobalCache()
        {
            s_GlobalCache.Clear();
        }

        private static void PopulateBuiltinAttributes()
        {
            s_BuiltinAttributes = new Dictionary<string, List<PropertyAttribute>>();

            AddBuiltinAttribute("GUIText", "m_Text", new MultilineAttribute());
            AddBuiltinAttribute("TextMesh", "m_Text", new MultilineAttribute());
            // Example: Make Orthographic Size in Camera component be in range between 0 and 1000
            //AddBuiltinAttribute ("Camera", "m_OrthographicSize", new RangeAttribute (0, 1000));
        }

        private static void AddBuiltinAttribute(string componentTypeName, string propertyPath, PropertyAttribute attr)
        {
            string key = componentTypeName + "_" + propertyPath;
            if (!s_BuiltinAttributes.ContainsKey(key))
            {
                s_BuiltinAttributes.Add(key, new List<PropertyAttribute>());
            }
            s_BuiltinAttributes[key].Add(attr);
        }

        private static List<PropertyAttribute> GetBuiltinAttributes(InspectableProperty property)
        {
            if (property.InspectableObject.SerializedObject.targetObject == null)
            {
                return null;
            }
            Type t = property.InspectableObject.SerializedObject.targetObject.GetType();
            if (t == null)
            {
                return null;
            }
            string attrKey = t.Name + "_" + property.PropertyPath;
            List<PropertyAttribute> attr = null;
            s_BuiltinAttributes.TryGetValue(attrKey, out attr);
            return attr;
        }

        // Called on demand
        public static void BuildDrawerTypeForTypeDictionary()
        {
            s_DrawerTypeForType = new Dictionary<Type, DrawerKeySet>();

            var loadedTypes = AppDomain.CurrentDomain.GetAssemblies().SelectMany(x => AssemblyHelper.GetTypesFromAssembly(x)).ToArray();

            foreach (Type type in EditorAssembliesHelper.SubclassesOf(typeof(GUIDrawer)))
            {
                object[] attrs = type.GetCustomAttributes(typeof(InspectablePropertyDrawer), true);
                foreach (InspectablePropertyDrawer editor in attrs)
                {
                    //Debug.Log("Base type: " + editor.Type);
                    if (!(s_DrawerTypeForType.ContainsKey(editor.Type) && typeof(PropertyDrawer).IsAssignableFrom(type) && typeof(InspectableDrawer).IsAssignableFrom(s_DrawerTypeForType[editor.Type].Drawer)))
                    {
                        s_DrawerTypeForType[editor.Type] = new DrawerKeySet()
                        {
                            Drawer = type,
                            Type = editor.Type
                        };
                    }

                    if (!editor.UseForChildren)
                    {
                        continue;
                    }

                    var candidateTypes = loadedTypes.Where(x => x.IsSubclassOf(editor.Type));
                    foreach (var candidateType in candidateTypes)
                    {
                        //Debug.Log("Candidate Type: "+ candidateType);
                        if (s_DrawerTypeForType.ContainsKey(candidateType)
                            && (editor.Type.IsAssignableFrom(s_DrawerTypeForType[candidateType].Type)))
                        {
                            //  Debug.Log("skipping");
                            continue;
                        }

                        //Debug.Log("Setting");
                        s_DrawerTypeForType[candidateType] = new DrawerKeySet()
                        {
                            Drawer = type,
                            Type = editor.Type
                        };
                    }
                }

                attrs = type.GetCustomAttributes(typeof(CustomPropertyDrawer), true);
                foreach (CustomPropertyDrawer editor in attrs)
                {
                    var editorType = CustomPropertyDrawerHelper.GetType(editor);
                    bool useForChildren = CustomPropertyDrawerHelper.UseForChildren(editor);
                    //Debug.Log("Base type: " + editorType);
                    if (!(s_DrawerTypeForType.ContainsKey(editorType) && typeof(PropertyDrawer).IsAssignableFrom(type) && typeof(InspectableDrawer).IsAssignableFrom(s_DrawerTypeForType[editorType].Drawer)))
                    {
                        s_DrawerTypeForType[editorType] = new DrawerKeySet()
                        {
                            Drawer = type,
                            Type = editorType
                        };
                    }

                    if (!useForChildren)
                    {
                        continue;
                    }

                    var candidateTypes = loadedTypes.Where(x => x.IsSubclassOf(editorType));
                    foreach (var candidateType in candidateTypes)
                    {
                        //Debug.Log("Candidate Type: "+ candidateType);
                        if (s_DrawerTypeForType.ContainsKey(candidateType)
                            && (editorType.IsAssignableFrom(s_DrawerTypeForType[candidateType].Type)))
                        {
                            //  Debug.Log("skipping");
                            continue;
                        }

                        //Debug.Log("Setting");
                        s_DrawerTypeForType[candidateType] = new DrawerKeySet()
                        {
                            Drawer = type,
                            Type = editorType
                        };
                    }
                }
            }
        }

        public static Type GetDrawerTypeForType(Type type)
        {
            if (s_DrawerTypeForType == null)
            {
                BuildDrawerTypeForTypeDictionary();
            }

            DrawerKeySet drawerType;
            s_DrawerTypeForType.TryGetValue(type, out drawerType);
            if (drawerType.Drawer != null)
            {
                return drawerType.Drawer;
            }

            // now check for base generic versions of the drawers...
            if (type.IsGenericType)
            {
                s_DrawerTypeForType.TryGetValue(type.GetGenericTypeDefinition(), out drawerType);
            }
            return drawerType.Drawer;
        }

        private static List<PropertyAttribute> GetFieldAttributes(FieldInfo field)
        {
            if (field == null)
            {
                return null;
            }

            object[] attrs = field.GetCustomAttributes(typeof(PropertyAttribute), true);
            if (attrs != null && attrs.Length > 0)
            {
                return new List<PropertyAttribute>(attrs.Select(e => e as PropertyAttribute).OrderBy(e => -e.order));
            }
            return null;
        }

        private static FieldInfo GetFieldInfoFromProperty(InspectableProperty property, out Type type)
        {
            var classType = GetScriptTypeFromProperty(property);
            if (classType == null)
            {
                type = null;
                return null;
            }

            return GetFieldInfoFromPropertyPath(classType, property.PropertyPath, out type);
        }

        private static Type GetScriptTypeFromProperty(InspectableProperty property)
        {
            SerializedProperty scriptProp = property.InspectableObject.SerializedObject.FindProperty("m_Script");

            if (property.InspectableObject != null && property.InspectableObject.Object != null)
            {
                return property.InspectableObject.Object.GetType();
            }

            if (scriptProp == null)
            {
                return null;
            }

            MonoScript script = scriptProp.objectReferenceValue as MonoScript;

            if (script == null)
            {
                return null;
            }

            return script.GetClass();
        }

        private static FieldInfo GetFieldInfoFromPropertyPath(Type host, string path, out Type type)
        {
            FieldInfo field = null;
            type = host;
            string[] parts = path.Split('.');
            for (int i = 0; i < parts.Length; i++)
            {
                string member = parts[i];

                // *Here is different from the ScriptAttributeUtility
                if (i < parts.Length - 1 && type.IsArrayOrList())
                {
                    type = type.GetArrayOrListElementType();
                    continue;
                }

                // GetField on class A will not find private fields in base classes to A,
                // so we have to iterate through the base classes and look there too.
                // Private fields are relevant because they can still be shown in the Inspector,
                // and that applies to private fields in base classes too.
                FieldInfo foundField = null;
                for (Type currentType = type; foundField == null && currentType != null; currentType = currentType.BaseType)
                {
                    foundField = currentType.GetField(member, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                }

                if (foundField == null)
                {
                    type = null;
                    return null;
                }

                field = foundField;
                type = field.FieldType;
            }
            return field;
        }

        public static InspectableHandler GetHandler(InspectableProperty property)
        {
            if (property == null)
            {
                return s_SharedNullHandler;
            }

            // Don't use custom drawers in debug mode
            if (property.InspectableObject.SerializedObject.InspectorMode() != InspectorMode.Normal)
            {
                return s_SharedNullHandler;
            }

            // If the drawer is cached, use the cached drawer
            InspectableHandler handler = InspectableHandlerCache.GetHandler(property);
            if (handler != null)
            {
                return handler;
            }

            Type propertyType = null;
            List<PropertyAttribute> attributes = null;
            FieldInfo field = null;

            // Determine if SerializedObject target is a script or a builtin type
            UnityEngine.Object targetObject = property.InspectableObject.SerializedObject.targetObject;
            if (targetObject is MonoBehaviour || targetObject is ScriptableObject)
            {
                // For scripts, use reflection to get FieldInfo for the member the property represents
                field = GetFieldInfoFromProperty(property, out propertyType);

                // Use reflection to see if this member has an attribute
                attributes = GetFieldAttributes(field);
            }
            else
            {
                // For builtin types, look if we hardcoded an attribute for this property
                // First initialize the hardcoded properties if not already done
                if (s_BuiltinAttributes == null)
                {
                    PopulateBuiltinAttributes();
                }

                if (attributes == null)
                {
                    attributes = GetBuiltinAttributes(property);
                }
            }

            handler = s_NextHandler;

            if (attributes != null)
            {
                for (int i = attributes.Count - 1; i >= 0; i--)
                {
                    handler.HandleAttribute(attributes[i], field, propertyType);
                }
            }

            // Field has no CustomPropertyDrawer attribute with matching drawer so look for default drawer for field type
            if (!handler.HasInspectableDrawer && propertyType != null)
            {
                handler.HandleDrawnType(propertyType, propertyType, field, null);
            }

            if (handler.Empty)
            {
                InspectableHandlerCache.SetHandler(property, s_SharedNullHandler);
                handler = s_SharedNullHandler;
            }
            else
            {
                InspectableHandlerCache.SetHandler(property, handler);
                s_NextHandler = new InspectableHandler();
            }

            return handler;
        }
    }
}
