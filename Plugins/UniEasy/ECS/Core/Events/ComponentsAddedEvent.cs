namespace UniEasy.ECS
{
    public class ComponentsAddedEvent
    {
        public IEntity Entity { get; private set; }

        public object[] Components { get; private set; }

        public ComponentsAddedEvent(IEntity entity, params object[] components)
        {
            Entity = entity;
            Components = components;
        }
    }
}
