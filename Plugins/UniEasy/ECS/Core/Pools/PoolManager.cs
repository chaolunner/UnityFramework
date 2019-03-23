using System.Collections.Generic;

namespace UniEasy.ECS
{
    public class PoolManager : IPoolManager
    {
        public static string DefaultPoolName = "default";

        private IDictionary<string, IPool> pools;

        public IEventSystem EventSystem { get; private set; }

        public IEnumerable<IPool> Pools { get { return pools.Values; } }

        public IIdentityGenerator IdentityGenerator { get; private set; }

        public PoolManager(IIdentityGenerator identityGenerator, IEventSystem eventSystem)
        {
            IdentityGenerator = identityGenerator;
            EventSystem = eventSystem;
            pools = new Dictionary<string, IPool>();
            CreatePool(DefaultPoolName);
        }

        public IPool CreatePool(string name)
        {
            var pool = new Pool(name, IdentityGenerator, EventSystem);
            pools.Add(name, pool);
            EventSystem.Publish(new PoolAddedEvent(pool));
            return pool;
        }

        public IPool GetPool(string name = null)
        {
            return pools[name ?? DefaultPoolName];
        }

        public void RemovePool(string name)
        {
            if (!pools.ContainsKey(name))
            {
                return;
            }
            var pool = pools[name];
            pools.Remove(name);
            EventSystem.Publish(new PoolRemovedEvent(pool));
        }
    }
}
