namespace UniEasy
{
    public class ExecuteDelegateAction : StateMachineAction
    {
        public DelegateAction Action;

        public override void Execute(StateMachineActionObject smao)
        {
            Action.Execute(smao);
        }
    }
}
