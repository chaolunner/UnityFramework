using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using System;

namespace UniEasy.Editor
{
    public class RuntimePropertyHandler
    {
        #region Fields

        private RuntimePropertyDrawer runtimePropertyDrawer = null;

        private List<UnityEditor.DecoratorDrawer> DecoratorDrawers = null;
        public string Tooltip = null;

        #endregion

        #region Properties

        public bool HasRuntimePropertyDrawer { get { return RuntimePropertyDrawer != null; } }
        public RuntimePropertyDrawer RuntimePropertyDrawer { get { return IsCurrentlyNested ? null : runtimePropertyDrawer; } }

        public bool IsCurrentlyNested
        {
            get
            {
                return (runtimePropertyDrawer != null
                    && RuntimeScriptAttributeUtility.s_DrawerStack.Count > 0
                    && runtimePropertyDrawer == RuntimeScriptAttributeUtility.s_DrawerStack.Peek());
            }
        }

        public List<ContextMenuItemAttribute> ContextMenuItems = null;

        public bool Empty
        {
            get
            {
                return DecoratorDrawers == null
                    && Tooltip == null
                    && RuntimePropertyDrawer == null
                    && ContextMenuItems == null;
            }
        }

        #endregion

        #region Methods

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
            Type drawerType = RuntimeScriptAttributeUtility.GetDrawerTypeForType(drawnType);

            // If we found a drawer type, instantiate the drawer, cache it, and return it.
            if (drawerType != null)
            {
                if (typeof(RuntimePropertyDrawer).IsAssignableFrom(drawerType))
                {
                    // HACK: Here is different from the U3D setting, maybe is not a good idea!
                    // Unity only want use PropertyDrawer on array elements, not on array itself,
                    // But i don't like this setting, so i recode it.
                    runtimePropertyDrawer = (RuntimePropertyDrawer)Activator.CreateInstance(drawerType);
                    runtimePropertyDrawer.FieldInfo = field;

                    // Will be null by design if default type drawer!
                    runtimePropertyDrawer.Attribute = attribute;
                }
                else if (typeof(UnityEditor.DecoratorDrawer).IsAssignableFrom(drawerType))
                {
                    // Draw decorators on array itself, not on each array elements
                    if (field != null && field.FieldType.IsArrayOrList() && !propertyType.IsArrayOrList())
                    {
                        return;
                    }
                    UnityEditor.DecoratorDrawer decoratorDrawer = (UnityEditor.DecoratorDrawer)Activator.CreateInstance(drawerType);
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
        public bool OnGUI(Rect position, RuntimeSerializedProperty property, GUIContent label, bool includeChildren)
        {
            Rect visibleArea = new Rect(0, 0, position.width, float.MaxValue);
            return OnGUI(position, property, label, includeChildren, visibleArea);
        }

        public bool OnGUI(Rect position, RuntimeSerializedProperty property, GUIContent label, bool includeChildren, Rect visibleArea)
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
            if (RuntimePropertyDrawer != null)
            {
                // Remember widths
                oldLabelWidth = EditorGUIUtility.labelWidth;
                oldFieldWidth = EditorGUIUtility.fieldWidth;
                // Draw with custom drawer
                RuntimePropertyDrawer.OnGUISafe(position, property, label ?? EditorGUIUtilityHelper.TempContent(property.DisplayName));
                // Restore widths
                EditorGUIUtility.labelWidth = oldLabelWidth;
                EditorGUIUtility.fieldWidth = oldFieldWidth;

                return false;
            }
            else
            {
                if (!includeChildren)
                {
                    return RuntimeEasyGUI.DefaultPropertyField(position, property, label);
                }
                // Remember state
                Vector2 oldIconSize = EditorGUIUtility.GetIconSize();
                bool wasEnabled = GUI.enabled;
                int origIndent = EditorGUI.indentLevel;

                int relIndent = origIndent - property.Depth;

                RuntimeSerializedProperty prop = property.Copy();

                position.height = RuntimeEasyGUI.GetSinglePropertyHeight(property, label);

                // First property with custom label
                EditorGUI.indentLevel = property.Depth + relIndent;
                bool childrenAreExpanded = RuntimeEasyGUI.DefaultPropertyField(position, property, label) && RuntimeEasyGUI.HasVisibleChildFields(property);
                position.y += position.height + EasyGUI.kControlVerticalSpacing;

                // Loop through all child properties
                if (childrenAreExpanded)
                {
                    RuntimeSerializedProperty endProperty = property.GetEndProperty();
                    while (prop.NextVisible(childrenAreExpanded) && !RuntimeSerializedProperty.EqualContents(prop, endProperty))
                    {
                        var handler = RuntimeScriptAttributeUtility.GetHandler(prop);
                        EditorGUI.indentLevel = prop.Depth + relIndent;
                        position.height = handler.GetHeight(prop, null, false);

                        if (position.Overlaps(visibleArea))
                        {
                            EditorGUI.BeginChangeCheck();
                            childrenAreExpanded = handler.OnGUI(position, prop, null, false) && RuntimeEasyGUI.HasVisibleChildFields(prop);
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

        public float GetHeight(RuntimeSerializedProperty property, GUIContent label, bool includeChildren)
        {
            float height = 0;

            if (DecoratorDrawers != null && !IsCurrentlyNested)
            {
                foreach (var drawer in DecoratorDrawers)
                {
                    height += drawer.GetHeight();
                }
            }

            if (RuntimePropertyDrawer != null)
            {
                height += RuntimePropertyDrawer.GetPropertyHeightSafe(property, label ?? EditorGUIUtilityHelper.TempContent(property.DisplayName));
            }
            else if (!includeChildren)
            {
                height += RuntimeEasyGUI.GetSinglePropertyHeight(property, label);
            }
            else
            {
                RuntimeSerializedProperty prop = property.Copy();

                // First property with custom label
                height += RuntimeEasyGUI.GetSinglePropertyHeight(prop, label);
                bool childrenAreExpanded = prop.IsExpanded && RuntimeEasyGUI.HasVisibleChildFields(prop);

                // Loop through all child properties
                if (childrenAreExpanded)
                {
                    RuntimeSerializedProperty endProperty = prop.GetEndProperty();
                    while (prop.NextVisible(childrenAreExpanded) && !RuntimeSerializedProperty.EqualContents(prop, endProperty))
                    {
                        height += RuntimeScriptAttributeUtility.GetHandler(prop).GetHeight(prop, EditorGUIUtilityHelper.TempContent(prop.DisplayName), true);
                        childrenAreExpanded = false;
                        height += EasyGUI.kControlVerticalSpacing;
                    }
                }
            }

            return height;
        }

        public void AddMenuItems(RuntimeSerializedProperty property, GenericMenu menu)
        {
            if (ContextMenuItems == null)
            {
                return;
            }

            Type scriptType = property.RuntimeSerializedObject.Target.GetType();
            foreach (ContextMenuItemAttribute attribute in ContextMenuItems)
            {
                MethodInfo method = scriptType.GetMethod(attribute.function, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                if (method == null)
                {
                    continue;
                }
                menu.AddItem(new GUIContent(attribute.name), false, () =>
                {
                    CallMenuCallback(method, property.RuntimeSerializedObject.Target);
                });
            }
        }

        public void CallMenuCallback(MethodInfo method, params object[] targets)
        {
            for (int i = 0; i < targets.Length; i++)
            {
                object obj = targets[i];
                method.Invoke(obj, new object[] { });
            }
        }

        #endregion
    }
}
