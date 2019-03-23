using UnityEngine;

namespace UniEasy
{
    public class OnStateIKHandler : StateMachineHandler
    {
        public override void OnStateIK(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            base.OnStateIK(animator, stateInfo, layerIndex);

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
                State = AnimatorState.IK
            };
            foreach (var action in Actions)
            {
                action.Execute(smao);
            }
        }
    }
}
