using UnityEngine;

namespace UniEasy
{
    public class SetBoolParameter : StateMachineAction
    {
        public string ParameterName;
        public bool IsOn;
        private int ParameterHash;

        private void OnEnable()
        {
            ParameterHash = Animator.StringToHash(ParameterName);
        }

        public override void Execute(StateMachineActionObject smao)
        {
            smao.Animator.SetBool(ParameterHash, IsOn);
        }
    }
}
