#if (NETFX_CORE || NET_4_6 || NET_STANDARD_2_0 || UNITY_WSA_10_0)
using System;
#else
using UniRx;
#endif

namespace UniEasy.ECS
{
    public interface IEventSystem
    {
        /// <summary>
        /// Don't use this method directly, try to use Send() method instead!
        /// </summary>
        void Publish<T>(T message);

        /// <summary>
        /// Don't use this method directly, try to use Send() method instead!
        /// </summary>
        void Publish(object message);

        /// <summary>
        /// Don't use this method directly, try to use OnEvent() method instead!
        /// </summary>
        IObservable<T> Receive<T>();

        /// <summary>
        /// Don't use this method directly, try to use OnEvent() method instead!
        /// </summary>
        IObservable<object> Receive(Type type);
    }
}
