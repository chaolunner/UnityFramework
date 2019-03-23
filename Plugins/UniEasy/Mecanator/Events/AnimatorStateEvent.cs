using UnityEngine;

namespace UniEasy
{
    [ContextMenuAttribute("Kernel/AnimatorStateEvent")]
    public class AnimatorStateEvent : AnimatorEvent
    {
        public int PathHash;
        public AnimatorStateInfo StateInfo;

        public AnimatorStateEvent(Animator animator, int pathHash, AnimatorStateInfo stateInfo, int layerIndex, AnimatorState state)
        {
            Animator = animator;
            PathHash = pathHash;
            StateInfo = stateInfo;
            LayerIndex = layerIndex;
            State = state;
        }
    }
}
