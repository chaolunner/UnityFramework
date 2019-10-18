using System.Collections.Generic;
using UnityEngine;

namespace UniEasy
{
    public class StateMachineHandler : StateMachineBehaviour
    {
        [Reorderable, DropdownMenu(typeof(StateMachineAction)), BackgroundColor("#00008080"), ObjectReference]
        public List<StateMachineAction> Actions = new List<StateMachineAction>();
        public bool IgnoreWeight = false;
    }
}
