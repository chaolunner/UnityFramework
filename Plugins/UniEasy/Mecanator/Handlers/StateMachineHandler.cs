using System.Collections.Generic;
using UnityEngine;

namespace UniEasy
{
    public class StateMachineHandler : StateMachineBehaviour
    {
        [Reorderable(elementName: null), DropdownMenu(typeof(StateMachineAction)), BackgroundColor("#00008080")]
        public List<StateMachineAction> Actions = new List<StateMachineAction>();
        public bool IgnoreWeight = false;
    }
}
