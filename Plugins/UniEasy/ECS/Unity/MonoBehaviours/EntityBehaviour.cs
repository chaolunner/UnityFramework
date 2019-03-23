using System.Collections.Generic;
using UnityEngine;
using UniEasy.DI;
using System;

namespace UniEasy.ECS
{
    public class EntityBehaviour : ComponentBehaviour
    {
        public IPool Pool
        {
            get
            {
                if (pool != null)
                {
                    return pool;
                }
                else
                {
                    return (pool = PoolManager.GetPool());
                }
            }
            set { pool = value; }
        }

        private IPool pool;

        public IEntity Entity
        {
            get
            {
                return entity == null ? (entity = Pool.CreateEntity()) : entity;
            }
            set
            {
                entity = value;
            }
        }

        private IEntity entity;

        public IPoolManager PoolManager
        {
            get
            {
                return poolManager == null ? ProjectContext.ProjectContainer.Resolve<IPoolManager>() : poolManager;
            }
            set { poolManager = value; }
        }

        private IPoolManager poolManager;

        [Reorderable(elementName: null), BackgroundColor("#00808080")]
        public UnityEngine.Object[] Components;
        [Reorderable, DropdownMenu(typeof(RuntimeComponent)), BackgroundColor("#00408080")]
        public List<InspectableObjectData> RuntimeComponents = new List<InspectableObjectData>();
        [Reorderable, DropdownMenu(typeof(ScriptableComponent)), BackgroundColor("#00008080")]
        public List<ScriptableObject> ScriptableComponents = new List<ScriptableObject>();
        [HideInInspector]
        public bool AutoUpdate = true;

        void Awake()
        {
            if (Components == null)
            {
                PreLoadingComponents();
            }

            var components = new List<object>();
            if (Components != null && Components.Length > 0)
            {
                for (int i = 0; i < Components.Length; i++)
                {
                    if (Components[i] == null)
                    {
#if UNITY_EDITOR
                        Debug.LogError(string.Format("Component on {0} is missing! [{1}]", this.gameObject.name, i));
#endif
                        continue;
                    }
                    components.Add(Components[i]);
                }
            }
            var runtimeComponents = new List<object>();
            if (RuntimeComponents != null && RuntimeComponents.Count > 0)
            {
                for (int i = 0; i < RuntimeComponents.Count; i++)
                {
                    if (RuntimeComponents[i] == null)
                    {
#if UNITY_EDITOR
                        Debug.LogError(string.Format("Runtime Component on {0} is missing! [{1}]", this.gameObject.name, i));
#endif
                        continue;
                    }
                    runtimeComponents.Add(RuntimeComponents[i].CreateInstance());
                }
            }
            components.Add(AddViewComponent(runtimeComponents));
            components.AddRange(runtimeComponents);

            if (ScriptableComponents != null && ScriptableComponents.Count > 0)
            {
                for (int i = 0; i < ScriptableComponents.Count; i++)
                {
                    if (ScriptableComponents[i] == null)
                    {
#if UNITY_EDITOR
                        Debug.LogError(string.Format("Scriptable Component on {0} is missing! [{1}]", this.gameObject.name, i));
#endif
                        continue;
                    }
                    components.Add(ScriptableComponents[i]);
                }
            }
            Entity.AddComponents(components.ToArray());
        }

        public void PreLoadingComponents()
        {
            var addedComponents = GetComponents<Component>();
            var validComponents = new Dictionary<Type, Component>();
            for (int i = 0; i < addedComponents.Length; i++)
            {
                if (addedComponents[i] == null)
                {
#if UNITY_EDITOR
                    Debug.LogError(string.Format("Component on {0} is missing!", this.gameObject.name));
#endif
                }
                else
                {
                    var type = addedComponents[i].GetType();
                    if (type != typeof(Transform) && type != typeof(RectTransform) && type != typeof(EntityBehaviour))
                    {
                        if (!validComponents.ContainsKey(type))
                        {
                            validComponents.Add(type, addedComponents[i]);
                        }
                        else
                        {
#if UNITY_EDITOR
                            var components = Entity.Components.GetEnumerator();
                            while (components.MoveNext())
                            {
                                if (components.Current as Component == addedComponents[i])
                                {
                                    Debug.LogError(string.Format("Can't add multiple identical components on {0}, this is not supported! [{1}]", this.gameObject.name, components.Current.GetType()));
                                    break;
                                }
                            }
#endif
                        }
                    }
                }
            }

            var typeSet = new HashSet<Type>();
            var list = new List<UnityEngine.Object>();
            var enumerator = validComponents.Values.GetEnumerator();
            if (Components != null && Components.Length > 0)
            {
                for (int i = 0; i < Components.Length; i++)
                {
                    if (Components[i] != null)
                    {
                        if (!typeSet.Contains(Components[i].GetType()))
                        {
                            typeSet.Add(Components[i].GetType());
                            list.Add(Components[i]);
                        }
                        else
                        {
#if UNITY_EDITOR
                            Debug.LogError(string.Format("Can't add multiple identical components on {0}, this is not supported! [{1}]", this.gameObject.name, Components[i].GetType()));
#endif
                        }
                    }
                }
            }
            while (enumerator.MoveNext())
            {
                if (enumerator.Current != null)
                {
                    if (!typeSet.Contains(enumerator.Current.GetType()))
                    {
                        typeSet.Add(enumerator.Current.GetType());
                        list.Add(enumerator.Current);
                    }
                }
            }
            Components = list.ToArray();
        }

        public override void OnDestroy()
        {
            Pool.RemoveEntity(Entity);

            base.OnDestroy();
        }

        private ViewComponent AddViewComponent(List<object> runtimeComponents)
        {
            ViewComponent viewComponent = null;
            foreach (var component in runtimeComponents)
            {
                if (component is ViewComponent)
                {
                    viewComponent = component as ViewComponent;
                    if (!viewComponent.Transforms.Contains(transform))
                    {
                        viewComponent.Transforms.Insert(0, transform);
                    }
                    runtimeComponents.Remove(component);
                    return viewComponent;
                }
            }

            viewComponent = new ViewComponent();
            viewComponent.Transforms = new List<Transform>();
            viewComponent.Transforms.Add(transform);
            return viewComponent;
        }
    }
}
