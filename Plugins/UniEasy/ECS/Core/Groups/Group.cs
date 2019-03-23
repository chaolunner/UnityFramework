using System.Collections.Generic;
using UniEasy.DI;
using System;
using UniRx;

namespace UniEasy.ECS
{
    public class Group : IGroup, IDisposableContainer
    {
        public IEventSystem EventSystem { get; set; }

        public IPool EntityPool { get; set; }

        public string Name { get; set; }

        ReactiveHashSet<IEntity> cachedEntities = new ReactiveHashSet<IEntity>();
        ReactiveCollection<IEntity> entities = new ReactiveCollection<IEntity>();

        public ReactiveCollection<IEntity> Entities
        {
            get { return entities; }
            set { entities = value; }
        }

        public Type[] Components { get; set; }

        protected List<Func<IEntity, ReactiveProperty<bool>>> Predicates { get; set; }

        protected Dictionary<IEntity, IDisposable> predicatesTable = new Dictionary<IEntity, IDisposable>();

        protected CompositeDisposable disposer = new CompositeDisposable();

        public CompositeDisposable Disposer
        {
            get { return disposer; }
            set { disposer = value; }
        }

        public Group(Type[] components, List<Func<IEntity, ReactiveProperty<bool>>> predicates)
        {
            Components = components;
            Predicates = new List<Func<IEntity, ReactiveProperty<bool>>>();
            for (int i = 0; i < predicates.Count; i++)
            {
                Predicates.Add(predicates[i]);
            }
        }

        [Inject]
        public virtual void Initialize(IEventSystem eventSystem, IPoolManager poolManager)
        {
            EventSystem = eventSystem;
            EntityPool = poolManager.GetPool();

            cachedEntities.ObserveAdd().Select(e => e.Value).Subscribe(entity =>
            {
                if (Predicates.Count == 0)
                {
                    PreAdd(entity);
                    AddEntity(entity);
                    return;
                }

                var bools = new List<ReactiveProperty<bool>>();
                for (int i = 0; i < Predicates.Count; i++)
                {
                    bools.Add(Predicates[i].Invoke(entity));
                }
                var onLatest = Observable.CombineLatest(bools.ToArray());
                var predicateDisposable = onLatest.DistinctUntilChanged().Subscribe(values =>
                {
                    for (int i = 0; i < values.Count; i++)
                    {
                        if (!values[i])
                        {
                            if (Entities.Contains(entity))
                            {
                                PreRemove(entity);
                                RemoveEntity(entity);
                            }
                            return;
                        }
                    }
                    PreAdd(entity);
                    AddEntity(entity);
                }).AddTo(this.Disposer);
                predicatesTable.Add(entity, predicateDisposable);
            }).AddTo(this.Disposer);

            cachedEntities.ObserveRemove().Select(e => e.Value).Subscribe(entity =>
            {
                if (predicatesTable.ContainsKey(entity))
                {
                    predicatesTable[entity].Dispose();
                    predicatesTable.Remove(entity);
                }
                PreRemove(entity);
                RemoveEntity(entity);
            }).AddTo(this.Disposer);

            foreach (IEntity entity in EntityPool.Entities)
            {
                if (entity.HasComponents(Components))
                {
                    cachedEntities.Add(entity);
                }
            }

            EventSystem.Receive<EntityAddedEvent>().Subscribe(evt =>
            {
                if (!cachedEntities.Contains(evt.Entity) && evt.Entity.HasComponents(Components))
                {
                    cachedEntities.Add(evt.Entity);
                }
            }).AddTo(this);

            EventSystem.Receive<EntityRemovedEvent>().Subscribe(evt =>
            {
                if (cachedEntities.Contains(evt.Entity))
                {
                    cachedEntities.Remove(evt.Entity);
                }
            }).AddTo(this);

            EventSystem.Receive<ComponentsAddedEvent>().Subscribe(evt =>
            {
                if (!cachedEntities.Contains(evt.Entity) && evt.Entity.HasComponents(Components))
                {
                    cachedEntities.Add(evt.Entity);
                }
            }).AddTo(this);

            EventSystem.Receive<ComponentsRemovedEvent>().Subscribe(evt =>
            {
                if (cachedEntities.Contains(evt.Entity))
                {
                    foreach (var component in evt.Components)
                    {
                        for (int i = 0; i < Components.Length; i++)
                        {
                            if (Components[i] == component.GetType())
                            {
                                cachedEntities.Remove(evt.Entity);
                                return;
                            }
                        }
                    }
                }
            }).AddTo(this);
        }

        protected void AddEntity(IEntity entity)
        {
            Entities.Add(entity);
        }

        protected void RemoveEntity(IEntity entity)
        {
            Entities.Remove(entity);
        }

        protected bool IsCached(IEntity entity)
        {
            return cachedEntities.Contains(entity);
        }

        public void Dispose()
        {
            foreach (var kvp in predicatesTable)
            {
                kvp.Value.Dispose();
            }
            predicatesTable.Clear();
            Disposer.Dispose();
        }

        private Subject<IEntity> onPreAdd;

        protected virtual void PreAdd(IEntity entity)
        {
            if (onPreAdd != null)
            {
                onPreAdd.OnNext(entity);
            }
        }

        public IObservable<IEntity> OnPreAdd()
        {
            return onPreAdd ?? (onPreAdd = new Subject<IEntity>());
        }

        private Subject<IEntity> onPreRemove;

        protected virtual void PreRemove(IEntity entity)
        {
            if (onPreRemove != null)
            {
                onPreRemove.OnNext(entity);
            }
        }

        public IObservable<IEntity> OnPreRemove()
        {
            return onPreRemove ?? (onPreRemove = new Subject<IEntity>());
        }
    }
}
