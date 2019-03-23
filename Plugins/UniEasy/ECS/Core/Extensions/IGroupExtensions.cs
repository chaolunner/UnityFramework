#if (NETFX_CORE || NET_4_6 || NET_STANDARD_2_0 || UNITY_WSA_10_0)
using System;
#endif
using UniRx;

namespace UniEasy.ECS
{
    public static class IGroupExtensions
    {
        public static IObservable<IEntity> OnAdd(this IGroup group)
        {
            return group.Entities.ObserveAdd().Select(e => e.Value).StartWith(group.Entities);
        }

        public static IObservable<IEntity> OnRemove(this IGroup group)
        {
            return group.Entities.ObserveRemove().Select(e => e.Value);
        }
    }
}
