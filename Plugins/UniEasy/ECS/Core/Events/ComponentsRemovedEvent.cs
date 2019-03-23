namespace UniEasy.ECS
{
    public class ComponentsRemovedEvent
    {
        public IEntity Entity { get; private set; }

        public object[] Components { get; private set; }

        public ComponentsRemovedEvent(IEntity entity, params object[] components)
        {
            Entity = entity;
            Components = components;
        }
    }
}
