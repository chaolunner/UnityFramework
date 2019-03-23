using System.Collections.Generic;
using System.Collections;
using System.Reflection;
using System.Threading;
using UnityEngine;
using System.Linq;
using System.Net;
using System.IO;
using System;

namespace UniEasy.Console
{
    public class Server : MonoBehaviour
    {
        public int Port = 55055;

        public delegate void FileHandler(RequestContext context, bool download);

        private static Thread MainThread;
        private static string FileRoot;
        private static HttpListener Listener = new HttpListener();
        private static List<RouteAttribute> RegisteredRoutes;
        private static Queue<RequestContext> MainRequests = new Queue<RequestContext>();

        private static Dictionary<string, string> FileTypes = new Dictionary<string, string> {
            { "js",   "application/javascript" },
            { "json", "application/json" },
            { "icns", "image/x-icon" },
            { "ico",  "image/x-icon" },
            { "jpg",  "image/jpeg" },
            { "jpeg", "image/jpeg" },
            { "gif",  "image/gif" },
            { "png",  "image/png" },
            { "htm",  "text/html" },
            { "html", "text/html" },
            { "css",  "text/css" },
        };

        void Awake()
        {
            MainThread = Thread.CurrentThread;
            FileRoot = Path.Combine(Application.streamingAssetsPath, "Console");

            Listener.Prefixes.Add("http://*:" + Port + "/");
            Listener.Start();
            Listener.BeginGetContext(ListenerCallback, null);

            StartCoroutine(AsyncHandleRequests());
        }

#if !UNITY_EDITOR
		void OnApplicationPause (bool paused)
		{
			if (paused) {
				Listener.Stop ();
			} else {
				Listener.Start ();
				Listener.BeginGetContext (ListenerCallback, null);
			}
		}
#endif

        void Update()
        {
            Console.Update();
        }

        void OnDestroy()
        {
            Listener.Close();
            Listener = null;
        }

        void RegisterRoutes()
        {
            if (RegisteredRoutes == null)
            {
                RegisteredRoutes = new List<RouteAttribute>();

                foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
                {
                    foreach (var type in assembly.GetTypes())
                    {
                        foreach (var method in type.GetMethods(BindingFlags.Public | BindingFlags.Static))
                        {
                            var attrs = method.GetCustomAttributes(typeof(RouteAttribute), true) as RouteAttribute[];
                            if (attrs.Length == 0)
                            {
                                continue;
                            }

                            var cbm = Delegate.CreateDelegate(typeof(RouteAttribute.CallbackHandler), method, false) as RouteAttribute.CallbackHandler;
                            if (cbm == null)
                            {
                                Debug.LogError(string.Format("Method {0}.{1} takes the wrong arguments for a console route.", type, method.Name));
                                continue;
                            }

                            // try with a bare action
                            foreach (RouteAttribute route in attrs)
                            {
                                if (route.Route == null)
                                {
                                    Debug.LogError(string.Format("Method {0}.{1} needs a valid route regexp.", type, method.Name));
                                    continue;
                                }

                                route.Callback = cbm;
                                RegisteredRoutes.Add(route);
                            }
                        }
                    }
                }
                RegisterFileHandlers();
            }
        }

        static void FindFileType(RequestContext context, bool download, out string path, out string type)
        {
            path = Path.Combine(FileRoot, context.Match.Groups[1].Value);

            var ext = Path.GetExtension(path).ToLower().TrimStart(new char[] { '.' });
            if (download || !FileTypes.TryGetValue(ext, out type))
            {
                type = "application/octet-stream";
            }
        }

        static void AsyncHandleFile(RequestContext context, bool download)
        {
            string path, type;
            FindFileType(context, download, out path, out type);

            WWW req = new WWW(path);
            while (!req.isDone)
            {
                Thread.Sleep(0);
            }

            if (string.IsNullOrEmpty(req.error))
            {
                context.Response.ContentType = type;
                if (download)
                {
                    context.Response.AddHeader("Content-disposition", string.Format("attachment; filename={0}", Path.GetFileName(path)));
                }
                context.Response.WriteBytes(req.bytes);
                return;
            }

            if (req.error.StartsWith("Couldn't open file"))
            {
                context.Pass = true;
            }
            else
            {
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                context.Response.StatusDescription = string.Format("Fatal error:\n{0}", req.error);
            }
        }

        static void HandleFile(RequestContext context, bool download)
        {
            string path, type;
            FindFileType(context, download, out path, out type);

            if (File.Exists(path))
            {
                context.Response.WriteFile(path, type, download);
            }
            else
            {
                context.Pass = true;
            }
        }

        static void RegisterFileHandlers()
        {
            var pattern = string.Format("({0})", string.Join("|", FileTypes.Select(ft => ft.Key).ToArray()));
            var downloadRoute = new RouteAttribute(string.Format(@"^/download/(.*\.{0})$", pattern));
            var fileRoute = new RouteAttribute(string.Format(@"^/(.*\.{0})$", pattern));

            var needs_www = FileRoot.Contains("://");
            downloadRoute.RunOnMainThread = needs_www;
            fileRoute.RunOnMainThread = needs_www;

            FileHandler callback = HandleFile;
            if (needs_www)
            {
                callback = AsyncHandleFile;
            }

            downloadRoute.Callback = delegate (RequestContext context)
            {
                callback(context, true);
            };
            fileRoute.Callback = delegate (RequestContext context)
            {
                callback(context, false);
            };

            RegisteredRoutes.Add(downloadRoute);
            RegisteredRoutes.Add(fileRoute);
        }

        void ListenerCallback(IAsyncResult result)
        {
            RequestContext context = new RequestContext(Listener.EndGetContext(result));

            HandleRequest(context);

            if (Listener.IsListening)
            {
                Listener.BeginGetContext(ListenerCallback, null);
            }
        }

        void HandleRequest(RequestContext context)
        {
            RegisterRoutes();

            try
            {
                bool handled = false;

                for (; context.CurrentRoute < RegisteredRoutes.Count; ++context.CurrentRoute)
                {
                    var route = RegisteredRoutes[context.CurrentRoute];
                    var match = route.Route.Match(context.Path);
                    if (!match.Success)
                    {
                        continue;
                    }

                    if (!route.Methods.IsMatch(context.Request.HttpMethod))
                    {
                        continue;
                    }

                    // Upgrade to main thread if necessary
                    if (route.RunOnMainThread && Thread.CurrentThread != MainThread)
                    {
                        lock (MainRequests)
                        {
                            MainRequests.Enqueue(context);
                        }
                        return;
                    }

                    context.Match = match;
                    route.Callback(context);
                    handled = !context.Pass;
                    if (handled)
                    {
                        break;
                    }
                }

                if (!handled)
                {
                    context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                    context.Response.StatusDescription = "Not Found";
                }
            }
            catch (Exception exception)
            {
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                context.Response.StatusDescription = string.Format("Fatal error:\n{0}", exception);

                Debug.LogException(exception);
            }

            context.Response.OutputStream.Close();
        }

        IEnumerator AsyncHandleRequests()
        {
            while (true)
            {
                while (MainRequests.Count == 0)
                {
                    yield return new WaitForEndOfFrame();
                }

                RequestContext context = null;
                lock (MainRequests)
                {
                    context = MainRequests.Dequeue();
                }

                HandleRequest(context);
            }
        }
    }
}
