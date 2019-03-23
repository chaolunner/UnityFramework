using System.Text.RegularExpressions;
using UnityEngine;
using System.Net;

namespace UniEasy.Console
{
    public class RequestContext
    {
        public HttpListenerContext Context;
        public string Path;
        public Match Match;
        public bool Pass;
        public int CurrentRoute;

        public HttpListenerRequest Request { get { return Context.Request; } }

        public HttpListenerResponse Response { get { return Context.Response; } }

        public RequestContext(HttpListenerContext context)
        {
            Context = context;
            Match = null;
            Pass = false;
            Path = WWW.UnEscapeURL(context.Request.Url.AbsolutePath);
            if (Path == "/")
            {
                Path = "/console.html";
            }
            CurrentRoute = 0;
        }
    }
}
