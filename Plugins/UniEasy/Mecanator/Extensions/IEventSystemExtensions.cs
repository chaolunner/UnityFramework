using UniEasy.ECS;
#if (NETFX_CORE || NET_4_6 || NET_STANDARD_2_0 || UNITY_WSA_10_0)
using System;
#endif
using UniRx;

namespace UniEasy
{
    public static partial class IEventSystemExtensions
    {
        public static IObservable<T> OnEvent<T>(this IEventSystem eventSystem)
        {
            return eventSystem.Receive(typeof(T)).Select(evt => (T)evt).ObserveOnMainThread();
        }
    }
}
