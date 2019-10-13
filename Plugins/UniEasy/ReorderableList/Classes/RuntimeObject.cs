using UnityEngine;

namespace UniEasy
{
    [System.Serializable]
    public class RuntimeObject
    {
        public string Type;
        public string Data;

        public RuntimeObject()
        {
            Type = string.Empty;
            Data = string.Empty;
        }

        public RuntimeObject(string type, string data)
        {
            Type = type;
            Data = data;
        }

        private object CreateInstance()
        {
            var type = Type.GetTypeFromCached();

            if (type.IsSameOrSubclassOf(typeof(ScriptableObject)))
            {
                var obj = ScriptableObject.CreateInstance(type);
                JsonUtility.FromJsonOverwrite(Data, obj);
                return obj;
            }
            else
            {
                return JsonUtility.FromJson(Data, type);
            }
        }

        public static object FromJson(string json)
        {
            try
            {
                return JsonUtility.FromJson<RuntimeObject>(json).CreateInstance();
            }
            catch
            {
                return null;
            }
        }

        public static string ToJson(string type, string data)
        {
            return JsonUtility.ToJson(new RuntimeObject(type, data));
        }
    }
}
