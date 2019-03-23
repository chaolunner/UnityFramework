using UnityEngine.UI;
using UniRx.Triggers;
using UniEasy.ECS;
using UnityEngine;
using System.Linq;
using System;
using UniRx;

namespace UniEasy.Console
{
    [UniEasy.ContextMenu("Console/DebugSystem")]
    public class DebugSystem : RuntimeSystem
    {
        public DebugSetting Setting;
        public GameObject LogPrefab;

        public IGroup DebugCanvas;
        public IGroup DebugView;
        public IGroup DebugLog;

        private static int SortingOrder = 100;
        private static string Empty = "";
        private static string LogCount = "Log ({0})";
        private static string WarningCount = "Warning ({0})";
        private static string ErrorCount = "Error ({0})";
        private static string HtmlLogStyle = "<span>{0}</span>";
        private static string HtmlWarningStyle = "<span class=\"Warning\">{0}</span>";
        private static string HtmlErrorStyle = "<span class=\"Error\">{0}</span>";
        private static string LogStyle = "<b><size={0}><color=#ffffffff>{1}</color></size></b>";
        private static string WarningStyle = "<b><size={0}><color=#ffff00ff>{1}</color></size></b>";
        private static string ErrorStyle = "<b><size={0}><color=#ff0000ff>{1}</color></size></b>";
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

            DebugCanvas.OnAdd().Subscribe(entity =>
            {
                var debugCanvas = entity.GetComponent<DebugCanvas>();
                var canvas = entity.GetComponent<Canvas>();
                var canvasScaler = entity.GetComponent<CanvasScaler>();

                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvas.sortingOrder = SortingOrder;
                canvasScaler.matchWidthOrHeight = 1;
                canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;

                DebugView.OnAdd().Subscribe(viewEntity =>
                {
                    View = viewEntity.GetComponent<DebugView>();

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
                            while (View.Logs.Count > 0)
                            {
                                GameObject.Destroy(View.Logs[0].GetComponent<ViewComponent>().Transforms[0].gameObject);
                                View.Logs.RemoveAt(0);
                            }
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

                    var onCollapse = Setting.DebugView.Collapse.DistinctUntilChanged();
                    var onLog = Setting.DebugView.Log.DistinctUntilChanged();
                    var onWarning = Setting.DebugView.Warning.DistinctUntilChanged();
                    var onError = Setting.DebugView.Error.DistinctUntilChanged();
                    var onAdd = View.Logs.ObserveAdd().Select(_ => true);
                    var onRemove = View.Logs.ObserveRemove().Select(_ => true);
                    onCollapse.Merge(onLog).Merge(onWarning).Merge(onError).Merge(onAdd).Merge(onRemove).Subscribe(_ =>
                    {
                        Debugger.History.Clear();

                        var list = new ReactiveDictionary<string, DebugLog>();
                        for (int i = 0; i < View.Logs.Count; i++)
                        {
                            var view = View.Logs[i].GetComponent<ViewComponent>();
                            var log = View.Logs[i].GetComponent<DebugLog>();
                            if (i % 2 == 0)
                            {
                                log.MessageBackground.color = MBGColor0;
                                log.CountBackground.color = CBGColor0;
                            }
                            else
                            {
                                log.MessageBackground.color = MBGColor1;
                                log.CountBackground.color = CBGColor1;
                            }
                            if (Setting.DebugView.Collapse.Value)
                            {
                                if (list.ContainsKey(log.Message.Value))
                                {
                                    view.Transforms[0].gameObject.SetActive(false);
                                    var t = list[log.Message.Value];
                                    t.Count.Value++;
                                    list[log.Message.Value] = t;
                                }
                                else
                                {
                                    view.Transforms[0].gameObject.SetActive(true);
                                    log.Count.Value = 1;
                                    list.Add(log.Message.Value, log);
                                }
                            }
                            else
                            {
                                view.Transforms[0].gameObject.SetActive(true);
                                log.Count.Value = 1;
                            }
                            if (log.LogType == LogType.Log && !Setting.DebugView.Log.Value)
                            {
                                view.Transforms[0].gameObject.SetActive(false);
                            }
                            else if (log.LogType == LogType.Warning && !Setting.DebugView.Warning.Value)
                            {
                                view.Transforms[0].gameObject.SetActive(false);
                            }
                            else if (log.LogType == LogType.Error && !Setting.DebugView.Error.Value)
                            {
                                view.Transforms[0].gameObject.SetActive(false);
                            }

                            if (view.Transforms[0].gameObject.activeSelf)
                            {
                                string str;
                                if (log.LogType == LogType.Warning)
                                {
                                    str = string.Format(HtmlWarningStyle, log.Message.Value);
                                }
                                else if (log.LogType == LogType.Error)
                                {
                                    str = string.Format(HtmlErrorStyle, log.Message.Value);
                                }
                                else
                                {
                                    str = string.Format(HtmlLogStyle, log.Message.Value);
                                }
                                Debugger.History.Add(str);
                            }
                        }

                        var activeLogs = View.Logs.Where(e => e.GetComponent<ViewComponent>().Transforms[0].gameObject.activeSelf).Select(e => e.GetComponent<DebugLog>().LogType);
                        View.LogText.text = string.Format(LogCount, activeLogs.Where(e => e == LogType.Log).Count());
                        View.WarningText.text = string.Format(WarningCount, activeLogs.Where(e => e == LogType.Warning).Count());
                        View.ErrorText.text = string.Format(ErrorCount, activeLogs.Where(e => e == LogType.Error).Count());

                        View.CollapseToggle.isOn = Setting.DebugView.Collapse.Value;
                        View.LogToggle.isOn = Setting.DebugView.Log.Value;
                        View.WarningToggle.isOn = Setting.DebugView.Warning.Value;
                        View.ErrorToggle.isOn = Setting.DebugView.Error.Value;
                    }).AddTo(this.Disposer).AddTo(debugCanvas.Disposer).AddTo(View.Disposer);

                    View.SelectedLog.DistinctUntilChanged().Subscribe(log =>
                    {
                        if (log == null)
                        {
                            View.StackTraceText.text = Empty;
                        }
                        else
                        {
                            if (log.LogType == LogType.Warning)
                            {
                                View.StackTraceText.text = string.Format(WarningStyle, View.Size, log.Message.Value);
                            }
                            else if (log.LogType == LogType.Error)
                            {
                                View.StackTraceText.text = string.Format(ErrorStyle, View.Size, log.Message.Value);
                            }
                            else
                            {
                                View.StackTraceText.text = string.Format(LogStyle, View.Size, log.Message.Value);
                            }
                        }
                        Observable.EveryEndOfFrame().FirstOrDefault().Subscribe(_ =>
                        {
                            View.StackTraceScrollbar.value = 1;
                        }).AddTo(this.Disposer).AddTo(View.Disposer);
                    }).AddTo(this.Disposer).AddTo(View.Disposer);

                    View.DebugPanel.gameObject.SetActive(false);
                }).AddTo(this.Disposer).AddTo(debugCanvas.Disposer);
            }).AddTo(this.Disposer);

            DebugLog.OnAdd().Subscribe(entity =>
            {
                var view = entity.GetComponent<ViewComponent>();
                var log = entity.GetComponent<DebugLog>();

                log.Message.DistinctUntilChanged().Subscribe(message =>
                {
                    if (log.LogType == LogType.Warning)
                    {
                        message = string.Format(WarningStyle, View.Size, message);
                    }
                    else if (log.LogType == LogType.Error)
                    {
                        message = string.Format(ErrorStyle, View.Size, message);
                    }
                    else
                    {
                        message = string.Format(LogStyle, View.Size, message);
                    }
                    log.MessageText.text = message;
                }).AddTo(this.Disposer).AddTo(log.Disposer);

                log.Count.DistinctUntilChanged().Subscribe(count =>
                {
                    log.CountText.text = count.ToString();
                }).AddTo(this.Disposer).AddTo(log.Disposer);

                view.Transforms[0].OnPointerClickAsObservable().Subscribe(_ =>
                {
                    View.SelectedLog.Value = log;
                }).AddTo(this.Disposer).AddTo(View.Disposer).AddTo(log.Disposer);
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
                var go = PrefabFactory.Instantiate(LogPrefab, View.OutputPanel);
                var entity = (go.GetComponent<EntityBehaviour>() ?? go.AddComponent<EntityBehaviour>()).Entity;
                var view = entity.GetComponent<ViewComponent>();
                var log = entity.GetComponent<DebugLog>();
                log.LogType = type;
                log.Message.Value = message.ToString();
                log.Count.Value = 1;
                View.Logs.Add(entity);
                view.Transforms[0].OnDisableAsObservable().Where(_ => View.SelectedLog.Value == log).Subscribe(_ =>
                {
                    View.SelectedLog.Value = null;
                }).AddTo(this.Disposer).AddTo(View.Disposer);
                Observable.EveryEndOfFrame().FirstOrDefault().Subscribe(_ =>
                {
                    View.OutputScrollbar.value = 0;
                }).AddTo(this.Disposer).AddTo(View.Disposer);
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
