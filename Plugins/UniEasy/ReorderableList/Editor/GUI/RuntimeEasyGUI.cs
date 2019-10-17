using UnityEditor.SceneManagement;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEditor;
using System.Linq;

namespace UniEasy.Editor
{
    public class RuntimeEasyGUI
    {
        #region Static Fields

        private static int s_GenericField = "s_GenericField".GetHashCode();
        private static string s_ArrayMultiInfoFormatString = EditorGUIUtilityHelper.TextContent("This field cannot display arrays with more than {0} elements when multiple objects are selected.").text;
        private static GUIContent s_PropertyFieldTempContent = new GUIContent();
        private static GUIContent s_ArrayMultiInfoContent = new GUIContent();
        private static RuntimeSerializedProperty s_PendingPropertyKeyboardHandling = null;
        private static RuntimeSerializedProperty s_PendingPropertyDelete = null;
        private static Stack<RuntimePropertyGUIData> s_PropertyStack = new Stack<RuntimePropertyGUIData>();
        private static Dictionary<ContextMenu, MethodInfo> contextMenuIndex = new Dictionary<ContextMenu, MethodInfo>();
        private static Dictionary<string, bool> contextMenuResultIndex = new Dictionary<string, bool>();
        private static Dictionary<string, bool> easyContextMenuResultIndex = new Dictionary<string, bool>();
        private static Dictionary<Object, Dictionary<Object, UnityEditor.Editor>> editableIndex = new Dictionary<Object, Dictionary<Object, UnityEditor.Editor>>();

        public const float kSingleLineHeight = 16;
        public const string EmptyStr = "";

        #endregion

        #region Static Methods

        public static void ClearStacks()
        {
            RuntimeScriptAttributeUtility.s_DrawerStack.Clear();
        }

        public static bool HasVisibleChildFields(RuntimeSerializedProperty property)
        {
            if (property.PropertyType == RuntimeSerializedPropertyType.Color ||
                property.PropertyType == RuntimeSerializedPropertyType.Vector2 ||
                property.PropertyType == RuntimeSerializedPropertyType.Vector3 ||
                property.PropertyType == RuntimeSerializedPropertyType.Vector4 ||
                property.PropertyType == RuntimeSerializedPropertyType.Vector2Int ||
                property.PropertyType == RuntimeSerializedPropertyType.Vector3Int ||
                property.PropertyType == RuntimeSerializedPropertyType.Rect ||
                property.PropertyType == RuntimeSerializedPropertyType.Bounds ||
                property.PropertyType == RuntimeSerializedPropertyType.RectInt ||
                property.PropertyType == RuntimeSerializedPropertyType.BoundsInt ||
                property.PropertyType.IsSameOrSubclassOf(RuntimeSerializedPropertyType.Object))
            {
                return false;
            }
            return property.HasVisibleChildren;
        }

        public static bool PropertyField(Rect position, RuntimeSerializedProperty property, List<PropertyAttribute> attributes)
        {
            return PropertyField(position, property, null, false, attributes);
        }

        public static bool PropertyField(Rect position, RuntimeSerializedProperty property, GUIContent label, List<PropertyAttribute> attributes)
        {
            return PropertyField(position, property, label, false, attributes);
        }

        public static bool PropertyField(Rect position, RuntimeSerializedProperty property, GUIContent label, bool includeChildren, List<PropertyAttribute> attributes)
        {
            return PropertyFieldInternal(position, property, label, includeChildren, attributes);
        }

        public static bool PropertyFieldInternal(Rect position, RuntimeSerializedProperty property, GUIContent label, bool includeChildren, List<PropertyAttribute> attributes)
        {
            return RuntimeScriptAttributeUtility.GetHandler(property, attributes).OnGUI(position, property, label, includeChildren);
        }

        public static bool DefaultPropertyField(Rect position, RuntimeSerializedProperty property, GUIContent label)
        {
            label = BeginProperty(position, label, property);

            System.Type propertyType = property.PropertyType;

            bool childrenAreExpanded = false;

            // Should we inline? All one-line vars as well as Vector2, Vector3, Rect and Bounds properties are inlined.
            if (!HasVisibleChildFields(property))
            {
                if (propertyType == RuntimeSerializedPropertyType.Bool)
                {
                    EditorGUI.BeginChangeCheck();
                    bool boolValue = EditorGUI.Toggle(position, label, property.BoolValue);
                    if (EditorGUI.EndChangeCheck())
                    {
                        property.BoolValue = boolValue;
                    }
                }
                else if (propertyType == RuntimeSerializedPropertyType.Byte)
                {
                    EditorGUI.BeginChangeCheck();
                    int intValue = EditorGUI.IntField(position, label, property.ByteValue);
                    if (intValue >= byte.MaxValue)
                    {
                        intValue = byte.MaxValue;
                    }
                    else if (intValue <= byte.MinValue)
                    {
                        intValue = byte.MinValue;
                    }
                    if (EditorGUI.EndChangeCheck())
                    {
                        property.ByteValue = (byte)intValue;
                    }
                }
                else if (propertyType == RuntimeSerializedPropertyType.Char)
                {
                    char[] value = new char[] {
                        property.CharValue
                    };
                    bool changed = GUI.changed;
                    GUI.changed = false;
                    string text = EditorGUI.TextField(position, label, new string(value));
                    if (GUI.changed)
                    {
                        if (text.Length == 1)
                        {
                            property.CharValue = (char)text[0];
                        }
                        else
                        {
                            GUI.changed = false;
                        }
                    }
                    GUI.changed |= changed;
                }
                else if (propertyType == RuntimeSerializedPropertyType.Short)
                {
                    EditorGUI.BeginChangeCheck();
                    int intValue = EditorGUI.IntField(position, label, property.ShortValue);
                    if (intValue >= short.MaxValue)
                    {
                        intValue = short.MaxValue;
                    }
                    else if (intValue <= short.MinValue)
                    {
                        intValue = short.MinValue;
                    }
                    if (EditorGUI.EndChangeCheck())
                    {
                        property.ShortValue = (short)intValue;
                    }
                }
                else if (propertyType == RuntimeSerializedPropertyType.Integer)
                {
                    EditorGUI.BeginChangeCheck();
                    int intValue = EditorGUI.IntField(position, label, property.IntValue);
                    if (EditorGUI.EndChangeCheck())
                    {
                        property.IntValue = intValue;
                    }
                }
                else if (propertyType == RuntimeSerializedPropertyType.Long)
                {
                    EditorGUI.BeginChangeCheck();
                    long longValue = EditorGUI.LongField(position, label, property.LongValue);
                    if (EditorGUI.EndChangeCheck())
                    {
                        property.LongValue = longValue;
                    }
                }
                else if (propertyType == RuntimeSerializedPropertyType.sByte)
                {
                    EditorGUI.BeginChangeCheck();
                    int intValue = EditorGUI.IntField(position, label, property.sByteValue);
                    if (intValue >= sbyte.MaxValue)
                    {
                        intValue = sbyte.MaxValue;
                    }
                    else if (intValue <= sbyte.MinValue)
                    {
                        intValue = sbyte.MinValue;
                    }
                    if (EditorGUI.EndChangeCheck())
                    {
                        property.sByteValue = (sbyte)intValue;
                    }
                }
                else if (propertyType == RuntimeSerializedPropertyType.uShort)
                {
                    EditorGUI.BeginChangeCheck();
                    int intValue = EditorGUI.IntField(position, label, property.uShortValue);
                    if (intValue >= ushort.MaxValue)
                    {
                        intValue = ushort.MaxValue;
                    }
                    else if (intValue <= ushort.MinValue)
                    {
                        intValue = ushort.MinValue;
                    }
                    if (EditorGUI.EndChangeCheck())
                    {
                        property.uShortValue = (ushort)intValue;
                    }
                }
                else if (propertyType == RuntimeSerializedPropertyType.uInteger)
                {
                    EditorGUI.BeginChangeCheck();
                    long longValue = EditorGUI.LongField(position, label, property.uIntValue);
                    if (longValue >= uint.MaxValue)
                    {
                        longValue = uint.MaxValue;
                    }
                    else if (longValue <= uint.MinValue)
                    {
                        longValue = uint.MinValue;
                    }
                    if (EditorGUI.EndChangeCheck())
                    {
                        property.uIntValue = (uint)longValue;
                    }
                }
                else if (propertyType == RuntimeSerializedPropertyType.uLong)
                {
                    EditorGUI.BeginChangeCheck();
                    string stringValue = EditorGUI.TextField(position, label, property.uLongValue.ToString());
                    if (EditorGUI.EndChangeCheck())
                    {
                        ulong ulongValue = property.uLongValue;
                        if (ulong.TryParse(stringValue, out ulongValue))
                        {
                            property.uLongValue = ulongValue;
                        }
                    }
                }
                else if (propertyType == RuntimeSerializedPropertyType.Float)
                {
                    EditorGUI.BeginChangeCheck();
                    float floatValue = EditorGUI.FloatField(position, label, property.FloatValue);
                    if (EditorGUI.EndChangeCheck())
                    {
                        property.FloatValue = floatValue;
                    }
                }
                else if (propertyType == RuntimeSerializedPropertyType.Double)
                {
                    EditorGUI.BeginChangeCheck();
                    double doubleValue = EditorGUI.DoubleField(position, label, property.DoubleValue);
                    if (EditorGUI.EndChangeCheck())
                    {
                        property.DoubleValue = doubleValue;
                    }
                }
                else if (propertyType == RuntimeSerializedPropertyType.String)
                {
                    EditorGUI.BeginChangeCheck();
                    string stringValue = EditorGUI.TextField(position, label, property.StringValue);
                    if (EditorGUI.EndChangeCheck())
                    {
                        property.StringValue = stringValue;
                    }
                }
                else if (propertyType == RuntimeSerializedPropertyType.Color)
                {
                    EditorGUI.BeginChangeCheck();
                    Color colorValue = EditorGUI.ColorField(position, label, property.ColorValue);
                    if (EditorGUI.EndChangeCheck())
                    {
                        property.ColorValue = colorValue;
                    }
                }
                else if (propertyType.IsSameOrSubclassOf(RuntimeSerializedPropertyType.Object))
                {
                    GUIStyle style = new GUIStyle();
                    style.richText = true;
                    EditorGUI.LabelField(position, "<color=red>Reference type is not supported!</color>", style);
                }
                else if (propertyType == RuntimeSerializedPropertyType.LayerMask)
                {
                    EditorGUI.BeginChangeCheck();
                    LayerMask layerMaskValue = property.LayerMaskValue;
                    string[] displayedOptions = RuntimeSerializedProperty.GetLayerMaskNames(0);
                    int num = EditorGUI.MaskField(position, label, layerMaskValue.value, displayedOptions);
                    if (EditorGUI.EndChangeCheck())
                    {
                        property.LayerMaskValue = num;
                    }
                }
                else if (propertyType.IsEnum)
                {
                    EditorGUI.BeginChangeCheck();
                    System.Enum enumValue = EditorGUI.EnumPopup(position, label, property.EnumValue);
                    if (EditorGUI.EndChangeCheck())
                    {
                        property.EnumValue = enumValue;
                    }
                }
                else if (propertyType == RuntimeSerializedPropertyType.Vector2)
                {
                    EditorGUI.BeginChangeCheck();
                    Vector2 vector2Value = EditorGUI.Vector2Field(position, label, property.Vector2Value);
                    if (EditorGUI.EndChangeCheck())
                    {
                        property.Vector2Value = vector2Value;
                    }
                }
                else if (propertyType == RuntimeSerializedPropertyType.Vector3)
                {
                    EditorGUI.BeginChangeCheck();
                    Vector3 vector3Value = EditorGUI.Vector3Field(position, label, property.Vector3Value);
                    if (EditorGUI.EndChangeCheck())
                    {
                        property.Vector3Value = vector3Value;
                    }
                }
                else if (propertyType == RuntimeSerializedPropertyType.Vector4)
                {
                    EditorGUI.BeginChangeCheck();
                    Vector4 vector4Value = EditorGUI.Vector4Field(position, label, property.Vector4Value);
                    if (EditorGUI.EndChangeCheck())
                    {
                        property.Vector4Value = vector4Value;
                    }
                }
                else if (propertyType == RuntimeSerializedPropertyType.Rect)
                {
                    EditorGUI.BeginChangeCheck();
                    Rect rectValue = EditorGUI.RectField(position, label, property.RectValue);
                    if (EditorGUI.EndChangeCheck())
                    {
                        property.RectValue = rectValue;
                    }
                }
                else if (propertyType == RuntimeSerializedPropertyType.ArraySize)
                {
                    EditorGUI.BeginChangeCheck();
                    int intValue = EasyGUI.ArraySizeField(position, label, property.ArraySize, EditorStyles.numberField);
                    if (EditorGUI.EndChangeCheck())
                    {
                        property.ArraySize = intValue;
                    }
                }
                else if (propertyType == RuntimeSerializedPropertyType.AnimationCurve)
                {
                    EditorGUI.BeginChangeCheck();
                    if (property.AnimationCurveValue == null)
                    {
                        property.AnimationCurveValue = new AnimationCurve(new Keyframe[] {
                            new Keyframe (0, 1),
                            new Keyframe (1, 1)
                        });
                    }
                    AnimationCurve animationCurveValue = EditorGUI.CurveField(position, label, property.AnimationCurveValue);
                    if (EditorGUI.EndChangeCheck())
                    {
                        property.AnimationCurveValue = animationCurveValue;
                    }

                }
                else if (propertyType == RuntimeSerializedPropertyType.Bounds)
                {
                    EditorGUI.BeginChangeCheck();
                    Bounds boundsValue = EditorGUI.BoundsField(position, label, property.BoundsValue);
                    if (EditorGUI.EndChangeCheck())
                    {
                        property.BoundsValue = boundsValue;
                    }
                }
                else if (propertyType == RuntimeSerializedPropertyType.Gradient)
                {
                    EditorGUI.BeginChangeCheck();
                    Gradient gradientValue = EasyGUI.GradientField(label, position, property.GradientValue);
                    if (EditorGUI.EndChangeCheck())
                    {
                        property.GradientValue = gradientValue;
                    }
                }
                else if (propertyType == RuntimeSerializedPropertyType.FixedBufferSize)
                {
                    EditorGUI.IntField(position, label, property.IntValue);
                }
                else if (propertyType == RuntimeSerializedPropertyType.Vector2Int)
                {
                    EditorGUI.BeginChangeCheck();
                    Vector2Int vector2IntValue = EditorGUI.Vector2IntField(position, label, property.Vector2IntValue);
                    if (EditorGUI.EndChangeCheck())
                    {
                        property.Vector2IntValue = vector2IntValue;
                    }
                }
                else if (propertyType == RuntimeSerializedPropertyType.Vector3Int)
                {
                    EditorGUI.BeginChangeCheck();
                    Vector3Int vector3IntValue = EditorGUI.Vector3IntField(position, label, property.Vector3IntValue);
                    if (EditorGUI.EndChangeCheck())
                    {
                        property.Vector3IntValue = vector3IntValue;
                    }
                }
                else if (propertyType == RuntimeSerializedPropertyType.RectInt)
                {
                    EditorGUI.BeginChangeCheck();
                    RectInt rectIntValue = EditorGUI.RectIntField(position, label, property.RectIntValue);
                    if (EditorGUI.EndChangeCheck())
                    {
                        property.RectIntValue = rectIntValue;
                    }
                }
                else if (propertyType == RuntimeSerializedPropertyType.BoundsInt)
                {
                    EditorGUI.BeginChangeCheck();
                    BoundsInt boundsIntValue = EditorGUI.BoundsIntField(position, label, property.BoundsIntValue);
                    if (EditorGUI.EndChangeCheck())
                    {
                        property.BoundsIntValue = boundsIntValue;
                    }
                }
                else
                {
                    int num = GUIUtility.GetControlID(s_GenericField, FocusType.Keyboard, position);
                    EditorGUI.PrefixLabel(position, num, label);
                }
            }
            // Handle Foldout
            else
            {
                Event tempEvent = new Event(Event.current);

                // Handle the actual foldout first, since that's the one that supports keyboard control.
                // This makes it work more consistent with PrefixLabel.
                childrenAreExpanded = property.IsExpanded;

                bool newChildrenAreExpanded = childrenAreExpanded;
                using (new EditorGUI.DisabledScope(!property.Editable))
                {
                    GUIStyle foldoutStyle = (DragAndDrop.activeControlID != -10) ? EditorStyles.foldout : EditorStyles.foldoutPreDrop;
                    newChildrenAreExpanded = EditorGUI.Foldout(position, childrenAreExpanded, s_PropertyFieldTempContent, true, foldoutStyle);
                }
                if (childrenAreExpanded && property.IsArray && property.ArraySize > property.RuntimeSerializedObject.SerializedObject.maxArraySizeForMultiEditing && property.RuntimeSerializedObject.SerializedObject.isEditingMultipleObjects)
                {
                    Rect boxRect = position;
                    boxRect.xMin += EditorGUIUtility.labelWidth - EasyGUI.Indent;

                    s_ArrayMultiInfoContent.text = s_ArrayMultiInfoContent.tooltip = string.Format(s_ArrayMultiInfoFormatString, property.RuntimeSerializedObject.SerializedObject.maxArraySizeForMultiEditing);
                    EditorGUI.LabelField(boxRect, GUIContent.none, s_ArrayMultiInfoContent, EditorStyles.helpBox);
                }

                if (newChildrenAreExpanded != childrenAreExpanded)
                {
                    // Recursive set expanded
                    if (Event.current.alt)
                    {
                        SetExpandedRecurse(property, newChildrenAreExpanded);
                    }
                    // Expand one element only
                    else
                    {
                        property.IsExpanded = newChildrenAreExpanded;
                    }
                }
                childrenAreExpanded = newChildrenAreExpanded;


                // Check for drag & drop events here, to add objects to an array by dragging to the foldout.
                // The event may have already been used by the Foldout control above, but we want to also use it here,
                // so we use the event copy we made prior to calling the Foldout method.

                // We need to use last s_LastControlID here to ensure we do not break duplicate functionality (fix for case 598389)
                // If we called GetControlID here s_LastControlID would be incremented and would not longer be in sync with GUIUtililty.keyboardFocus that
                // is used for duplicating (See DoPropertyFieldKeyboardHandling)
                int id = EditorGUIUtilityHelper.s_LastControlID;
                switch (tempEvent.type)
                {
                    case EventType.DragExited:
                        if (GUI.enabled)
                        {
                            HandleUtility.Repaint();
                        }

                        break;
                    case EventType.DragUpdated:
                    case EventType.DragPerform:

                        if (position.Contains(tempEvent.mousePosition) && GUI.enabled)
                        {
                            Object[] references = DragAndDrop.objectReferences;

                            // Check each single object, so we can add multiple objects in a single drag.
                            Object[] oArray = new Object[1];
                            bool didAcceptDrag = false;
                            foreach (Object o in references)
                            {
                                oArray[0] = o;
                                Object validatedObject = ValidateObjectFieldAssignment(oArray, null, property, EasyGUI.ObjectFieldValidatorOptions.None);
                                if (validatedObject != null)
                                {
                                    DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
                                    if (tempEvent.type == EventType.DragPerform)
                                    {
                                        property.AppendFoldoutPPtrValue(validatedObject);
                                        didAcceptDrag = true;
                                        DragAndDrop.activeControlID = 0;
                                    }
                                    else
                                    {
                                        DragAndDrop.activeControlID = id;
                                    }
                                }
                            }
                            if (didAcceptDrag)
                            {
                                GUI.changed = true;
                                DragAndDrop.AcceptDrag();
                            }
                        }
                        break;
                }
            }

            EndProperty();

            return childrenAreExpanded;
        }

        public static bool ValidateObjectReferenceValue(RuntimeSerializedProperty property, Object obj, EasyGUI.ObjectFieldValidatorOptions options)
        {
            if ((options & EasyGUI.ObjectFieldValidatorOptions.ExactObjectTypeValidation) == EasyGUI.ObjectFieldValidatorOptions.ExactObjectTypeValidation)
            {
                return property.ValidateObjectReferenceValueExact(obj);
            }
            return property.ValidateObjectReferenceValue(obj);
        }

        public static float GetPropertyHeight(RuntimeSerializedProperty property, List<PropertyAttribute> attributes)
        {
            return GetPropertyHeight(property, null, false, attributes);
        }

        public static float GetPropertyHeight(System.Type type, GUIContent label)
        {
            if (type == RuntimeSerializedPropertyType.Vector3 || type == RuntimeSerializedPropertyType.Vector2 || type == RuntimeSerializedPropertyType.Vector4)
            {
                return ((EasyGUI.LabelHasContent(label) && !EditorGUIUtility.wideMode) ? kSingleLineHeight : 0f) + kSingleLineHeight;
            }
            if (type == RuntimeSerializedPropertyType.Rect || type == RuntimeSerializedPropertyType.RectInt)
            {
                return ((EasyGUI.LabelHasContent(label) && !EditorGUIUtility.wideMode) ? kSingleLineHeight : 0f) + 2 * kSingleLineHeight;
            }
            if (type == RuntimeSerializedPropertyType.Bounds || type == RuntimeSerializedPropertyType.BoundsInt)
            {
                return (EasyGUI.LabelHasContent(label) ? kSingleLineHeight : 0f) + 2 * kSingleLineHeight;
            }
            return kSingleLineHeight;
        }

        public static float GetPropertyHeight(RuntimeSerializedProperty property, GUIContent label, List<PropertyAttribute> attributes)
        {
            return GetPropertyHeight(property, label, false, attributes);
        }

        public static float GetPropertyHeight(RuntimeSerializedProperty property, GUIContent label, bool includeChildren, List<PropertyAttribute> attributes)
        {
            return GetPropertyHeightInternal(property, label, includeChildren, attributes);
        }

        public static float GetPropertyHeightInternal(RuntimeSerializedProperty property, GUIContent label, bool includeChildren, List<PropertyAttribute> attributes)
        {
            return RuntimeScriptAttributeUtility.GetHandler(property, attributes).GetHeight(property, label, includeChildren);
        }

        public static float GetSinglePropertyHeight(RuntimeSerializedProperty property, GUIContent label)
        {
            if (property == null)
            {
                return kSingleLineHeight;
            }
            return GetPropertyHeight(property.PropertyType, label);
        }

        public static void Slider(Rect position, RuntimeSerializedProperty property, float leftValue, float rightValue, GUIContent label)
        {
            label = BeginProperty(position, label, property);

            EditorGUI.BeginChangeCheck();

            float newValue = EditorGUI.Slider(position, label, property.FloatValue, leftValue, rightValue);
            if (EditorGUI.EndChangeCheck())
            {
                property.FloatValue = newValue;
            }

            EndProperty();
        }

        public static void IntSlider(Rect position, RuntimeSerializedProperty property, int leftValue, int rightValue, GUIContent label)
        {
            label = BeginProperty(position, label, property);

            EditorGUI.BeginChangeCheck();

            int newValue = EditorGUI.IntSlider(position, label, property.IntValue, leftValue, rightValue);
            if (EditorGUI.EndChangeCheck())
            {
                property.IntValue = newValue;
            }

            EndProperty();
        }

        private static void DoPropertyFieldKeyboardHandling(RuntimeSerializedProperty property)
        {
            // Delete & Duplicate commands
            if (Event.current.type == EventType.ExecuteCommand || Event.current.type == EventType.ValidateCommand)
            {
                if (GUIUtility.keyboardControl == EditorGUIUtilityHelper.s_LastControlID && (Event.current.commandName == EventCommandNamesHelper.Delete || Event.current.commandName == EventCommandNamesHelper.SoftDelete))
                {
                    if (Event.current.type == EventType.ExecuteCommand)
                    {
                        // Wait with deleting the property until the property stack is empty. See EndProperty.
                        s_PendingPropertyDelete = property.Copy();
                    }
                    Event.current.Use();
                }
                if (GUIUtility.keyboardControl == EditorGUIUtilityHelper.s_LastControlID && Event.current.commandName == EventCommandNamesHelper.Duplicate)
                {
                    if (Event.current.type == EventType.ExecuteCommand)
                    {
                        property.DuplicateCommand();
                    }
                    Event.current.Use();
                }
            }
            s_PendingPropertyKeyboardHandling = null;
        }

        public static void DoPropertyContextMenu(RuntimeSerializedProperty property)
        {
            GenericMenu pm = FillPropertyContextMenu(property);

            if (pm == null || pm.GetItemCount() == 0)
            {
                return;
            }

            Event.current.Use();
            pm.ShowAsContext();
        }

        public static GenericMenu FillPropertyContextMenu(RuntimeSerializedProperty property, RuntimeSerializedProperty linkedProperty = null)
        {
            GenericMenu popupMenu = new GenericMenu();
            RuntimeSerializedProperty propertyWithPath = property.RuntimeSerializedObject.FindProperty(property.PropertyPath);
            RuntimeScriptAttributeUtility.GetHandler(property, null).AddMenuItems(property, popupMenu);

            if (property.RuntimeSerializedObject.SerializedObject.targetObjects.Length == 1 && property.IsInstantiatedPrefab)
            {
                popupMenu.AddItem(EditorGUIUtility.TrTextContent("Revert Value to Prefab"), false, RuntimeTargetChoiceHandler.RevertPrefabPropertyOverride, propertyWithPath);
            }

            // If property is an element in an array, show duplicate and delete menu options
            if (property.Depth > 0 && !property.PropertyPath.EndsWith("Size"))
            {
                var length = property.PropertyPath.LastIndexOf(".");
                var parentPath = property.PropertyPath.Substring(0, length);
                var prop = property.FindProperty(parentPath);

                if (prop != null && prop.IsArray)
                {
                    if (popupMenu.GetItemCount() > 0)
                    {
                        popupMenu.AddSeparator(EmptyStr);
                    }
                    popupMenu.AddItem(EditorGUIUtility.TrTextContent("Duplicate Array Element"), false, (object a) =>
                    {
                        RuntimeTargetChoiceHandler.DuplicateArrayElement(a);
                        EditorGUIUtility.editingTextField = false;
                    }, propertyWithPath);
                    popupMenu.AddItem(EditorGUIUtility.TrTextContent("Delete Array Element"), false, (object a) =>
                    {
                        RuntimeTargetChoiceHandler.DeleteArrayElement(a);
                        EditorGUIUtility.editingTextField = false;
                    }, propertyWithPath);
                }
            }

            // If shift is held down, show debug menu options
            if (Event.current.shift)
            {
                if (popupMenu.GetItemCount() > 0)
                {
                    popupMenu.AddSeparator(EmptyStr);
                }
                popupMenu.AddItem(EditorGUIUtility.TrTextContent("Print Property Path"), false, e => Debug.Log(((SerializedProperty)e).propertyPath), propertyWithPath);
            }
            else
            {
                if (popupMenu.GetItemCount() > 0)
                {
                    popupMenu.AddSeparator(EmptyStr);
                }
                popupMenu.AddItem(EditorGUIUtility.TrTextContent("Copy Component"), false, (object a) =>
                {
                    RuntimeTargetChoiceHandler.CopyComponent(a);
                }, propertyWithPath);
                if (RuntimeTargetChoiceHandler.CanPasteAsNew(propertyWithPath))
                {
                    popupMenu.AddItem(EditorGUIUtility.TrTextContent("Paste Component As New"), false, (object a) =>
                    {
                        RuntimeTargetChoiceHandler.PasteComponentAsNew(a);
                    }, propertyWithPath);
                }
                else
                {
                    popupMenu.AddDisabledItem(EditorGUIUtility.TrTextContent("Paste Component As New"));
                }
                if (RuntimeTargetChoiceHandler.CanPaste(propertyWithPath))
                {
                    popupMenu.AddItem(EditorGUIUtility.TrTextContent("Paste Component Values"), false, (object a) =>
                    {
                        RuntimeTargetChoiceHandler.PasteComponentValues(a);
                    }, propertyWithPath);
                }
                else
                {
                    popupMenu.AddDisabledItem(EditorGUIUtility.TrTextContent("Paste Component Values"));
                }
            }

            if (property.RuntimeSerializedObject.Target != null)
            {
                if (popupMenu.GetItemCount() > 0)
                {
                    popupMenu.AddSeparator(EmptyStr);
                }

                var obj = propertyWithPath.RuntimeSerializedObject.Target;
                var methods = obj.GetType().GetAllInstanceMethods();

                contextMenuIndex.Clear();
                contextMenuResultIndex.Clear();
                easyContextMenuResultIndex.Clear();

                foreach (var method in methods)
                {
                    foreach (ContextMenu contextMenu in method.GetCustomAttributes(typeof(ContextMenu), false))
                    {
                        contextMenuIndex.Add(contextMenu, method);
                    }
                }
                var orderedContextMenu = contextMenuIndex.Select(kvp => kvp.Key).OrderByDescending(menu => menu.validate).ThenBy(menu => menu.priority).ToArray();
                foreach (var menu in orderedContextMenu)
                {
                    if (menu.validate)
                    {
                        var result = (bool)contextMenuIndex[menu].Invoke(obj, null);
                        if (contextMenuResultIndex.ContainsKey(menu.menuItem))
                        {
                            if (contextMenuResultIndex[menu.menuItem] && !result)
                            {
                                contextMenuResultIndex[menu.menuItem] = result;
                            }
                        }
                        else
                        {
                            contextMenuResultIndex.Add(menu.menuItem, result);
                        }
                    }
                    else
                    {
                        if (contextMenuResultIndex.ContainsKey(menu.menuItem) && !contextMenuResultIndex[menu.menuItem])
                        {
                            popupMenu.AddDisabledItem(EditorGUIUtility.TrTextContent(menu.menuItem));
                        }
                        else
                        {
                            popupMenu.AddItem(EditorGUIUtility.TrTextContent(menu.menuItem), false, (object o) =>
                            {
                                contextMenuIndex[menu].Invoke(o, null);
                            }, obj);
                        }
                    }
                }
            }
            return popupMenu;
        }

        public static void SetExpandedRecurse(RuntimeSerializedProperty property, bool expanded)
        {
            RuntimeSerializedProperty search = property.Copy();
            search.IsExpanded = expanded;

            int depth = search.Depth;
            while (search.NextVisible(true) && search.Depth > depth)
            {
                if (search.HasVisibleChildren)
                {
                    search.IsExpanded = expanded;
                }
            }
        }

        public static void EndProperty()
        {
            EditorGUI.showMixedValue = false;
            RuntimePropertyGUIData data = s_PropertyStack.Pop();
            // Context menu
            // Handle context menu in EndProperty instead of BeginProperty. This ensures that child properties
            // get the event rather than parent properties when clicking inside the child property rects, but the menu can
            // still be invoked for the parent property by clicking inside the parent rect but outside the child rects.
            if (Event.current.type == EventType.ContextClick && data.TotalPosition.Contains(Event.current.mousePosition))
            {
                DoPropertyContextMenu(data.Property);
            }

            EditorGUIUtilityHelper.SetBoldDefaultFont(data.WasBoldDefaultFont);
            GUI.enabled = data.WasEnabled;
            GUI.backgroundColor = data.Color;

            if (s_PendingPropertyKeyboardHandling != null)
            {
                DoPropertyFieldKeyboardHandling(s_PendingPropertyKeyboardHandling);
            }

            // Wait with deleting the property until the property stack is empty in order to avoid
            // deleting a property in the middle of GUI calls that's dependent on it existing.
            if (s_PendingPropertyDelete != null && s_PropertyStack.Count == 0)
            {
                // For SerializedProperty iteration reasons, if the property we delete is the current property,
                // we have to delete on the actual iterator property rather than a copy of it,
                // otherwise we get an error next time we call NextVisible on the iterator property.
                if (s_PendingPropertyDelete.PropertyPath == data.Property.PropertyPath)
                {
                    data.Property.DeleteCommand();
                }
                else
                {
                    s_PendingPropertyDelete.DeleteCommand();
                }
                s_PendingPropertyDelete = null;
            }
        }

        public static GUIContent BeginProperty(Rect totalPosition, GUIContent label, RuntimeSerializedProperty property)
        {
            Highlighter.HighlightIdentifier(totalPosition, property.PropertyPath);

            if (s_PendingPropertyKeyboardHandling != null)
            {
                DoPropertyFieldKeyboardHandling(s_PendingPropertyKeyboardHandling);
            }

            // Properties can be nested, so A BeginProperty may not be followed by its corresponding EndProperty
            // before there have been one or more pairs of BeginProperty/EndProperty in between.
            // The keyboard handling for a property (that handles duplicate and delete commands for array items)
            // uses EditorGUI.lastControlID so it has to be executed for a property before any possible child
            // properties are handled. However, it can't be done in it's own BeginProperty, because the controlID
            // for the property is not yet known at that point. For that reason we mark the keyboard handling as
            // pending and handle it either the next BeginProperty call (for the first child property) or if there's
            // no child properties, then in the matching EndProperty call.
            s_PendingPropertyKeyboardHandling = property;

            if (property == null)
            {
                string error = ((label != null) ? (label.text + ": ") : EmptyStr) + "RuntimeSerializedProperty is null";
                EditorGUI.HelpBox(totalPosition, "null", MessageType.Error);
                throw new System.NullReferenceException(error);
            }

            s_PropertyFieldTempContent.text = LocalizationDatabaseHelper.GetLocalizedString((label != null) ? label.text : property.DisplayName); // no necessary to be translated.
            s_PropertyFieldTempContent.tooltip = EasyGUI.IsCollectingTooltips ? ((label == null) ? property.Tooltip : label.tooltip) : null;
            string attributeTooltip = RuntimeScriptAttributeUtility.GetHandler(property, null).Tooltip;
            if (attributeTooltip != null)
            {
                s_PropertyFieldTempContent.tooltip = attributeTooltip;
            }
            s_PropertyFieldTempContent.image = label != null ? label.image : null;

            // In inspector debug mode & when holding down alt. Show the property path of the property.
            if (Event.current.alt && property.RuntimeSerializedObject.SerializedObject.InspectorMode() != InspectorMode.Normal)
            {
                s_PropertyFieldTempContent.tooltip = s_PropertyFieldTempContent.text = property.PropertyPath;
            }

            bool wasBoldDefaultFont = EditorGUIUtilityHelper.GetBoldDefaultFont();
            if (property.RuntimeSerializedObject.SerializedObject.targetObjects.Length == 1 && property.IsInstantiatedPrefab)
            {
                EditorGUIUtilityHelper.SetBoldDefaultFont(property.PrefabOverride);
            }

            s_PropertyStack.Push(new RuntimePropertyGUIData(property, totalPosition, wasBoldDefaultFont, GUI.enabled, GUI.backgroundColor));

            GUI.enabled &= property.Editable;

            return s_PropertyFieldTempContent;
        }

        public static Object ValidateObjectFieldAssignment(Object[] references, System.Type objType, RuntimeSerializedProperty property, EasyGUI.ObjectFieldValidatorOptions options)
        {
            if (references.Length > 0)
            {
                bool dragAssignment = DragAndDrop.objectReferences.Length > 0;
                bool isTextureRef = (references[0] != null && references[0] is Texture2D);

                if (objType == typeof(Sprite) && isTextureRef && dragAssignment)
                {
                    return SpriteUtilityHelper.TextureToSprite(references[0] as Texture2D);
                }

                if (property != null)
                {
                    if (references[0] != null && ValidateObjectReferenceValue(property, references[0], options))
                    {
                        if (EditorSceneManager.preventCrossSceneReferences && EasyGUI.CheckForCrossSceneReferencing(references[0], property.RuntimeSerializedObject.TargetObject))
                        {
                            return null;
                        }

                        if (objType != null)
                        {
                            if (references[0] is GameObject && typeof(Component).IsAssignableFrom(objType))
                            {
                                GameObject go = (GameObject)references[0];
                                references = go.GetComponents(typeof(Component));
                            }
                            foreach (Object i in references)
                            {
                                if (i != null && objType.IsAssignableFrom(i.GetType()))
                                {
                                    return i;
                                }
                            }
                        }
                        else
                        {
                            return references[0];
                        }
                    }

                    // If array, test against the target arrayElementType, if not test against the target Type.
                    var type = property.PropertyType;
                    if (property.IsArray)
                    {
                        type = property.ArrayElementType;
                    }

                    if (type == RuntimeSerializedPropertyType.Sprite && isTextureRef && dragAssignment)
                    {
                        return SpriteUtilityHelper.TextureToSprite(references[0] as Texture2D);
                    }
                }
                else
                {
                    if (references[0] != null && references[0] is GameObject && typeof(Component).IsAssignableFrom(objType))
                    {
                        GameObject gameObject = (GameObject)references[0];
                        references = gameObject.GetComponents(typeof(Component));
                    }
                    foreach (Object i in references)
                    {
                        if (i != null && objType.IsAssignableFrom(i.GetType()))
                        {
                            return i;
                        }
                    }
                }
            }
            return null;
        }

        #endregion
    }
}
