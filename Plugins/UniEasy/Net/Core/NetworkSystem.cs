using Common;
using System;

namespace UniEasy.Net
{
    public interface INetworkSystem : IRequestPublisher, IRequestReceiver
    {
    }

    public class NetworkSystem : INetworkSystem, IDisposable
    {
        public INetworkBroker NetworkBroker { get; private set; }

        public NetworkSystem(INetworkBroker networkBroker)
        {
            NetworkBroker = networkBroker;
            NetworkBroker.Connect();
        }

        public void Publish(RequestCode requestCode, string data)
        {
            NetworkBroker.Publish(requestCode, data);
        }

        public void Receive(RequestCode requestCode, Action<string> action)
        {
            NetworkBroker.Receive(requestCode, action);
        }

        public void Dispose()
        {
            NetworkBroker.Disconnect();
        }
    }
}
