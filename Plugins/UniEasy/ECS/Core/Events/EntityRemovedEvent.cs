namespace UniEasy.ECS
{
    public class EntityRemovedEvent
    {
        public IEntity Entity { get; private set; }

        public IPool Pool { get; private set; }

        public EntityRemovedEvent(IEntity entity, IPool pool)
        {
            Entity = entity;
            Pool = pool;
        }
    }
}
