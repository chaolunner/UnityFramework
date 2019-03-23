using System.Collections.Generic;
using System;

namespace UniEasy.ECS
{
    public interface IEntity
    {
        int Id { get; }

        IEnumerable<object> Components { get; }

        void AddComponents(params object[] components);

        void AddComponent<T>() where T : class, new();

        void RemoveComponents(params object[] components);

        void RemoveComponent<T>() where T : class;

        T GetComponent<T>() where T : class;

        bool HasComponent<T>() where T : class;

        bool HasComponents(params Type[] component);
    }
}
