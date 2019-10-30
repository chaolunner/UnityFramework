using UniEasy.ECS;
#if (NETFX_CORE || NET_4_6 || NET_STANDARD_2_0 || UNITY_WSA_10_0)
using System;
#endif
using UniRx;

namespace UniEasy.ECS
{
    public static partial class IEventSystemExtensions
    {
        public static void Send<T>(this IEventSystem eventSystem, T message)
        {
            if (typeof(T) == typeof(object) || typeof(ISerializableEvent).IsAssignableFrom(typeof(T)))
            {
                eventSystem.Publish((object)message);
            }
            else
            {
                eventSystem.Publish(message);
            }
        }

        public static IObservable<T> OnEvent<T>(this IEventSystem eventSystem, bool onlyOnMainThread = false)
        {
            IObservable<T> result = null;
            if (typeof(T) == typeof(object) || typeof(ISerializableEvent).IsAssignableFrom(typeof(T)))
            {
                result = eventSystem.Receive(typeof(T)).Select(evt => (T)evt);
            }
            else
            {
                result = eventSystem.Receive<T>();
            }
            if (onlyOnMainThread)
            {
                result = result.ObserveOnMainThread();
            }
            return result;
        }
    }
}
