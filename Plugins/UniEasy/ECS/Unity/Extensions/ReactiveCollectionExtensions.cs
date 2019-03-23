#if (NETFX_CORE || NET_4_6 || NET_STANDARD_2_0 || UNITY_WSA_10_0)
using System;
#endif
using UniRx;

namespace UniEasy.ECS
{
    public static class ReactiveCollectionExtensions
    {
        public static IObservable<T> OnAdd<T>(this ReactiveCollection<T> collection)
        {
            return collection.ObserveAdd().Select(source => source.Value).StartWith(collection);
        }

        public static IObservable<T> OnRemove<T>(this ReactiveCollection<T> collection)
        {
            return collection.ObserveRemove().Select(source => source.Value);
        }
    }
}
