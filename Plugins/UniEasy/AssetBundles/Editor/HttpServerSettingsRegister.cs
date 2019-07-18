using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace UniEasy.Editor
{
    public static class HttpServerSettingsRegister
    {
        [SettingsProvider]
        public static SettingsProvider CreateHttpServerSettingsProvider()
        {
            // First parameter is the path in the Settings window.
            // Second parameter is the scope of this setting: it only appears in the Preferences Settings window.
            var provider = new SettingsProvider("Preferences/Http Server", SettingsScope.User)
            {
                // Create the SettingsProvider and initialize its drawing (IMGUI) function in place:
                guiHandler = (searchContext) =>
                {
                    var settings = HttpServerSettings.GetSerializedSettings();
                    EditorGUILayout.PropertyField(settings.FindProperty("IsEnable"), new GUIContent("Is Enable"));
                    EditorGUILayout.PropertyField(settings.FindProperty("Address"), new GUIContent("Address"));
                    EditorGUILayout.PropertyField(settings.FindProperty("Port"), new GUIContent("Port"));
                    settings.ApplyModifiedProperties();
                },

                // Populate the search keywords to enable smart search filtering and label highlighting:
                keywords = new HashSet<string>(new[] { "Address", "Port" })
            };

            return provider;
        }
    }
}
