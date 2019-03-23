#if (NETFX_CORE || NET_4_6 || NET_STANDARD_2_0 || UNITY_WSA_10_0)
using System;
#else
using UniRx;
#endif

namespace UniEasy.ECS
{
    public interface IEventSystem
    {
        void Publish<T>(T message);

        void Publish(object message);

        IObservable<T> Receive<T>();

        IObservable<object> Receive(Type type);
    }
}
