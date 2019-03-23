namespace UniEasy.ECS
{
    public class PoolAddedEvent
    {
        public IPool Pool { get; private set; }

        public PoolAddedEvent(IPool pool)
        {
            Pool = pool;
        }
    }
}
