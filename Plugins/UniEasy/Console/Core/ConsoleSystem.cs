using UnityEngine.UI;
using UniRx.Triggers;
using UniEasy.ECS;
using UnityEngine;
using System.Linq;
using System;
using UniRx;

namespace UniEasy.Console
{
    [UniEasy.ContextMenu("Console/ConsoleSystem")]
    public class ConsoleSystem : RuntimeSystem
    {
        public IGroup DebugCanvas;
        public IGroup DebugView;

        public override void Initialize(IEventSystem eventSystem, IPoolManager poolManager, GroupFactory groupFactory, PrefabFactory prefabFactory)
        {
            base.Initialize(eventSystem, poolManager, groupFactory, prefabFactory);

            DebugCanvas = this.Create(typeof(DebugCanvas));
            DebugView = this.Create(typeof(DebugView));
        }

        public override void OnEnable()
        {
            base.OnEnable();

            DebugCanvas.OnAdd().Subscribe(debugEntity =>
            {
                var debugCanvas = debugEntity.GetComponent<DebugCanvas>();

                DebugView.OnAdd().Subscribe(entity =>
                {
                    var debugView = entity.GetComponent<DebugView>();

                    debugView.OkButton.OnClickAsObservable()
                        .Merge(debugView.InputField.OnSubmitAsObservable().AsUnitObservable())
                        .Subscribe(_ =>
                        {
                            if (UnityEngine.EventSystems.EventSystem.current.alreadySelecting)
                            {
                                return;
                            }
                            if (debugView.InputField.text.Length > 0)
                            {
                                Console.Run(debugView.InputField.text);
                                debugView.OutputScrollbar.value = 0;
                                debugView.InputField.MoveTextStart(false);
                                debugView.InputField.text = "";
                                debugView.InputField.MoveTextEnd(false);
                            }
                            debugView.InputField.ActivateInputField();
                        }).AddTo(this.Disposer).AddTo(debugCanvas.Disposer).AddTo(debugView.Disposer);

                    debugView.ClearButton.OnClickAsObservable().Select(_ => true)
                        .Merge(ClearCommand.OnClearAsObservable())
                        .Subscribe(_ =>
                        {
                            debugView.InputField.text = "";
                            debugView.InputField.ActivateInputField();
                        }).AddTo(this.Disposer).AddTo(debugCanvas.Disposer).AddTo(debugView.Disposer);

                    var clickStream = Observable.EveryUpdate().Where(_ => Input.anyKeyDown);

                    clickStream.Buffer(clickStream.Throttle(TimeSpan.FromMilliseconds(250)))
                        .Where(c => c.Count >= 20).AsUnitObservable()
                        .Merge(clickStream.Where(_ => Input.GetKeyDown(KeyCode.BackQuote)).AsUnitObservable())
                        .Subscribe(_ =>
                        {
                            debugView.DebugPanel.gameObject.SetActive(!debugView.DebugPanel.gameObject.activeSelf);
                            if (debugView.DebugPanel.gameObject.activeSelf)
                            {
                                debugView.InputField.ActivateInputField();
                                debugView.InputField.text = "";
                            }
                        }).AddTo(this.Disposer).AddTo(debugCanvas.Disposer).AddTo(debugView.Disposer);

                    clickStream.Subscribe(_ =>
                    {
                        if (Input.GetKeyDown(KeyCode.Escape))
                        {
                            debugView.DebugPanel.gameObject.SetActive(false);
                        }
                        else if (Input.GetKeyDown(KeyCode.UpArrow))
                        {
                            var navigatedToInput = Console.InputHistory.Navigate(true);
                            debugView.InputField.MoveTextStart(false);
                            debugView.InputField.text = navigatedToInput;
                            debugView.InputField.MoveTextEnd(false);
                        }
                        else if (Input.GetKeyDown(KeyCode.DownArrow))
                        {
                            var navigatedToInput = Console.InputHistory.Navigate(false);
                            debugView.InputField.MoveTextStart(false);
                            debugView.InputField.text = navigatedToInput;
                            debugView.InputField.MoveTextEnd(false);
                        }
                        else if (Input.GetKeyDown(KeyCode.Tab))
                        {
                            debugView.InputField.MoveTextStart(false);
                            debugView.InputField.text = Console.Complete(debugView.InputField.text);
                            debugView.InputField.MoveTextEnd(false);
                        }
                    }).AddTo(this.Disposer).AddTo(debugCanvas.Disposer).AddTo(debugView.Disposer);

                    debugView.DebugPanel.gameObject.SetActive(false);
                }).AddTo(this.Disposer).AddTo(debugCanvas.Disposer);
            }).AddTo(this.Disposer);

            Console.RegisterLog(OnLog);
        }

        public override void OnDisable()
        {
            base.OnDisable();

            Console.DeregisterLog(OnLog);
        }

        void OnLog(string[] args)
        {
            Debugger.Log(args.FirstOrDefault());
        }
    }
}
