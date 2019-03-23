using UnityEngine;

namespace UniEasy
{
    [ContextMenuAttribute("Kernel/IAnimatorEvent")]
    public interface IAnimatorEvent
    {
        Animator Animator { get; set; }
        int LayerIndex { get; set; }
        AnimatorState State { get; set; }
    }
}
