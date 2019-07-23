using System.Threading;
using UnityEngine;
using System.Net;
using System.IO;
using System;

namespace UniEasy
{
    public class HttpServer : IDisposable
    {
        private HttpListener listener;
        private Thread listenerThread;
        private static HttpServerSettings settings;

#if UNITY_EDITOR
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        public static void Initialize()
        {
            settings = HttpServerSettings.GetOrCreateSettings();
            if (settings.IsEnable)
            {
                var server = new HttpServer();
                server.Start();
            }
        }
#endif

        public void Start()
        {
            listener = new HttpListener();
            listener.Prefixes.Add(settings.URL);
            listener.AuthenticationSchemes = AuthenticationSchemes.Anonymous;
            listener.Start();

            listenerThread = new Thread(StartListener);
            listenerThread.Start();
        }

        public void Dispose()
        {
            if (listener != null)
            {
                listener.Stop();
                listener = null;
            }
            if (listenerThread != null)
            {
                listenerThread.Abort();
                listenerThread = null;
            }
        }

        private void StartListener()
        {
            while (true)
            {
                var result = listener.BeginGetContext(ListenerCallback, listener);
                result.AsyncWaitHandle.WaitOne();
            }
        }

        private void ListenerCallback(IAsyncResult result)
        {
            var context = listener.EndGetContext(result);

            if (context.Request.QueryString.AllKeys.Length > 0)
            {
                foreach (var key in context.Request.QueryString.AllKeys)
                {
#if UNITY_EDITOR
                    Debug.Log("Key: " + key + ", Value: " + context.Request.QueryString.GetValues(key)[0]);
#endif
                }
            }

            if (context.Request.HttpMethod == "GET")
            {
                Thread.Sleep(1000);
                if (context.Request.Url.LocalPath.StartsWith("//" + PathsUtility.GetPlatformName()))
                {
                    var startIndex = PathsUtility.GetPlatformName().Length + 2;
                    var localPath = context.Request.Url.LocalPath.Substring(startIndex);
                    context.Response.WriteFile(PathsUtility.GetABOutPath() + localPath);
                }
            }
            else if (context.Request.HttpMethod == "POST")
            {
                Thread.Sleep(1000);
                var data = new StreamReader(context.Request.InputStream, context.Request.ContentEncoding).ReadToEnd();
#if UNITY_EDITOR
                Debug.Log(data);
#endif
            }
            else if (context.Request.HttpMethod == "HEAD")
            {

            }

            context.Response.Close();
        }
    }
}
