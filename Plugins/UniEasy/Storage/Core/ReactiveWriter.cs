using UnityEngine;
using System.IO;
using System;
using UniRx;

namespace UniEasy
{
    public class ReactiveWriter : IDisposableContainer
    {
        string FilePath = "Defaults";
        EasyDictionary<string, EasyObject> dict;

        public CompositeDisposable Disposer { get; set; } = new CompositeDisposable();

        public ReactiveWriter(EasyDictionary<string, EasyObject> setter)
        {
            Initialize(null, setter);
        }

        public ReactiveWriter(string path, EasyDictionary<string, EasyObject> setter)
        {
            Initialize(path, setter);
        }

        void Initialize(string path, EasyDictionary<string, EasyObject> setter)
        {
            FilePath = path;
            dict = setter ?? new EasyDictionary<string, EasyObject>();

            if (Application.isPlaying)
            {
                Observable.OnceApplicationQuit().Subscribe(_ =>
                {
                    Dispose();
                });
            }
        }

        public void Save()
        {
            if (string.IsNullOrEmpty(FilePath))
            {
                return;
            }
#if UNITY_EDITOR
            // You can't create assets(eg. json) directly in the streamingassets folder,
            // The created assets have some issues(can't be readable),
            // So we need to create assets outside first,
            // Then move them to the streamingassets folder.
            if (FilePath.Contains(Application.streamingAssetsPath))
            {
                var fileName = Path.GetFileName(FilePath);
                var tempPath = string.Format("{0}/{1}", Application.dataPath, fileName);
                EasyWriter.Serialize(tempPath, dict);
                var newPath = FilePath.Substring(FilePath.IndexOf("Assets"));
                UnityEditor.AssetDatabase.Refresh();
                UnityEditor.AssetDatabase.DeleteAsset(newPath);
                UnityEditor.AssetDatabase.MoveAsset("Assets/" + fileName, newPath);
                UnityEditor.AssetDatabase.Refresh();
                return;
            }
#endif
            EasyWriter.Serialize(FilePath, dict);
        }

        public virtual void Dispose()
        {
            Save();
            Disposer.Dispose();
        }

        public bool HasKey(string key)
        {
            return dict.HasKey(key);
        }

        public object GetObject(string key)
        {
            return dict.GetObject(key);
        }

        public void SetObject(string key, object value)
        {
            dict.SetObject(key, value);
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                Save();
            }
#endif
        }

        public T Get<T>(string key)
        {
            return dict.Get<T>(key);
        }

        public void Get(string key, object value, Type type)
        {
            dict.Get(key, value, type);
        }

        public void Get<T>(string key, T target)
        {
            dict.Get<T>(key, target);
        }

        public void GetArray<T>(string key, T[] target)
        {
            dict.GetArray<T>(key, target);
        }

        public T[] GetArray<T>(string key)
        {
            return dict.GetArray<T>(key);
        }

        public void Set(string key, object value, Type type)
        {
            dict.Set(key, value, type);
        }

        public void Set<T>(string key, T value)
        {
            dict.Set<T>(key, value);
        }

        public void SetArray<T>(string key, object value)
        {
            dict.SetArray<T>(key, value);
        }

        public void Remove(string key)
        {
            dict.Remove(key);
        }

        public void Clear()
        {
            dict.Clear();
        }
    }
}
