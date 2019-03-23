using UnityEngine;

namespace UniEasy
{
    public class DebugBreak : StateMachineAction
    {
        public override void Execute(StateMachineActionObject smao)
        {
            Debug.Break();
        }
    }
}
