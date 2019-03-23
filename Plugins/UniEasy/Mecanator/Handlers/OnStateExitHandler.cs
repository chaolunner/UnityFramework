using UnityEngine;

namespace UniEasy
{
    public class OnStateExitHandler : StateMachineHandler
    {
        public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            base.OnStateExit(animator, stateInfo, layerIndex);

            if (!IgnoreWeight && layerIndex > 0 && animator.GetLayerWeight(layerIndex) <= 0)
            {
                return;
            }

            var smao = new StateMachineActionObject()
            {
                Animator = animator,
                PathHash = stateInfo.fullPathHash,
                StateInfo = stateInfo,
                LayerIndex = layerIndex,
                State = AnimatorState.Exit
            };
            foreach (var action in Actions)
            {
                action.Execute(smao);
            }
        }
    }
}
