#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace UniEasy
{
    public class HttpServerSettings : ScriptableObject
    {
        public bool IsEnable;
        public string Address = "localhost";
        public int Port = 4747;

        public string URL
        {
            get
            {
                return string.Format("http://{0}:{1}/", Address, Port);
            }
        }

        private const string HttpServerSettingsPath = "Assets/UnityFramework/Plugins/UniEasy/Resources/HttpServerSettings.asset";

        public static HttpServerSettings GetOrCreateSettings()
        {
            var settings = Resources.Load<HttpServerSettings>("HttpServerSettings");
#if UNITY_EDITOR
            if (settings == null)
            {
                settings = CreateInstance<HttpServerSettings>();
                AssetDatabase.CreateAsset(settings, HttpServerSettingsPath);
                AssetDatabase.SaveAssets();
            }    
#endif
            return settings;
        }

#if UNITY_EDITOR
        public static SerializedObject GetSerializedSettings()
        {
            return new SerializedObject(GetOrCreateSettings());
        }
#endif
    }
}
