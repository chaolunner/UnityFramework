using UnityEngine;

namespace UniEasy
{
    public enum AnimatorState
    {
        Enter,
        Exit,
        IK,
        Move,
        Update,
        NormalizedTime,
    }

    public class StateMachineActionObject
    {
        public Animator Animator;
        public AnimatorStateInfo StateInfo;
        public int LayerIndex;
        public int PathHash;
        public AnimatorState State;
    }
}
