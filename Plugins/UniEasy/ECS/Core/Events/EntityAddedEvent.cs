namespace UniEasy.ECS
{
    public class EntityAddedEvent
    {
        public IEntity Entity { get; private set; }

        public IPool Pool { get; private set; }

        public EntityAddedEvent(IEntity entity, IPool pool)
        {
            Entity = entity;
            Pool = pool;
        }
    }
}
