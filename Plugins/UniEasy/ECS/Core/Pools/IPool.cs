using System.Collections.Generic;

namespace UniEasy.ECS
{
    public interface IPool
    {
        string Name { get; }

        IEnumerable<IEntity> Entities { get; }

        IIdentityGenerator IdentityGenerator { get; }

        IEntity CreateEntity();

        void RemoveEntity(IEntity entity);
    }
}
