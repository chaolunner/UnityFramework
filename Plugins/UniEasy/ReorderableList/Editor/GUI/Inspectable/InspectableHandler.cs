using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEditor;
using System;

namespace UniEasy.Editor
{
    public class InspectableHandler
    {
        private InspectableDrawer inspectableDrawer = null;

        private List<UnityEditor.DecoratorDrawer> DecoratorDrawers = null;
        public string Tooltip = null;

        public bool HasInspectableDrawer { get { return InspectableDrawer != null; } }
        public InspectableDrawer InspectableDrawer { get { return IsCurrentlyNested ? null : inspectableDrawer; } }

        public bool IsCurrentlyNested
        {
            get
            {
                return (inspectableDrawer != null
                    && InspectableAttributeUtility.s_DrawerStack.Count > 0
                    && inspectableDrawer == InspectableAttributeUtility.s_DrawerStack.Peek());
            }
        }

        public List<ContextMenuItemAttribute> ContextMenuItems = null;

        public bool Empty
        {
            get
            {
                return DecoratorDrawers == null
                    && Tooltip == null
                    && InspectableDrawer == null
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
            Type drawerType = InspectableAttributeUtility.GetDrawerTypeForType(drawnType);

            // If we found a drawer type, instantiate the drawer, cache it, and return it.
            if (drawerType != null)
            {
                if (typeof(InspectableDrawer).IsAssignableFrom(drawerType))
                {
                    // HACK: Here is different from the U3D setting, maybe is not a good idea!
                    // Unity only want use PropertyDrawer on array elements, not on array itself,
                    // But i don't like this setting, so i recode it.
                    inspectableDrawer = (InspectableDrawer)Activator.CreateInstance(drawerType);
                    inspectableDrawer.FieldInfo = field;

                    // Will be null by design if default type drawer!
                    inspectableDrawer.Attribute = attribute;
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
        public bool OnGUI(Rect position, InspectableProperty property, GUIContent label, bool includeChildren)
        {
            Rect visibleArea = new Rect(0, 0, position.width, float.MaxValue);
            return OnGUI(position, property, label, includeChildren, visibleArea);
        }

        public bool OnGUI(Rect position, InspectableProperty property, GUIContent label, bool includeChildren, Rect visibleArea)
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
            if (InspectableDrawer != null)
            {
                // Remember widths
                oldLabelWidth = EditorGUIUtility.labelWidth;
                oldFieldWidth = EditorGUIUtility.fieldWidth;
                // Draw with custom drawer
                InspectableDrawer.OnGUISafe(position, property.Copy(), label ?? EditorGUIUtilityHelper.TempContent(property.DisplayName));
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

                int relIndent = origIndent - property.Depth;

                InspectableProperty prop = property.Copy();

                position.height = EasyGUI.GetSinglePropertyHeight(prop, label);

                // First property with custom label
                EditorGUI.indentLevel = prop.Depth + relIndent;
                bool childrenAreExpanded = EasyGUI.DefaultPropertyField(position, prop, label) && EasyGUI.HasVisibleChildFields(prop);
                position.y += position.height + EasyGUI.kControlVerticalSpacing;

                // Loop through all child properties
                if (childrenAreExpanded)
                {
                    InspectableProperty endProperty = property.GetEndProperty();
                    while (prop.NextVisible(childrenAreExpanded) && !InspectableProperty.EqualContents(prop, endProperty))
                    {
                        var handler = InspectableAttributeUtility.GetHandler(prop);
                        EditorGUI.indentLevel = prop.Depth + relIndent;
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

        public float GetHeight(InspectableProperty property, GUIContent label, bool includeChildren)
        {
            float height = 0;

            if (DecoratorDrawers != null && !IsCurrentlyNested)
            {
                foreach (var drawer in DecoratorDrawers)
                {
                    height += drawer.GetHeight();
                }
            }

            if (InspectableDrawer != null)
            {
                height += InspectableDrawer.GetPropertyHeightSafe(property.Copy(), label ?? EditorGUIUtilityHelper.TempContent(property.DisplayName));
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
                bool childrenAreExpanded = property.IsExpanded && EasyGUI.HasVisibleChildFields(property);

                // Loop through all child properties
                if (childrenAreExpanded)
                {
                    InspectableProperty endProperty = property.GetEndProperty();
                    while (property.NextVisible(childrenAreExpanded) && !InspectableProperty.EqualContents(property, endProperty))
                    {
                        height += InspectableAttributeUtility.GetHandler(property).GetHeight(property, EditorGUIUtilityHelper.TempContent(property.DisplayName), true);
                        childrenAreExpanded = false;
                        height += EasyGUI.kControlVerticalSpacing;
                    }
                }
            }

            return height;
        }

        public void AddMenuItems(InspectableProperty property, GenericMenu menu)
        {
            if (ContextMenuItems == null)
            {
                return;
            }

            Type scriptType = property.InspectableObject.Object.GetType();
            foreach (ContextMenuItemAttribute attribute in ContextMenuItems)
            {
                MethodInfo method = scriptType.GetMethod(attribute.function, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                if (method == null)
                {
                    continue;
                }
                menu.AddItem(new GUIContent(attribute.name), false, () =>
                {
                    CallMenuCallback(method, property.InspectableObject.Object);
                    property.InspectableObject.UpdateIfRequiredOrScript();
                });
            }
        }

        public void CallMenuCallback(MethodInfo method, params object[] targets)
        {
            for (int i = 0; i < targets.Length; i++)
            {
                object obj = targets[i];
                method.Invoke(obj, new object[0]);
            }
        }
    }
}
