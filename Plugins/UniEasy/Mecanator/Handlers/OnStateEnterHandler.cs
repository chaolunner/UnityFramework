using UnityEngine;

namespace UniEasy
{
    public class OnStateEnterHandler : StateMachineHandler
    {
        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            base.OnStateEnter(animator, stateInfo, layerIndex);

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
                State = AnimatorState.Enter
            };
            foreach (var action in Actions)
            {
                action.Execute(smao);
            }
        }
    }
}
