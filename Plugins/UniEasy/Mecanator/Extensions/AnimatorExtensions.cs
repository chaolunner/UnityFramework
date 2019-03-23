using UnityEngine;

namespace UniEasy
{
    public static class AnimatorExtensions
    {
        public static int State(this Animator animator, string layerName)
        {
            return State(animator, animator.GetLayerIndex(layerName));
        }

        public static int State(this Animator animator, int layerIndex)
        {
            if (IsValid(animator))
            {
                return animator.GetCurrentAnimatorStateInfo(layerIndex).fullPathHash;
            }
            else
            {
                return -1;
            }
        }

        public static bool IsValid(this Animator animator)
        {
            return (animator != null && animator.runtimeAnimatorController != null && animator.gameObject.activeInHierarchy);
        }

        public static bool IsValid(this Animator animator, params int[] nameHashs)
        {
            if (!IsValid(animator))
            {
                return false;
            }
            foreach (var nameHash in nameHashs)
            {
                if (!HasParameter(animator, nameHash))
                {
                    return false;
                }
            }
            return true;
        }

        public static bool HasParameter(this Animator animator, int nameHash)
        {
            if (animator != null)
            {
                foreach (var param in animator.parameters)
                {
                    if (param.nameHash == nameHash)
                    {
                        return true;
                    }
                }
            }
            return false;
        }
    }
}
