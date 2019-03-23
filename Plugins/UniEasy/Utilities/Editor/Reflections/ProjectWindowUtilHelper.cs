using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEditor;
using System;

namespace UniEasy.Editor
{
    public class ProjectWindowUtilHelper
    {
        // empty array for invoking methods using reflection
        static private readonly object[] EMPTY_ARRAY = new object[0];
        static private Dictionary<string, MethodInfo> methods = new Dictionary<string, MethodInfo>();

        static protected object CallMethod(string methodName)
        {
            MethodInfo method = null;

            // Add MethodInfo to cache
            if (!methods.ContainsKey(methodName))
            {
                var flags = BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public;

                method = typeof(ProjectWindowUtil).GetMethod(methodName, flags);

                if (method != null)
                {
                    methods[methodName] = method;
                }
                else
                {
                    Debug.LogError(string.Format("Could not find method {0}", method));
                }
            }
            else
            {
                method = methods[methodName];
            }

            if (method != null)
            {
                return method.Invoke(null, EMPTY_ARRAY);
            }
            return null;
        }

        static public string GetActiveFolderPath()
        {
            return CallMethod("GetActiveFolderPath").ToString();
        }
    }
}
