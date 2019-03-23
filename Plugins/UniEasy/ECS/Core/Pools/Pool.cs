using System.Collections.Generic;
using System.Linq;
using System;

namespace UniEasy.ECS
{
    public class Pool : IPool
    {
        private readonly IList<IEntity> entities;

        public string Name { get; private set; }

        public IEnumerable<IEntity> Entities { get { return entities; } }

        public IIdentityGenerator IdentityGenerator { get; private set; }

        public IEventSystem EventSystem { get; private set; }

        public Pool(string name, IIdentityGenerator identityGenerator, IEventSystem eventSystem)
        {
            entities = new List<IEntity>();
            Name = name;
            IdentityGenerator = identityGenerator;
            EventSystem = eventSystem;
        }

        public IEntity CreateEntity()
        {
            var newId = IdentityGenerator.GenerateId();
            var entity = new Entity(newId, EventSystem);

            entities.Add(entity);
            EventSystem.Publish(new EntityAddedEvent(entity, this));

            return entity;
        }

        public void RemoveEntity(IEntity entity)
        {
            entities.Remove(entity);

            var components = entity.Components.ToArray();
            for (int i = 0; i < components.Length; i++)
            {
                if (components[i] is IDisposable)
                {
                    (components[i] as IDisposable).Dispose();
                }
            }

            EventSystem.Publish(new EntityRemovedEvent(entity, this));
        }
    }
}
