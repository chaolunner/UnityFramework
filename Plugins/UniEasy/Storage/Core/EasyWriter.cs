using System.Runtime.Serialization.Formatters.Binary;
using System.Collections;
#if (NETFX_CORE || NET_4_6 || NET_STANDARD_2_0 || UNITY_WSA_10_0)
using System.Threading;
#endif
using UnityEngine;
using System.Text;
#if CSHARP_7_OR_LATER || (UNITY_2018_3_OR_NEWER && (NET_STANDARD_2_0 || NET_4_6))
using UniRx.Async;
#endif
using System.IO;
using System;
using UniRx;

namespace UniEasy
{
    public partial class EasyWriter
    {
        public static void Serialize<T>(string path, T t)
        {
            var fs = new FileStream(path, FileMode.Create);
            try
            {
#if UNITY_EDITOR
                var serialize = JsonUtility.ToJson(t, true);
#else
				var serialize = JsonUtility.ToJson (t);
#endif
                var bytes = Encoding.UTF8.GetBytes(serialize);
                fs.Write(bytes, 0, bytes.Length);
            }
            catch (Exception e)
            {
                Debug.LogError("Failed to serialize. Reason: " + e.Message);
                throw;
            }
            finally
            {
                fs.Close();
            }
        }

        public static T Deserialize<T>(string path)
        {
            T t = default(T);
            if (File.Exists(path))
            {
                var fs = new FileStream(path, FileMode.Open);
                try
                {
                    var bytes = new byte[(int)fs.Length];
                    fs.Read(bytes, 0, bytes.Length);
                    var value = Encoding.UTF8.GetString(bytes);
                    t = JsonUtility.FromJson<T>(value);
                }
                catch (Exception e)
                {
                    Debug.LogError("Failed to deserialize. Reason: " + e.Message);
                    throw;
                }
                finally
                {
                    fs.Close();
                }
            }
            return t;
        }

        public static IObservable<T> DeserializeAsync<T>(string path)
        {
#if UNITY_EDITOR
            if (!File.Exists(path))
            {
                return Observable.ToObservable<T>(new T[] { default(T) });
            }
#endif
#if UNITY_ANDROID
            if (path.StartsWith(Application.streamingAssetsPath))
            {
#if UNITY_EDITOR
                path = "file://" + path;
#endif
#if CSHARP_7_OR_LATER || (UNITY_2018_3_OR_NEWER && (NET_STANDARD_2_0 || NET_4_6))
                return new UniTask<string>(() =>
                {
                    return GetWWWTask(path);
                }).ToObservable().Select(json => JsonUtility.FromJson<T>(json));
#else
                return Observable.FromCoroutine<string>((observer, cancellationToken) =>
                {
                    return GetWWWCore(path, observer, cancellationToken);
                }).Last().Select(x => JsonUtility.FromJson<T>(x));
#endif
            }
#endif
            return Observable.ToObservable<T>(new T[] { Deserialize<T>(path) });
        }

        public static IEnumerator GetWWWCore(string url, IObserver<string> observer, CancellationToken cancellationToken)
        {
            var www = new WWW(url);

            while (!www.isDone && !cancellationToken.IsCancellationRequested)
            {
                yield return null;
            }

            if (cancellationToken.IsCancellationRequested)
            {
                yield break;
            }

            if (!string.IsNullOrEmpty(www.error))
            {
                observer.OnError(new Exception(www.error));
            }
            else
            {
                observer.OnNext(www.text);
                observer.OnCompleted();
            }
        }

#if CSHARP_7_OR_LATER || (UNITY_2018_3_OR_NEWER && (NET_STANDARD_2_0 || NET_4_6))
        public static async UniTask<string> GetWWWTask(string url)
        {
            var www = new WWW(url);

            while (!www.isDone)
            {
                await Observable.NextFrame();
            }

            if (!string.IsNullOrEmpty(www.error))
            {
                throw new Exception(www.error);
            }
            else
            {
                return www.text;
            }
        }
#endif

        public static byte[] SerializeObject(object o)
        {
            if (o == null)
            {
                return null;
            }
            var ms = new MemoryStream();
            var bf = new BinaryFormatter();
            bf.Serialize(ms, o);
            ms.Position = 0;
            var data = ms.GetBuffer();
            ms.Read(data, 0, data.Length);
            ms.Close();
            return data;
        }

        public static object DeserializeObject(byte[] data)
        {
            object o = null;
            if (data == null || data.Length <= 0)
            {
                return o;
            }
            var ms = new MemoryStream();
            ms.Write(data, 0, data.Length);
            ms.Position = 0;
            var bf = new BinaryFormatter();
            o = bf.Deserialize(ms);
            ms.Close();
            return o;
        }
    }
}
