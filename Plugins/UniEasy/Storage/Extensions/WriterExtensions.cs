using UnityEngine;
using System;

namespace UniEasy
{
    public static class WriterExtensions
    {
        public static bool HasKey(this EasyDictionary<string, EasyObject> writer, string key)
        {
            return writer.ToDictionary().ContainsKey(key);
        }

        public static object GetObject(this EasyDictionary<string, EasyObject> writer, string key)
        {
            if (writer.ToDictionary().ContainsKey(key))
            {
                return writer.ToDictionary()[key].GetObject();
            }
            return default(object);
        }

        public static void SetObject(this EasyDictionary<string, EasyObject> writer, string key, object value)
        {
            if (HasKey(writer, key))
            {
                writer.ToDictionary()[key] = new EasyObject(value);
            }
            else
            {
                writer.ToDictionary().Add(key, new EasyObject(value));
            }
        }

        public static T Get<T>(this EasyDictionary<string, EasyObject> writer, string key)
        {
            var type = typeof(T);
            if (type == typeof(string))
            {
                return (T)GetObject(writer, key);
            }
            else if (type.IsArray)
            {
                Debug.LogError("Sorry, we can not auto convert string type to array type, " +
                "But you can use GetArray<T> (string key) method replaced.");
            }
            else if (type.IsSerializable && type.IsPrimitive)
            {
                return (T)GetObject(writer, key);
            }
            else if (type.IsSerializable && type.IsEnum)
            {
            }
            else if (type.IsSerializable && type == typeof(Nullable))
            {
            }
            else if (type.IsSerializable && (type.IsClass || type.IsValueType))
            {
                if (type.IsSubclassOf(typeof(UnityEngine.Object)))
                {
                    Debug.LogError("Sorry, we can not overwrite UnityEngine.Object Type(such as MonoBehaviour or ScriptableObject), " +
                    "But you can use Get<T> (string key, T target) method replaced.");
                }
                else
                {
                    return JsonUtility.FromJson<T>(GetObject(writer, key).ToString());
                }
            }
            return default(T);
        }

        public static void Get(this EasyDictionary<string, EasyObject> writer, string key, object target, Type type)
        {
            if (type.IsSubclassOf(typeof(UnityEngine.Object)))
            {
                JsonUtility.FromJsonOverwrite(GetObject(writer, key).ToString(), target);
            }
        }

        public static void Get<T>(this EasyDictionary<string, EasyObject> writer, string key, T target)
        {
            Get(writer, key, target, target.GetType());
        }

        public static void GetArray<T>(this EasyDictionary<string, EasyObject> writer, string key, T[] target)
        {
            var array = GetArray<string>(writer, key);
            for (int i = 0; i < array.Length && i < target.Length; i++)
            {
                var type = target[i].GetType();
                if (type.IsSubclassOf(typeof(UnityEngine.Object)))
                {
                    JsonUtility.FromJsonOverwrite(array[i], target[i]);
                }
            }
        }

        public static T[] GetArray<T>(this EasyDictionary<string, EasyObject> writer, string key)
        {
            var content = Get<string>(writer, key);
            if (!string.IsNullOrEmpty(content))
            {
                return content.ToArray<T>();
            }
            return default(T[]);
        }

        public static void Set(this EasyDictionary<string, EasyObject> writer, string key, object value, Type type)
        {
            if (value == null)
            {
                Debug.LogError("NullReferenceException: Object reference not set to an instance of an object");
            }
            else if (type == typeof(string))
            {
                SetObject(writer, key, value);
            }
            else if (type.IsArray)
            {
                Debug.LogError("Sorry, we can not auto convert array type to string type, " +
                "But you can use SetArray<T> (string key, object value) method replaced.");
            }
            else if (type.IsSerializable && type.IsPrimitive)
            {
                SetObject(writer, key, value);
            }
            else if (type.IsSerializable && type.IsEnum)
            {
            }
            else if (type.IsSerializable && type == typeof(Nullable))
            {
            }
            else if (type.IsClass || (type.IsSerializable && type.IsValueType))
            {
#if UNITY_EDITOR
                SetObject(writer, key, JsonUtility.ToJson(value, true));
#else
				SetObject (writer, key, JsonUtility.ToJson (value));
#endif
            }
        }

        public static void Set<T>(this EasyDictionary<string, EasyObject> writer, string key, T value)
        {
            Set(writer, key, value, typeof(T));
        }

        public static void SetArray<T>(this EasyDictionary<string, EasyObject> writer, string key, object value)
        {
            var content = default(string);
            if (!value.GetType().IsArray)
            {
                T[] o = new object[] { value } as T[];
                content = o.ToString<T>();
            }
            else
            {
                content = value.ToString<T>();
            }
            Set<string>(writer, key, content);
        }

        public static void Remove(this EasyDictionary<string, EasyObject> writer, string key)
        {
            var dic = writer.ToDictionary();
            if (dic != null && dic.ContainsKey(key))
            {
                dic.Remove(key);
                writer = new EasyDictionary<string, EasyObject>(dic);
            }
        }

        public static void Clear(this EasyDictionary<string, EasyObject> writer)
        {
            var dic = writer.ToDictionary();
            if (dic != null)
            {
                dic.Clear();
                writer = new EasyDictionary<string, EasyObject>(dic);
            }
        }
    }
}
