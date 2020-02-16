using System.Collections.Generic;
using UnityEngine.UI;
using UniEasy.ECS;
using UnityEngine;
using UniRx;

namespace UniEasy.Console
{
    public class DebugView : ComponentBehaviour
    {
        [Header("Panels")]
        public RectTransform DebugPanel;
        public RectTransform OutputPanel;
        [Header("Console")]
        public InputField InputField;
        [Header("StackTrace")]
        public Text StackTraceText;
        [Header("Scrollbar")]
        public Scrollbar OutputScrollbar;
        public Scrollbar StackTraceScrollbar;
        [Header("Buttons")]
        public Button EscButton;
        public Button ClearButton;
        public Button OkButton;
        [Header("Collapse")]
        public Toggle CollapseToggle;
        [Header("Log")]
        public Toggle LogToggle;
        public Text LogText;
        [Header("Warning")]
        public Toggle WarningToggle;
        public Text WarningText;
        [Header("Error")]
        public Toggle ErrorToggle;
        public Text ErrorText;
        [HideInInspector]
        public IntReactiveProperty Selected = new IntReactiveProperty(-1);
        [HideInInspector]
        public ReactiveCollection<LogData> Logs = new ReactiveCollection<LogData>();
        [HideInInspector]
        public List<LogData> SortedLogs = new List<LogData>();
        [HideInInspector]
        public readonly int Size = 14;
    }
}
