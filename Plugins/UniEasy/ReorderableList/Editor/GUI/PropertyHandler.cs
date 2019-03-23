using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEditor;
using System;

namespace UniEasy.Editor
{
    public class PropertyHandler
    {
        private PropertyDrawer propertyDrawer = null;

        private List<UnityEditor.DecoratorDrawer> DecoratorDrawers = null;
        public string Tooltip = null;

        public bool HasPropertyDrawer { get { return PropertyDrawer != null; } }
        public PropertyDrawer PropertyDrawer { get { return IsCurrentlyNested ? null : propertyDrawer; } }

        public bool IsCurrentlyNested
        {
            get
            {
                return (propertyDrawer != null
                    && ScriptAttributeUtility.s_DrawerStack.Count > 0
                    && propertyDrawer == ScriptAttributeUtility.s_DrawerStack.Peek());
            }
        }

        public List<ContextMenuItemAttribute> ContextMenuItems = null;

        public bool Empty
        {
            get
            {
                return DecoratorDrawers == null
                    && Tooltip == null
                    && PropertyDrawer == null
                    && ContextMenuItems == null;
            }
        }

        public void HandleAttribute(PropertyAttribute attribute, FieldInfo field, Type propertyType)
        {
            if (attribute is TooltipAttribute)
            {
                Tooltip = (attribute as TooltipAttribute).tooltip;
                return;
            }
            else if (attribute is ContextMenuItemAttribute)
            {
                // Use context menu items on array elements, not on array itself
                if (propertyType.IsArrayOrList())
                {
                    return;
                }
                if (ContextMenuItems == null)
                {
                    ContextMenuItems = new List<ContextMenuItemAttribute>();
                }
                ContextMenuItems.Add(attribute as ContextMenuItemAttribute);
                return;
            }
            // Look for its drawer type of this attribute
            HandleDrawnType(attribute.GetType(), propertyType, field, attribute);
        }

        public void HandleDrawnType(Type drawnType, Type propertyType, FieldInfo field, PropertyAttribute attribute)
        {
            Type drawerType = ScriptAttributeUtility.GetDrawerTypeForType(drawnType);

            // If we found a drawer type, instantiate the drawer, cache it, and return it.
            if (drawerType != null)
            {
                if (typeof(PropertyDrawer).IsAssignableFrom(drawerType))
                {
                    // HACK: Here is different from the U3D setting, maybe is not a good idea!
                    // Unity only want use PropertyDrawer on array elements, not on array itself,
                    // But i don't like this setting, so i recode it.
                    propertyDrawer = (PropertyDrawer)Activator.CreateInstance(drawerType);
                    propertyDrawer.SetFieldInfo(field);

                    // Will be null by design if default type drawer!
                    propertyDrawer.SetAttribute(attribute);
                }
                else if (typeof(UnityEditor.DecoratorDrawer).IsAssignableFrom(drawerType))
                {
                    // Draw decorators on array itself, not on each array elements
                    if (field != null && field.FieldType.IsArrayOrList() && !propertyType.IsArrayOrList())
                    {
                        return;
                    }
                    UnityEditor.DecoratorDrawer decoratorDrawer = (UnityEditor.DecoratorDrawer)System.Activator.CreateInstance(drawerType);
                    DecoratorDrawerHelper.SetAttribute(decoratorDrawer, attribute);

                    if (DecoratorDrawers == null)
                    {
                        DecoratorDrawers = new List<UnityEditor.DecoratorDrawer>();
                    }
                    DecoratorDrawers.Add(decoratorDrawer);
                }
            }
        }

        // returns true if children needs to be drawn separately
        public bool OnGUI(Rect position, SerializedProperty property, GUIContent label, bool includeChildren)
        {
            Rect visibleArea = new Rect(0, 0, position.width, float.MaxValue);
            return OnGUI(position, property, label, includeChildren, visibleArea);
        }

        public bool OnGUI(Rect position, SerializedProperty property, GUIContent label, bool includeChildren, Rect visibleArea)
        {
            float oldLabelWidth, oldFieldWidth;

            float propHeight = position.height;
            position.height = 0;
            if (DecoratorDrawers != null && !IsCurrentlyNested)
            {
                foreach (var decorator in DecoratorDrawers)
                {
                    position.height = decorator.GetHeight();

                    oldLabelWidth = EditorGUIUtility.labelWidth;
                    oldFieldWidth = EditorGUIUtility.fieldWidth;
                    decorator.OnGUI(position);
                    EditorGUIUtility.labelWidth = oldLabelWidth;
                    EditorGUIUtility.fieldWidth = oldFieldWidth;

                    position.y += position.height;
                    propHeight -= position.height;
                }
            }

            position.height = propHeight;
            if (PropertyDrawer != null)
            {
                // Remember widths
                oldLabelWidth = EditorGUIUtility.labelWidth;
                oldFieldWidth = EditorGUIUtility.fieldWidth;
                // Draw with custom drawer
                PropertyDrawer.OnGUISafe(position, property.Copy(), label ?? EditorGUIUtilityHelper.TempContent(property.displayName));
                // Restore widths
                EditorGUIUtility.labelWidth = oldLabelWidth;
                EditorGUIUtility.fieldWidth = oldFieldWidth;

                return false;
            }
            else
            {
                if (!includeChildren)
                {
                    return EasyGUI.DefaultPropertyField(position, property, label);
                }
                // Remember state
                Vector2 oldIconSize = EditorGUIUtility.GetIconSize();
                bool wasEnabled = GUI.enabled;
                int origIndent = EditorGUI.indentLevel;

                int relIndent = origIndent - property.depth;

                SerializedProperty prop = property.Copy();

                position.height = EasyGUI.GetSinglePropertyHeight(prop, label);

                // First property with custom label
                EditorGUI.indentLevel = prop.depth + relIndent;
                bool childrenAreExpanded = EasyGUI.DefaultPropertyField(position, prop, label) && EasyGUI.HasVisibleChildFields(prop);
                position.y += position.height + EasyGUI.kControlVerticalSpacing;

                // Loop through all child properties
                if (childrenAreExpanded)
                {
                    SerializedProperty endProperty = prop.GetEndProperty();
                    while (prop.NextVisible(childrenAreExpanded) && !SerializedProperty.EqualContents(prop, endProperty))
                    {
                        var handler = ScriptAttributeUtility.GetHandler(prop);
                        EditorGUI.indentLevel = prop.depth + relIndent;
                        position.height = handler.GetHeight(prop, null, false);

                        if (position.Overlaps(visibleArea))
                        {
                            EditorGUI.BeginChangeCheck();
                            childrenAreExpanded = handler.OnGUI(position, prop, null, false) && EasyGUI.HasVisibleChildFields(prop);
                            // Changing child properties (like array size) may invalidate the iterator,
                            // so stop now, or we may get errors.
                            if (EditorGUI.EndChangeCheck())
                            {
                                break;
                            }
                        }

                        position.y += position.height + EasyGUI.kControlVerticalSpacing;
                    }
                }

                // Restore state
                GUI.enabled = wasEnabled;
                EditorGUIUtility.SetIconSize(oldIconSize);
                EditorGUI.indentLevel = origIndent;

                return false;
            }
        }

        public float GetHeight(SerializedProperty property, GUIContent label, bool includeChildren)
        {
            float height = 0f;

            if (DecoratorDrawers != null && !IsCurrentlyNested)
            {
                foreach (var drawer in DecoratorDrawers)
                {
                    height += drawer.GetHeight();
                }
            }

            if (PropertyDrawer != null)
            {
                height += PropertyDrawer.GetPropertyHeightSafe(property.Copy(), label ?? EditorGUIUtilityHelper.TempContent(property.displayName));
            }
            else if (!includeChildren)
            {
                height += EasyGUI.GetSinglePropertyHeight(property, label);
            }
            else
            {
                property = property.Copy();

                // First property with custom label
                height += EasyGUI.GetSinglePropertyHeight(property, label);
                bool childrenAreExpanded = property.isExpanded && EasyGUI.HasVisibleChildFields(property);

                // Loop through all child properties
                var tc = EditorGUIUtilityHelper.TempContent(property.displayName);
                if (childrenAreExpanded)
                {
                    SerializedProperty endProperty = property.GetEndProperty();
                    while (property.NextVisible(childrenAreExpanded) && !SerializedProperty.EqualContents(property, endProperty))
                    {
                        height += ScriptAttributeUtility.GetHandler(property).GetHeight(property, tc, true);
                        childrenAreExpanded = false;
                        height += EasyGUI.kControlVerticalSpacing;
                    }
                }
            }

            return height;
        }

        public void AddMenuItems(SerializedProperty property, GenericMenu menu)
        {
            if (ContextMenuItems == null)
            {
                return;
            }

            Type scriptType = property.serializedObject.targetObject.GetType();
            foreach (ContextMenuItemAttribute attribute in ContextMenuItems)
            {
                MethodInfo method = scriptType.GetMethod(attribute.function, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                if (method == null)
                {
                    continue;
                }
                menu.AddItem(new GUIContent(attribute.name), false, () => CallMenuCallback(property.serializedObject.targetObjects, method));
            }
        }

        public void CallMenuCallback(object[] targets, MethodInfo method)
        {
            foreach (object target in targets)
            {
                method.Invoke(target, new object[] { });
            }
        }

    }
}
