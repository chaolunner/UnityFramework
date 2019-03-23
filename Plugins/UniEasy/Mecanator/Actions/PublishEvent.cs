using System.Collections.Generic;
using UniEasy.ECS;
using UniEasy.DI;

namespace UniEasy
{
    public class PublishEvent : StateMachineAction
    {
        public IdentificationObject Identifier;
        [Reorderable(elementName: null), DropdownMenu(typeof(IAnimatorEvent))]
        public List<InspectableObjectData> Events;

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
            foreach (var evt in Events)
            {
                var message = evt.CreateInstance(false);
                var serializableEvent = message as ISerializableEvent;
                var animatorEvent = message as IAnimatorEvent;

                if (Identifier)
                {
                    serializableEvent.Source = Identifier;
                }
                else
                {
                    serializableEvent.Source = smao.Animator.gameObject;
                }
                animatorEvent.Animator = smao.Animator;
                animatorEvent.LayerIndex = smao.LayerIndex;
                animatorEvent.State = smao.State;
                EventSystem.Publish(message);
            }
        }
    }
}
