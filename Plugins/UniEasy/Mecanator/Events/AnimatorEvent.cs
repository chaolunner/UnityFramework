using UnityEngine;

namespace UniEasy
{
    [ContextMenuAttribute("Kernel/AnimatorEvent")]
    public class AnimatorEvent : SerializableEvent, IAnimatorEvent
    {
        public Animator Animator { get; set; }
        public int LayerIndex { get; set; }
        public AnimatorState State { get; set; }
    }
}
