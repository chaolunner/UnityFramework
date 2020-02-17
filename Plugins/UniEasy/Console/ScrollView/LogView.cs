using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

namespace UniEasy.Console
{
    public class LogView : FastScrollView<LogElement, LogData>
    {
        public RectTransform Content;
        public InputField InputField;
        public Text StackTraceText;
        [Header("Scrollbar")]
        public Scrollbar ContentScrollbar;
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

        public List<LogData> SortedData = new List<LogData>();
        public Dictionary<LogData, int> SameDataCountDict = new Dictionary<LogData, int>();

        public override int GetElementCount()
        {
            return Mathf.CeilToInt((Content.parent as RectTransform).rect.height / ElementSize) * ConstraintCount;
        }

        public override void SetContentSize(float value)
        {
            Content.sizeDelta = new Vector2(Content.sizeDelta.x, value);
        }
    }
}
