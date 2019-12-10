using Common;
using System;

namespace UniEasy.Net
{
    public interface INetworkSystem : IRequestPublisher, IRequestReceiver, IDisposable
    {
    }

    public class NetworkSystem : INetworkSystem
    {
        public INetworkBroker NetworkBroker { get; private set; }

        public NetworkSystem(INetworkBroker networkBroker)
        {
            NetworkBroker = networkBroker;
            NetworkBroker.Connect();
        }

        public void Publish<T>(RequestCode requestCode, T data)
        {
            NetworkBroker.Publish(requestCode, data);
        }

        public IActionSubject<T> Receive<T>(RequestCode requestCode)
        {
            return NetworkBroker.Receive<T>(requestCode);
        }

        public void Dispose()
        {
            NetworkBroker.Disconnect();
        }
    }
}
