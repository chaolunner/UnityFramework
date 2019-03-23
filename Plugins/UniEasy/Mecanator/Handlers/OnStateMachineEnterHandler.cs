using UnityEngine;

namespace UniEasy
{
    public class OnStateMachineEnterHandler : StateMachineHandler
    {
        public override void OnStateMachineEnter(Animator animator, int stateMachinePathHash)
        {
            base.OnStateMachineEnter(animator, stateMachinePathHash);

            var smao = new StateMachineActionObject()
            {
                Animator = animator,
                PathHash = stateMachinePathHash,
                State = AnimatorState.Enter
            };
            foreach (var action in Actions)
            {
                action.Execute(smao);
            }
        }
    }
}
