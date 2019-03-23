using System;
using UniRx;

namespace UniEasy.ECS
{
    public static class ISystemExtensions
    {
        public static IGroup Create(this ISystem system, params Type[] types)
        {
            var group = system.GroupFactory.Create(types);
            group.AddTo(system.Disposer);
            return group;
        }

        public static IGroup Create(this ISystem system, Type[] types, params Func<IEntity, ReactiveProperty<bool>>[] predicates)
        {
            var group = system.GroupFactory.WithPredicates(predicates).Create(types);
            group.AddTo(system.Disposer);
            return group;
        }
    }
}
