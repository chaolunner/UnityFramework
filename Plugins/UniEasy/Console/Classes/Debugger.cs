using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

namespace UniEasy.Console
{
    public class Debugger
    {
        private DebugOutputHistory DebugOutputHistory = new DebugOutputHistory(OutputHistoryCapacity);
        private DebugMask DebugMask;
        private bool LogEnabled;

        private static int OutputHistoryCapacity = 10000;
        private const string Default = "Default";

        public delegate void PreMatchingLayerHandler(string name);

        private event PreMatchingLayerHandler OnPreMatchingLayer;

        private static Debugger instance;

        public static Debugger Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new Debugger();
                }
                return instance;
            }
        }

        public static bool IsLogEnabled
        {
            get
            {
                return Instance.LogEnabled;
            }
            set
            {
                Instance.LogEnabled = value;
            }
        }

        public static DebugOutputHistory History
        {
            get
            {
                return Instance.DebugOutputHistory;
            }
        }

        public Debugger()
        {
            DebugMask = new DebugMask(Default);
        }

        public static bool IsLogLayerAllowed(string layerName)
        {
            if (IsLogEnabled && GetLayerMask().Contains(layerName))
            {
                return true;
            }
            return false;
        }

        private static HashSet<string> layerMask = new HashSet<string>();

        public static HashSet<string> GetLayerMask()
        {
            foreach (var layer in Instance.DebugMask.ToHashSet())
            {
                if (!layerMask.Contains(layer.LayerName))
                {
                    layerMask.Add(layer.LayerName);
                }
            }
            return layerMask;
        }

        public static void SetLayerMask(params string[] layerNames)
        {
            Instance.DebugMask = new DebugMask(layerNames);
        }

        public static void AddLayerMask(params string[] layerNames)
        {
            var second = GetLayerMask();
            Instance.DebugMask = new DebugMask(layerNames.Union(second).ToArray());
        }

        public static void RemoveLayerMask(params string[] layerNames)
        {
            var origin = GetLayerMask();
            var second = origin.Intersect(layerNames);
            var values = origin.Except(second).ToArray();

            Instance.DebugMask = new DebugMask(values);
        }

        public static void Log(LogType logType, object message, string layerName, UnityEngine.Object context)
        {
            Instance.OnPreMatchingLayer(layerName);
            if (IsLogLayerAllowed(layerName))
            {
                Debug.unityLogger.Log(logType, message, context);
            }
        }

        public static void Log(object message, string layerName = Default, UnityEngine.Object context = null)
        {
            Log(LogType.Log, message, layerName, context);
        }

        public static void LogWarnning(object message, string layerName = Default, UnityEngine.Object context = null)
        {
            Log(LogType.Warning, message, layerName, context);
        }

        public static void LogError(object message, string layerName = Default, UnityEngine.Object context = null)
        {
            Log(LogType.Error, message, layerName, context);
        }

        public static void RegisterPreMatchingLayer(PreMatchingLayerHandler handler)
        {
            Instance.OnPreMatchingLayer += handler;
        }

        public static void DeregisterPreMatchingLayer(PreMatchingLayerHandler handler)
        {
            Instance.OnPreMatchingLayer -= handler;
        }
    }
}
