using System.Text.RegularExpressions;
using System;

namespace UniEasy.Console
{
    [AttributeUsage(AttributeTargets.Method)]
    public class RouteAttribute : Attribute
    {
        public Regex Route;
        public Regex Methods;
        public bool RunOnMainThread;

        public delegate void CallbackHandler(RequestContext context);

        public CallbackHandler Callback;

        public RouteAttribute(string route, string methods = @"(GET|HEAD)", bool runOnMainThread = true)
        {
            Route = new Regex(route, RegexOptions.IgnoreCase);
            Methods = new Regex(methods);
            RunOnMainThread = runOnMainThread;
        }
    }
}
