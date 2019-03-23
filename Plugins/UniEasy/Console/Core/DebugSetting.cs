using UnityEngine;
using System;
using UniRx;

namespace UniEasy.Console
{
    [Serializable]
    public struct DebugViewSetting
    {
        public BoolReactiveProperty Collapse;
        public BoolReactiveProperty Log;
        public BoolReactiveProperty Warning;
        public BoolReactiveProperty Error;
    }

    [Serializable]
    public class DebugSetting : ScriptableObject
    {
        public BoolReactiveProperty IsLogEnabled = new BoolReactiveProperty(true);
        public BoolReactiveProperty ShowOnUGUI = new BoolReactiveProperty(true);
        public DebugViewSetting DebugView = new DebugViewSetting();
        public DebugMask DebugMask = new DebugMask();
    }
}
