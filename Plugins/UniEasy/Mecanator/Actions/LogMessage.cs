using UniEasy.Console;
using UnityEngine;

namespace UniEasy
{
    public class LogMessage : StateMachineAction
    {
        public LogType Type = LogType.Log;
        public string Message;

        public override void Execute(StateMachineActionObject smao)
        {
            Debugger.Log(Type, Message, "Mecanator", null);
        }
    }
}
