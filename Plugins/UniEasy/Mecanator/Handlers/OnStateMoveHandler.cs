using UnityEngine;

namespace UniEasy
{
    public class OnStateMoveHandler : StateMachineHandler
    {
        public override void OnStateMove(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            base.OnStateMove(animator, stateInfo, layerIndex);

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
                State = AnimatorState.Move
            };
            foreach (var action in Actions)
            {
                action.Execute(smao);
            }
        }
    }
}
