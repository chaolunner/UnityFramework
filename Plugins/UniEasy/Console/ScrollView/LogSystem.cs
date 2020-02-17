using UniRx.Triggers;
using UnityEngine.UI;
using UniEasy.ECS;
using UnityEngine;
using System.Linq;
using System;
using UniRx;

namespace UniEasy.Console
{

    public class LogSystem : SystemBehaviour
    {
        public DebugSetting Setting;
        public GameObject LogElementPrefab;
        public Canvas Canvas;
        public CanvasScaler CanvasScaler;
        public LogView LogView;

        private IGroup LogElementComponents;

        private const int SortingOrder = 100;
        private const int Size = 14;
        private const string One = "1";
        private const string LogCount = "Log ({0})";
        private const string WarningCount = "Warning ({0})";
        private const string ErrorCount = "Error ({0})";
        private const string HtmlLogStyle = "<span>{0}</span>";
        private const string HtmlWarningStyle = "<span class=\"Warning\">{0}</span>";
        private const string HtmlErrorStyle = "<span class=\"Error\">{0}</span>";
        private const string LogStyle = "<b><size={0}><color=#ffffffff>{1}</color></size></b>";
        private const string WarningStyle = "<b><size={0}><color=#ffff00ff>{1}</color></size></b>";
        private const string ErrorStyle = "<b><size={0}><color=#ff0000ff>{1}</color></size></b>";

        public override void Initialize(IEventSystem eventSystem, IPoolManager poolManager, GroupFactory groupFactory, PrefabFactory prefabFactory)
        {
            base.Initialize(eventSystem, poolManager, groupFactory, prefabFactory);
            LogElementComponents = this.Create(typeof(LogElement), typeof(ViewComponent));
        }

        public override void OnEnable()
        {
            base.OnEnable();

            Canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            Canvas.sortingOrder = SortingOrder;
            CanvasScaler.matchWidthOrHeight = 1;
            CanvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;

            LogView.EscButton.OnPointerClickAsObservable().Select(_ => false)
                .Merge(Setting.ShowOnUGUI.DistinctUntilChanged())
                .Subscribe(b =>
            {
                LogView.gameObject.SetActive(b);
            }).AddTo(this.Disposer).AddTo(Canvas).AddTo(LogView);

            Console.RegisterCommand("Debug", "Show debug console message on UGui.", "Debug [On/Off]", (args) =>
            {
                bool result = args.Length == 0 ? true : args.Any(m =>
                {
                    string cmd = m.Trim().ToLower();
                    return cmd == "on" || cmd == "true";
                });
                LogView.gameObject.SetActive(result);
                return string.Format("Debug message show on UGui is {0}", result == true ? "On" : "Off");
            });

            LogView.ClearButton.OnPointerClickAsObservable().Select(_ => true)
                .Merge(ClearCommand.OnClearAsObservable())
                .Subscribe(_ =>
            {
                LogView.Data.Clear();
            }).AddTo(this.Disposer).AddTo(Canvas).AddTo(LogView);

            LogView.CollapseToggle.OnPointerClickAsObservable().Subscribe(_ =>
            {
                Setting.DebugView.Collapse.Value = LogView.CollapseToggle.isOn;
            }).AddTo(this.Disposer).AddTo(Canvas).AddTo(LogView);

            LogView.LogToggle.OnPointerClickAsObservable().Subscribe(_ =>
            {
                Setting.DebugView.Log.Value = LogView.LogToggle.isOn;
            }).AddTo(this.Disposer).AddTo(Canvas).AddTo(LogView);

            LogView.WarningToggle.OnPointerClickAsObservable().Subscribe(_ =>
            {
                Setting.DebugView.Warning.Value = LogView.WarningToggle.isOn;
            }).AddTo(this.Disposer).AddTo(Canvas).AddTo(LogView);

            LogView.ErrorToggle.OnPointerClickAsObservable().Subscribe(_ =>
            {
                Setting.DebugView.Error.Value = LogView.ErrorToggle.isOn;
            }).AddTo(this.Disposer).AddTo(Canvas).AddTo(LogView);

            CreateElements();

            var onCollapse = Setting.DebugView.Collapse.DistinctUntilChanged();
            var onLog = Setting.DebugView.Log.DistinctUntilChanged();
            var onWarning = Setting.DebugView.Warning.DistinctUntilChanged();
            var onError = Setting.DebugView.Error.DistinctUntilChanged();
            var onCountChanged = LogView.Data.ObserveEveryValueChanged(data => data.Count).Select(_ => true);
            var onScrolling = LogView.ContentScrollbar.OnValueChangedAsObservable().Select(_ => true);
            var onEnabled = LogView.OnEnableAsObservable().Select(_ => true);

            onCollapse.Merge(onLog).Merge(onWarning).Merge(onError).Merge(onCountChanged).Merge(onScrolling).Merge(onEnabled)
                .Where(_ => LogView.gameObject.activeSelf)
                .Subscribe(_ =>
            {
                Debugger.History.Clear();
                LogView.SortedData.Clear();
                LogView.SameDataCountDict.Clear();

                if (Setting.DebugView.Collapse.Value)
                {
                    for (int i = 0; i < LogView.Data.Count; i++)
                    {
                        if (!LogView.SameDataCountDict.ContainsKey(LogView.Data[i]))
                        {
                            LogView.SameDataCountDict.Add(LogView.Data[i], 0);
                        }
                        LogView.SameDataCountDict[LogView.Data[i]]++;
                    }
                    LogView.SortedData = LogView.SameDataCountDict.Keys.ToList();
                }
                else
                {
                    LogView.SortedData = LogView.Data.ToList();
                }

                bool b;
                for (int i = 0; i < LogView.SortedData.Count; i++)
                {
                    b = true;
                    if (LogView.SortedData[i].LogType == LogType.Log && !Setting.DebugView.Log.Value) { }
                    else if (LogView.SortedData[i].LogType == LogType.Warning && !Setting.DebugView.Warning.Value) { }
                    else if (LogView.SortedData[i].LogType == LogType.Error && !Setting.DebugView.Error.Value) { }
                    else { b = false; }
                    if (b)
                    {
                        LogView.SortedData.RemoveAt(i);
                        i--;
                    }
                }

                for (int i = 0; i < LogView.SortedData.Count; i++)
                {
                    if (LogView.SortedData[i].LogType == LogType.Warning)
                    {
                        Debugger.History.Add(string.Format(HtmlWarningStyle, LogView.SortedData[i].Message));
                    }
                    else if (LogView.SortedData[i].LogType == LogType.Error)
                    {
                        Debugger.History.Add(string.Format(HtmlErrorStyle, LogView.SortedData[i].Message));
                    }
                    else
                    {
                        Debugger.History.Add(string.Format(HtmlLogStyle, LogView.SortedData[i].Message));
                    }
                }

                LogView.Scroll(LogView.SortedData.ToArray(), LogView.ContentScrollbar.value, LogView.ElementSize);

                int logCount = 0;
                int warningCount = 0;
                int errorCount = 0;
                for (int i = 0; i < LogView.SortedData.Count; i++)
                {
                    if (LogView.SortedData[i].LogType == LogType.Log)
                    { logCount++; }
                    else if (LogView.SortedData[i].LogType == LogType.Warning)
                    { warningCount++; }
                    else if (LogView.SortedData[i].LogType == LogType.Error)
                    { errorCount++; }
                }

                LogView.LogText.text = string.Format(LogCount, logCount);
                LogView.WarningText.text = string.Format(WarningCount, warningCount);
                LogView.ErrorText.text = string.Format(ErrorCount, errorCount);

                LogView.CollapseToggle.isOn = Setting.DebugView.Collapse.Value;
                LogView.LogToggle.isOn = Setting.DebugView.Log.Value;
                LogView.WarningToggle.isOn = Setting.DebugView.Warning.Value;
                LogView.ErrorToggle.isOn = Setting.DebugView.Error.Value;
            }).AddTo(this.Disposer).AddTo(Canvas).AddTo(LogView);

            LogElementComponents.OnAdd().Subscribe(entity =>
            {
                var logElement = entity.GetComponent<LogElement>();
                var viewComponent = entity.GetComponent<ViewComponent>();
                var rectTransform = viewComponent.Transforms[0] as RectTransform;

                rectTransform.sizeDelta = new Vector2(rectTransform.sizeDelta.x, LogView.ElementSize);

                logElement.OnPointerClickAsObservable().Subscribe(_ =>
                {
                    var data = logElement.GetData();
                    if (data.LogType == LogType.Warning)
                    {
                        LogView.StackTraceText.text = string.Format(WarningStyle, Size, data.Message);
                    }
                    else if (data.LogType == LogType.Error)
                    {
                        LogView.StackTraceText.text = string.Format(ErrorStyle, Size, data.Message);
                    }
                    else
                    {
                        LogView.StackTraceText.text = string.Format(LogStyle, Size, data.Message);
                    }
                }).AddTo(this.Disposer).AddTo(Canvas).AddTo(LogView).AddTo(viewComponent.Disposer);
            }).AddTo(this.Disposer);

            Debugger.RegisterPreMatchingLayer(OnPreMatchingLayerInEditor);
            Application.logMessageReceived += HandleLog;
            LogView.OnScroll += OnScroll;
        }

        public override void OnDisable()
        {
            base.OnDisable();

            Debugger.DeregisterPreMatchingLayer(OnPreMatchingLayerInEditor);
            Application.logMessageReceived -= HandleLog;
            LogView.OnScroll -= OnScroll;
        }

        private void HandleLog(string logString, string stackTrace, LogType type)
        {
            if (Setting.ShowOnUGUI.Value && LogView != null && LogView.Content != null)
            {
                var message = logString + Environment.NewLine + stackTrace;
                if (type == LogType.Exception)
                {
                    LogView.Data.Add(new LogData(message, LogType.Error));
                }
                else
                {
                    LogView.Data.Add(new LogData(message, type));
                }
            }
        }

        private void OnPreMatchingLayerInEditor(string layerName)
        {
#if UNITY_EDITOR
            // For the performance consideration :
            // We can't auto add a new layer when debug happening in every platform
            // So we only add a new layer when playing in editor
            if (!Setting.DebugMask.ToHashSet().Select(mask => mask.LayerName).Contains(layerName))
            {
                Setting.DebugMask.AddMask(true, layerName);
                ApplyChanges();
                UnityEditor.EditorUtility.SetDirty(Setting);
            }
#endif
        }

        private void ApplyChanges()
        {
            var masks = Setting.DebugMask.ToHashSet();
            var layers = new ReactiveCollection<string>();
            foreach (var mask in masks)
            {
                if (mask.IsEnable)
                {
                    layers.Add(mask.LayerName);
                }
            }
            Debugger.IsLogEnabled = Setting.IsLogEnabled.Value;
            Debugger.SetLayerMask(layers.ToArray());
        }

        private void CreateElements()
        {
            for (int i = 0; i < LogView.GetElementCount(); i++)
            {
                var go = PrefabFactory.Instantiate(LogElementPrefab, LogView.Content);
                LogView.Elements.Add(go.GetComponent<LogElement>());
            }
        }

        private void OnScroll(LogElement element, int index, LogData data, float elementSize, int constraintCount, bool visible)
        {
            if (visible)
            {
                if (Setting.DebugView.Collapse.Value)
                {
                    element.CountText.text = LogView.SameDataCountDict[data].ToString();
                }
                else
                {
                    element.CountText.text = One;
                }
            }
        }
    }
}