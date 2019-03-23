using UniEasy.ECS;
using UniEasy.DI;

namespace UniEasy
{
    public class PublishState : StateMachineAction
    {
        protected IEventSystem EventSystem
        {
            get
            {
                if (eventSystem == null)
                {
                    eventSystem = ProjectContext.ProjectContainer.Resolve<IEventSystem>();
                }
                return eventSystem;
            }
        }

        private IEventSystem eventSystem;

        public override void Execute(StateMachineActionObject smao)
        {
            var animatorStateEvent = new AnimatorStateEvent(smao.Animator, smao.PathHash, smao.StateInfo, smao.LayerIndex, smao.State);
            EventSystem.Publish(animatorStateEvent);
        }
    }
}
