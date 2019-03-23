using UnityEngine;
using System.IO;
using System;
using UniRx;

namespace UniEasy
{
    public class ReactiveWriter : IDisposableContainer
    {
        string FilePath = "Defaults";
        EasyDictionary<string, EasyObject> writer;

        private CompositeDisposable disposer = new CompositeDisposable();

        public CompositeDisposable Disposer
        {
            get { return disposer; }
            set { disposer = value; }
        }

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
            writer = setter ?? new EasyDictionary<string, EasyObject>();

            if (Application.isPlaying)
            {
                Observable.OnceApplicationQuit().Subscribe(_ =>
                {
                    Dispose();
                });
            }
        }

        public void Record()
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
                EasyWriter.Serialize(tempPath, writer);
                var newPath = FilePath.Substring(FilePath.IndexOf("Assets"));
                UnityEditor.AssetDatabase.Refresh();
                UnityEditor.AssetDatabase.DeleteAsset(newPath);
                UnityEditor.AssetDatabase.MoveAsset("Assets/" + fileName, newPath);
                UnityEditor.AssetDatabase.Refresh();
                return;
            }
#endif
            EasyWriter.Serialize(FilePath, writer);
        }

        public virtual void Dispose()
        {
            Record();
            Disposer.Dispose();
        }

        public bool HasKey(string key)
        {
            return writer.HasKey(key);
        }

        public object GetObject(string key)
        {
            return writer.GetObject(key);
        }

        public void SetObject(string key, object value)
        {
            writer.SetObject(key, value);
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                Record();
            }
#endif
        }

        public T Get<T>(string key)
        {
            return writer.Get<T>(key);
        }

        public void Get(string key, object value, Type type)
        {
            writer.Get(key, value, type);
        }

        public void Get<T>(string key, T target)
        {
            writer.Get<T>(key, target);
        }

        public void GetArray<T>(string key, T[] target)
        {
            writer.GetArray<T>(key, target);
        }

        public T[] GetArray<T>(string key)
        {
            return writer.GetArray<T>(key);
        }

        public void Set(string key, object value, Type type)
        {
            writer.Set(key, value, type);
        }

        public void Set<T>(string key, T value)
        {
            writer.Set<T>(key, value);
        }

        public void SetArray<T>(string key, object value)
        {
            writer.SetArray<T>(key, value);
        }

        public void Remove(string key)
        {
            writer.Remove(key);
        }

        public void Clear()
        {
            writer.Clear();
        }
    }
}
