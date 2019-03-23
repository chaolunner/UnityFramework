using UnityEngine;

namespace UniEasy
{
    public class OnStateUpdateHandler : StateMachineHandler
    {
        public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            base.OnStateUpdate(animator, stateInfo, layerIndex);

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
                State = AnimatorState.Update
            };
            foreach (var action in Actions)
            {
                action.Execute(smao);
            }
        }
    }
}
