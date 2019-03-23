using UnityEngine;

namespace UniEasy
{
    public class OnStateNormalizedTimeHandler : StateMachineHandler
    {
        public float NormalizedTime;
        public bool IsLooped;
        private bool emit = false;

        public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            base.OnStateUpdate(animator, stateInfo, layerIndex);

            if (!IgnoreWeight && layerIndex > 0 && animator.GetLayerWeight(layerIndex) <= 0)
            {
                return;
            }

            var normalizedTime = IsLooped ? stateInfo.normalizedTime % 1 : stateInfo.normalizedTime;
            if (emit)
            {
                if (normalizedTime >= NormalizedTime)
                {
                    emit = false;

                    var smao = new StateMachineActionObject() { Animator = animator, PathHash = stateInfo.fullPathHash, StateInfo = stateInfo, LayerIndex = layerIndex, State = AnimatorState.NormalizedTime };
                    foreach (var action in Actions)
                    {
                        action.Execute(smao);
                    }
                }
            }
            else
            {
                if (normalizedTime < NormalizedTime)
                {
                    emit = true;
                }
            }
        }
    }
}
