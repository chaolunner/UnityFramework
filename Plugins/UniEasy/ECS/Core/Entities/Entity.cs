using System.Collections.Generic;
using System;

namespace UniEasy.ECS
{
    public class Entity : IEntity
    {
        private readonly Dictionary<Type, object> components;

        public IEventSystem EventSystem { get; private set; }

        public int Id { get; private set; }

        public IEnumerable<object> Components { get { return components.Values; } }

        public Entity(int id, IEventSystem eventSystem)
        {
            Id = id;
            EventSystem = eventSystem;
            components = new Dictionary<Type, object>();
        }

        public void AddComponents(params object[] components)
        {
            foreach (var component in components)
            {
                if (component != null)
                {
                    this.components.Add(component.GetType(), component);
                }
            }
            EventSystem.Publish(new ComponentsAddedEvent(this, components));
        }

        public void AddComponent<T>() where T : class, new()
        {
            AddComponents(new T());
        }

        public void RemoveComponents(params object[] components)
        {
            int removedCount = 0;

            foreach (var component in components)
            {
                if (!this.components.ContainsKey(component.GetType()))
                {
                    continue;
                }

                if (component is IDisposable)
                {
                    (component as IDisposable).Dispose();
                }

                this.components.Remove(component.GetType());
                removedCount++;
            }
            if (removedCount > 0)
            {
                EventSystem.Publish(new ComponentsRemovedEvent(this, components));
            }
        }

        public void RemoveComponent<T>() where T : class
        {
            if (!HasComponent<T>())
            {
                return;
            }

            var component = GetComponent<T>();
            RemoveComponents(component);
        }

        public bool HasComponent<T>() where T : class
        {
            return components.ContainsKey(typeof(T));
        }

        public bool HasComponents(params Type[] componentTypes)
        {
            if (components.Count == 0)
            {
                return false;
            }

            for (int i = 0; i < componentTypes.Length; i++)
            {
                if (!components.ContainsKey(componentTypes[i]))
                {
                    return false;
                }
            }
            return true;
        }

        public T GetComponent<T>() where T : class
        {
            return components[typeof(T)] as T;
        }
    }
}
