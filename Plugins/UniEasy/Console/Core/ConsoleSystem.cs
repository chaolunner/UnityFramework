using UniRx.Triggers;
using UniEasy.ECS;
using UnityEngine;
using System.Linq;
using System;
using UniRx;

namespace UniEasy.Console
{
    public class ConsoleSystem : SystemBehaviour
    {
        public LogView LogView;
        private const string Empty = "";

        public override void OnEnable()
        {
            base.OnEnable();

            LogView.OkButton.OnClickAsObservable()
                .Merge(LogView.InputField.OnSubmitAsObservable().AsUnitObservable())
                .Subscribe(_ =>
            {
                if (UnityEngine.EventSystems.EventSystem.current.alreadySelecting)
                {
                    return;
                }
                if (LogView.InputField.text.Length > 0)
                {
                    Console.Run(LogView.InputField.text);
                    LogView.ContentScrollbar.value = 0;
                    LogView.InputField.MoveTextStart(false);
                    LogView.InputField.text = Empty;
                    LogView.InputField.MoveTextEnd(false);
                }
                LogView.InputField.ActivateInputField();
            }).AddTo(this.Disposer).AddTo(LogView);

            LogView.ClearButton.OnClickAsObservable().Select(_ => true)
                .Merge(ClearCommand.OnClearAsObservable())
                .Subscribe(_ =>
            {
                LogView.InputField.text = Empty;
                LogView.InputField.ActivateInputField();
            }).AddTo(this.Disposer).AddTo(LogView);

            var clickStream = Observable.EveryUpdate().Where(_ => Input.anyKeyDown);

            clickStream.Buffer(clickStream.Throttle(TimeSpan.FromMilliseconds(250)))
                .Where(c => c.Count >= 20).AsUnitObservable()
                .Merge(clickStream.Where(_ => Input.GetKeyDown(KeyCode.BackQuote)).AsUnitObservable())
                .Subscribe(_ =>
            {
                LogView.gameObject.SetActive(!LogView.gameObject.activeSelf);
                if (LogView.gameObject.activeSelf)
                {
                    LogView.InputField.ActivateInputField();
                    LogView.InputField.text = Empty;
                }
            }).AddTo(this.Disposer).AddTo(LogView);

            clickStream.Subscribe(_ =>
            {
                if (Input.GetKeyDown(KeyCode.Escape))
                {
                    LogView.gameObject.SetActive(false);
                }
                else if (Input.GetKeyDown(KeyCode.UpArrow))
                {
                    var navigatedToInput = Console.InputHistory.Navigate(true);
                    LogView.InputField.MoveTextStart(false);
                    LogView.InputField.text = navigatedToInput;
                    LogView.InputField.MoveTextEnd(false);
                }
                else if (Input.GetKeyDown(KeyCode.DownArrow))
                {
                    var navigatedToInput = Console.InputHistory.Navigate(false);
                    LogView.InputField.MoveTextStart(false);
                    LogView.InputField.text = navigatedToInput;
                    LogView.InputField.MoveTextEnd(false);
                }
                else if (Input.GetKeyDown(KeyCode.Tab))
                {
                    LogView.InputField.MoveTextStart(false);
                    LogView.InputField.text = Console.Complete(LogView.InputField.text);
                    LogView.InputField.MoveTextEnd(false);
                }
            }).AddTo(this.Disposer).AddTo(LogView);

            LogView.gameObject.SetActive(false);
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
