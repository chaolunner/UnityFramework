using System.Collections.Generic;
using UnityEngine.UI;
using UniRx.Triggers;
using UniEasy.ECS;
using UnityEngine;
using System.Linq;
using System;
using UniRx;

namespace UniEasy.Console
{
    public class DebugSystem : SystemBehaviour
    {
        public DebugSetting Setting;
        public GameObject LogPrefab;

        public IGroup DebugCanvas;
        public IGroup DebugView;
        public IGroup DebugLog;

        private const int SortingOrder = 100;
        private const string Empty = "";
        private const string LogCount = "Log ({0})";
        private const string WarningCount = "Warning ({0})";
        private const string ErrorCount = "Error ({0})";
        private const string HtmlLogStyle = "<span>{0}</span>";
        private const string HtmlWarningStyle = "<span class=\"Warning\">{0}</span>";
        private const string HtmlErrorStyle = "<span class=\"Error\">{0}</span>";
        private const string LogStyle = "<b><size={0}><color=#ffffffff>{1}</color></size></b>";
        private const string WarningStyle = "<b><size={0}><color=#ffff00ff>{1}</color></size></b>";
        private const string ErrorStyle = "<b><size={0}><color=#ff0000ff>{1}</color></size></b>";
        private static readonly Color32 MBGColor0 = new Color32(0x00, 0x00, 0x00, 0x24);
        private static readonly Color32 CBGColor0 = new Color32(0x42, 0x42, 0x42, 0x80);
        private static readonly Color32 MBGColor1 = new Color32(0x00, 0x00, 0x00, 0x48);
        private static readonly Color32 CBGColor1 = new Color32(0xA9, 0xA9, 0xA9, 0x80);

        public DebugView View { get; private set; }

        public override void Initialize(IEventSystem eventSystem, IPoolManager poolManager, GroupFactory groupFactory, PrefabFactory prefabFactory)
        {
            base.Initialize(eventSystem, poolManager, groupFactory, prefabFactory);

            DebugCanvas = this.Create(typeof(DebugCanvas), typeof(Canvas), typeof(CanvasScaler));
            DebugView = this.Create(typeof(DebugView));
            DebugLog = this.Create(typeof(DebugLog), typeof(ViewComponent));
        }

        public override void OnEnable()
        {
            base.OnEnable();

            ApplyChanges();

            DebugCanvas.OnAdd().Subscribe(entity1 =>
            {
                var debugCanvas = entity1.GetComponent<DebugCanvas>();
                var canvas = entity1.GetComponent<Canvas>();
                var canvasScaler = entity1.GetComponent<CanvasScaler>();

                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvas.sortingOrder = SortingOrder;
                canvasScaler.matchWidthOrHeight = 1;
                canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;

                DebugView.OnAdd().Subscribe(entity2 =>
                {
                    View = entity2.GetComponent<DebugView>();

                    View.EscButton.OnPointerClickAsObservable().Select(_ => false)
                        .Merge(Setting.ShowOnUGUI.DistinctUntilChanged())
                        .Subscribe(b =>
                    {
                        View.DebugPanel.gameObject.SetActive(b);
                    }).AddTo(this.Disposer).AddTo(debugCanvas.Disposer).AddTo(View.Disposer);

                    Console.RegisterCommand("Debug", "Show debug console message on UGui.", "Debug [On/Off]", (args) =>
                    {
                        bool result = args.Length == 0 ? true : args.Any(m =>
                        {
                            string cmd = m.Trim().ToLower();
                            return cmd == "on" || cmd == "true";
                        });
                        View.DebugPanel.gameObject.SetActive(result);
                        return string.Format("Debug message show on UGui is {0}", result == true ? "On" : "Off");
                    });

                    View.ClearButton.OnPointerClickAsObservable().Select(_ => true)
                        .Merge(ClearCommand.OnClearAsObservable())
                        .Subscribe(_ =>
                    {
                        View.Logs.Clear();
                    }).AddTo(this.Disposer).AddTo(debugCanvas.Disposer).AddTo(View.Disposer);

                    View.CollapseToggle.OnPointerClickAsObservable().Subscribe(_ =>
                    {
                        Setting.DebugView.Collapse.Value = View.CollapseToggle.isOn;
                    }).AddTo(this.Disposer).AddTo(debugCanvas.Disposer).AddTo(View.Disposer);

                    View.LogToggle.OnPointerClickAsObservable().Subscribe(_ =>
                    {
                        Setting.DebugView.Log.Value = View.LogToggle.isOn;
                    }).AddTo(this.Disposer).AddTo(debugCanvas.Disposer).AddTo(View.Disposer);

                    View.WarningToggle.OnPointerClickAsObservable().Subscribe(_ =>
                    {
                        Setting.DebugView.Warning.Value = View.WarningToggle.isOn;
                    }).AddTo(this.Disposer).AddTo(debugCanvas.Disposer).AddTo(View.Disposer);

                    View.ErrorToggle.OnPointerClickAsObservable().Subscribe(_ =>
                    {
                        Setting.DebugView.Error.Value = View.ErrorToggle.isOn;
                    }).AddTo(this.Disposer).AddTo(debugCanvas.Disposer).AddTo(View.Disposer);

                    var logHeight = LogPrefab.GetComponent<RectTransform>().rect.height;
                    var contentHeight = (View.OutputPanel.parent as RectTransform).rect.height;
                    var maxLogsVisible = Mathf.CeilToInt(contentHeight / logHeight);

                    for (int i = 0; i < maxLogsVisible; i++)
                    {
                        PrefabFactory.Instantiate(LogPrefab, View.OutputPanel);
                    }

                    var onCollapse = Setting.DebugView.Collapse.DistinctUntilChanged();
                    var onLog = Setting.DebugView.Log.DistinctUntilChanged();
                    var onWarning = Setting.DebugView.Warning.DistinctUntilChanged();
                    var onError = Setting.DebugView.Error.DistinctUntilChanged();
                    var onCountChanged = View.Logs.ObserveCountChanged().Select(_ => true);
                    var onScrolling = View.OutputScrollbar.OnValueChangedAsObservable().Select(_ => true);
                    var onEnabled = View.DebugPanel.OnEnableAsObservable().Select(_ => true);

                    onCollapse.Merge(onLog).Merge(onWarning).Merge(onError).Merge(onCountChanged).Merge(onScrolling).Merge(onEnabled).Where(_ => View.DebugPanel.gameObject.activeSelf).Subscribe(_ =>
                    {
                        Debugger.History.Clear();

                        var logData = new List<LogData>();
                        var logDataDict = new Dictionary<LogData, int>();
                        if (Setting.DebugView.Collapse.Value)
                        {
                            for (int i = 0; i < View.Logs.Count; i++)
                            {
                                if (!logDataDict.ContainsKey(View.Logs[i]))
                                {
                                    logDataDict.Add(View.Logs[i], 0);
                                }
                                logDataDict[View.Logs[i]]++;
                            }
                            logData = logDataDict.Keys.ToList();
                        }
                        else
                        {
                            logData = View.Logs.ToList();
                        }

                        var disabledData = new List<LogData>();
                        foreach (var data in logData)
                        {
                            if (data.LogType == LogType.Log && !Setting.DebugView.Log.Value)
                            {
                                disabledData.Add(data);
                            }
                            else if (data.LogType == LogType.Warning && !Setting.DebugView.Warning.Value)
                            {
                                disabledData.Add(data);
                            }
                            else if (data.LogType == LogType.Error && !Setting.DebugView.Error.Value)
                            {
                                disabledData.Add(data);
                            }
                        }
                        foreach (var data in disabledData)
                        {
                            logData.Remove(data);
                        }

                        foreach (var data in logData)
                        {
                            if (data.LogType == LogType.Warning)
                            {
                                Debugger.History.Add(string.Format(HtmlWarningStyle, data.Message));
                            }
                            else if (data.LogType == LogType.Error)
                            {
                                Debugger.History.Add(string.Format(HtmlErrorStyle, data.Message));
                            }
                            else
                            {
                                Debugger.History.Add(string.Format(HtmlLogStyle, data.Message));
                            }
                        }

                        View.OutputPanel.sizeDelta = new Vector2(View.OutputPanel.sizeDelta.x, logData.Count * logHeight);
                        int index = Mathf.FloorToInt(Mathf.Clamp(logData.Count - maxLogsVisible, 0, logData.Count) * (1 - View.OutputScrollbar.value));
                        foreach (var entity3 in DebugLog.Entities)
                        {
                            var view = entity3.GetComponent<ViewComponent>();
                            var debugLog = entity3.GetComponent<DebugLog>();
                            var rect = view.Transforms[0] as RectTransform;

                            rect.anchoredPosition = new Vector2(0, -index * logHeight);

                            if (index < logData.Count)
                            {
                                if (index % 2 == 0)
                                {
                                    debugLog.MessageBackground.color = MBGColor0;
                                    debugLog.CountBackground.color = CBGColor0;
                                }
                                else
                                {
                                    debugLog.MessageBackground.color = MBGColor1;
                                    debugLog.CountBackground.color = CBGColor1;
                                }
                                debugLog.LogData.Value = logData[index];
                                if (Setting.DebugView.Collapse.Value)
                                {
                                    debugLog.Count.Value = logDataDict[logData[index]];
                                }
                                else
                                {
                                    debugLog.Count.Value = 1;
                                }
                                debugLog.Index = index;
                                view.Transforms[0].gameObject.SetActive(true);
                            }
                            else
                            {
                                view.Transforms[0].gameObject.SetActive(false);
                            }
                            index++;
                        }

                        View.LogText.text = string.Format(LogCount, logData.Where(data => data.LogType == LogType.Log).Count());
                        View.WarningText.text = string.Format(WarningCount, logData.Where(data => data.LogType == LogType.Warning).Count());
                        View.ErrorText.text = string.Format(ErrorCount, logData.Where(data => data.LogType == LogType.Error).Count());

                        View.CollapseToggle.isOn = Setting.DebugView.Collapse.Value;
                        View.LogToggle.isOn = Setting.DebugView.Log.Value;
                        View.WarningToggle.isOn = Setting.DebugView.Warning.Value;
                        View.ErrorToggle.isOn = Setting.DebugView.Error.Value;
                    }).AddTo(this.Disposer).AddTo(debugCanvas.Disposer).AddTo(View.Disposer);

                    View.Selected.DistinctUntilChanged().Subscribe(index =>
                    {
                        if (index < 0 || index >= View.Logs.Count)
                        {
                            View.StackTraceText.text = Empty;
                        }
                        else
                        {
                            if (View.Logs[index].LogType == LogType.Warning)
                            {
                                View.StackTraceText.text = string.Format(WarningStyle, View.Size, View.Logs[index].Message);
                            }
                            else if (View.Logs[index].LogType == LogType.Error)
                            {
                                View.StackTraceText.text = string.Format(ErrorStyle, View.Size, View.Logs[index].Message);
                            }
                            else
                            {
                                View.StackTraceText.text = string.Format(LogStyle, View.Size, View.Logs[index].Message);
                            }
                        }
                    }).AddTo(this.Disposer).AddTo(View.Disposer);

                    View.DebugPanel.gameObject.SetActive(false);
                }).AddTo(this.Disposer).AddTo(debugCanvas.Disposer);
            }).AddTo(this.Disposer);

            DebugLog.OnAdd().Subscribe(entity =>
            {
                var view = entity.GetComponent<ViewComponent>();
                var debugLog = entity.GetComponent<DebugLog>();
                var rect = view.Transforms[0] as RectTransform;

                rect.anchorMin = Vector2.up;
                rect.anchorMax = Vector2.one;
                rect.pivot = Vector2.up;

                debugLog.LogData.DistinctUntilChanged().Subscribe(data =>
                {
                    if (data.LogType == LogType.Warning)
                    {
                        debugLog.MessageText.text = string.Format(WarningStyle, View.Size, data.Message);
                    }
                    else if (data.LogType == LogType.Error)
                    {
                        debugLog.MessageText.text = string.Format(ErrorStyle, View.Size, data.Message);
                    }
                    else
                    {
                        debugLog.MessageText.text = string.Format(LogStyle, View.Size, data.Message);
                    }
                }).AddTo(this.Disposer).AddTo(debugLog.Disposer);

                debugLog.Count.DistinctUntilChanged().Subscribe(count =>
                {
                    debugLog.CountText.text = count.ToString();
                }).AddTo(this.Disposer).AddTo(debugLog.Disposer);

                view.Transforms[0].OnPointerClickAsObservable().Subscribe(_ =>
                {
                    View.Selected.Value = debugLog.Index;
                }).AddTo(this.Disposer).AddTo(View.Disposer).AddTo(debugLog.Disposer);
            }).AddTo(this.Disposer);

            Debugger.RegisterPreMatchingLayer(OnPreMatchingLayerInEditor);
            Application.logMessageReceived += HandleLog;
        }

        public override void OnDisable()
        {
            base.OnDisable();

            Debugger.DeregisterPreMatchingLayer(OnPreMatchingLayerInEditor);
            Application.logMessageReceived -= HandleLog;
        }

        private void HandleLog(string logString, string stackTrace, LogType type)
        {
            if (Setting.ShowOnUGUI.Value && View != null && View.OutputPanel != null)
            {
                var message = logString + Environment.NewLine + stackTrace;
                if (type == LogType.Exception)
                {
                    View.Logs.Add(new LogData(message, LogType.Error));
                }
                else
                {
                    View.Logs.Add(new LogData(message, type));
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
    }
}
