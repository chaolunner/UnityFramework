using UnityEngine;

namespace UniEasy
{
    public abstract class StateMachineAction : ScriptableObject
    {
        public abstract void Execute(StateMachineActionObject smao);
    }
}
