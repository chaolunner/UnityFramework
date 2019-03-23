using UnityEngine;

namespace UniEasy
{
    public class SetFloatParameter : StateMachineAction
    {
        public string ParameterName;
        [MinMaxRange]
        public RangedFloat RangedValue;
        private int ParameterHash;

        private void OnEnable()
        {
            ParameterHash = Animator.StringToHash(ParameterName);
        }

        public override void Execute(StateMachineActionObject smao)
        {
            var value = Random.Range(RangedValue.Min, RangedValue.Max);
            smao.Animator.SetFloat(ParameterHash, value);
        }
    }
}
