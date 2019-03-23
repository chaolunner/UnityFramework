#if (NETFX_CORE || NET_4_6 || NET_STANDARD_2_0 || UNITY_WSA_10_0)
using System;
#endif
using UniRx;

namespace UniEasy.ECS
{
    public class EventSystem : IEventSystem
    {
        public IMessageBroker MessageBroker { get; private set; }

        public EventSystem(IMessageBroker messageBroker)
        {
            MessageBroker = messageBroker;
        }

        public void Publish<T>(T message)
        {
            MessageBroker.Publish(message);
        }

        public void Publish(object message)
        {
            MessageBroker.Publish(message);
        }

        public IObservable<T> Receive<T>()
        {
            return MessageBroker.Receive<T>();
        }

        public IObservable<object> Receive(Type type)
        {
            return MessageBroker.Receive(type);
        }
    }
}
