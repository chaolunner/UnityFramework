using Common;
using System;

namespace UniEasy.Net
{
    public interface INetworkSystem : IRequestPublisher, IRequestReceiver, IDisposable
    {
        SessionMode Mode { get; set; }
    }

    public class NetworkSystem : INetworkSystem
    {
        public INetworkBroker NetworkBroker { get; private set; }

        public SessionMode Mode
        {
            get { return NetworkBroker.Mode; }
            set { NetworkBroker.Mode = value; }
        }

        public NetworkSystem(INetworkBroker networkBroker)
        {
            NetworkBroker = networkBroker;
            NetworkBroker.Connect();
        }

        public void Publish<T>(RequestCode requestCode, T data)
        {
            NetworkBroker.Publish(requestCode, data);
        }

        public IActionSubject<ReceiveData> Receive(RequestCode requestCode)
        {
            return NetworkBroker.Receive(requestCode);
        }

        public void Dispose()
        {
            NetworkBroker.Disconnect();
        }
    }
}
