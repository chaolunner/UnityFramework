using UnityEngine;

namespace UniEasy
{
    public class SetIntParameter : StateMachineAction
    {
        public string ParameterName;
        [MinMaxRange]
        public RangedInt RangedValue;
        private int ParameterHash;

        private void OnEnable()
        {
            ParameterHash = Animator.StringToHash(ParameterName);
        }

        public override void Execute(StateMachineActionObject smao)
        {
            var value = Random.Range(RangedValue.Min, RangedValue.Max);
            smao.Animator.SetInteger(ParameterHash, value);
        }
    }
}
