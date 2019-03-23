using System.Collections.Generic;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using UnityEditor;
using System.Linq;
using System;

namespace UniEasy.Editor
{
    [CustomPropertyDrawer(typeof(TypePopupAttribute))]
    public class TypePopupDrawer : PropertyDrawer
    {
        private string searchCondition = "";
        private SearchField searchField = new SearchField();
        private Dictionary<string, bool> useSearchIndex = new Dictionary<string, bool>();

        private static string EmptyStr = "";
        private static string NoneStr = "None";
        private static string SearchLabelStr = "Search";
        private static string SearchLabelHtmlString = "#FFFF00FF";
        private static string SearchLabelTooltipStr = "After the input is complete, press the Enter key";
        private static Dictionary<Type, List<Type>> typesIndex = new Dictionary<Type, List<Type>>();
        private static Dictionary<Type, string[]> displayedOptionsIndex = new Dictionary<Type, string[]>();
        private static Dictionary<Type, int[]> optionValuesIndex = new Dictionary<Type, int[]>();

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            List<Type> types;
            string[] displayedOptions;
            int[] optionValues;
            int selectedIndex = GetTypePopupFromProperty(property, attribute as TypePopupAttribute, out types, out displayedOptions, out optionValues);

            if (!useSearchIndex.ContainsKey(property.propertyPath))
            {
                useSearchIndex.Add(property.propertyPath, false);
            }
            useSearchIndex[property.propertyPath] = EditorGUI.Foldout(new Rect(position.position, new Vector2(EasyGUI.Indent, EditorGUIUtility.singleLineHeight + 3)), useSearchIndex[property.propertyPath], GUIContent.none);
            EditorGUI.BeginChangeCheck();
            var index = EditorGUI.IntPopup(new Rect(position.position, new Vector2(position.width, EditorGUIUtility.singleLineHeight + 3)), property.displayName, selectedIndex, displayedOptions, optionValues);
            if (EditorGUI.EndChangeCheck())
            {
                property.stringValue = index == 0 ? null : types[index].FullName;
            }
            if (useSearchIndex[property.propertyPath])
            {
                var height = EditorGUIUtility.singleLineHeight + 3;
                var searchPosition = new Rect(new Vector2(position.x - EasyGUI.Indent, position.y + height), new Vector2(position.width, height));

                searchCondition = EasyGUI.SearchFieldWithPopupMenu(searchPosition, SearchLabelStr, searchField, searchCondition, popupMenu =>
                {
                    foreach (var type in types.Where(type => type != null && type.FullName.ToLower().Contains(searchCondition.ToLower())))
                    {
                        popupMenu.AddItem(new GUIContent(type.Name), false, () =>
                        {
                            property.stringValue = type.FullName;
                            property.serializedObject.ApplyModifiedProperties();
                            searchCondition = EmptyStr;
                        });
                    }
                }, SearchLabelHtmlString, SearchLabelTooltipStr);
            }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var height = base.GetPropertyHeight(property, label);
            if (!useSearchIndex.ContainsKey(property.propertyPath))
            {
                useSearchIndex.Add(property.propertyPath, false);
            }
            if (useSearchIndex[property.propertyPath])
            {
                height += EditorGUIUtility.singleLineHeight + 6;
            }
            return height;
        }

        private static int GetTypePopupFromProperty(SerializedProperty property, TypePopupAttribute attribute, out List<Type> types, out string[] displayedOptions, out int[] optionValues)
        {
            if (typesIndex.ContainsKey(attribute.Type))
            {
                types = typesIndex[attribute.Type];
                displayedOptions = displayedOptionsIndex[attribute.Type];
                optionValues = optionValuesIndex[attribute.Type];
            }
            else
            {
                types = EasyEditor.Types.Where(type => attribute.Type.IsAssignableFrom(type)).ToList();
                types.Insert(0, null);
                displayedOptions = types.Select(type => type == null ? NoneStr : type.Name).ToArray();
                optionValues = new int[displayedOptions.Length];
                for (int i = 0; i < optionValues.Length; i++)
                {
                    optionValues[i] = i;
                }

                typesIndex.Add(attribute.Type, types);
                displayedOptionsIndex.Add(attribute.Type, displayedOptions);
                optionValuesIndex.Add(attribute.Type, optionValues);
            }

            if (string.IsNullOrEmpty(property.stringValue))
            {
                return 0;
            }
            else
            {
                var type = property.stringValue.GetTypeWithAssembly();

                if (type == null)
                {
                    return 0;
                }
                else
                {
                    return types.IndexOf(type);
                }
            }
        }
    }
}
