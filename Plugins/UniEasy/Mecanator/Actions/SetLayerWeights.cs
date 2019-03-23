using System.Collections.Generic;
using UnityEngine;

namespace UniEasy
{
    public class SetLayerWeights : StateMachineAction
    {
        [Reorderable]
        public List<float> LayerWeights = new List<float>();

        public override void Execute(StateMachineActionObject smao)
        {
            for (var i = 0; i < LayerWeights.Count; i++)
            {
                smao.Animator.SetLayerWeight(i, Mathf.Clamp01(LayerWeights[i]));
            }
        }
    }
}
