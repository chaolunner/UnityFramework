using System.Collections.Generic;
using System.Net.Sockets;
using UnityEngine;
using System;
using Common;

namespace UniEasy.Net
{
    public interface IRequestPublisher
    {
        void Publish(RequestCode requestCode, string data);
    }

    public interface IRequestReceiver
    {
        void Receive(RequestCode requestCode, Action<string> action);
    }

    public interface IRequestResponser
    {
        void Response(RequestCode requestCode, string data);
    }

    public interface INetworkBroker : IRequestResponser, IRequestReceiver, IRequestPublisher
    {
        void Connect();
        void Disconnect();
    }

    public class NetworkBroker : INetworkBroker, IDisposable
    {
        public static readonly INetworkBroker Default = new NetworkBroker();

        public string Ip;
        public int Port;
        private bool isDisposed = false;
        private Socket clientSocket;
        private Message msg = new Message();
        private readonly Dictionary<RequestCode, List<Action<string>>> notifiers = new Dictionary<RequestCode, List<Action<string>>>();

        [DI.Inject]
        public NetworkBroker()
        {
            Ip = "127.0.0.1";
            Port = 8989;
        }

        public NetworkBroker(string ip, int port)
        {
            Ip = ip;
            Port = port;
        }

        public void Connect()
        {
            clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            try
            {
                clientSocket.Connect(Ip, Port);
                Start();
            }
            catch (Exception e)
            {
                Debug.LogWarning("Unable to connect to server, please check your network: " + e);
            }
        }

        private void Start()
        {
            clientSocket.BeginReceive(msg.Data, msg.StartIndex, msg.RemainSize, SocketFlags.None, ReceiveCallback, null);
        }

        private void ReceiveCallback(IAsyncResult ar)
        {
            try
            {
                int count = clientSocket.EndReceive(ar);
                msg.Process(count, Response);
                Start();
            }
            catch (Exception e)
            {
                Debug.Log(e);
            }
        }

        public void Disconnect()
        {
            try
            {
                clientSocket.Close();
            }
            catch (Exception e)
            {
                Debug.LogWarning("Unable to close connection with server: " + e);
            }
        }

        public void Publish(RequestCode requestCode, string data)
        {
            byte[] bytes = Message.Pack(requestCode, data);
            clientSocket.Send(bytes);
        }

        public void Receive(RequestCode requestCode, Action<string> action)
        {
            if (!notifiers.ContainsKey(requestCode))
            {
                notifiers.Add(requestCode, new List<Action<string>>());
            }
            notifiers[requestCode].Add(action);
        }

        public void Response(RequestCode requestCode, string data)
        {
            if (notifiers.ContainsKey(requestCode))
            {
                foreach (var action in notifiers[requestCode])
                {
                    action?.Invoke(data);
                }
            }
        }

        public void Dispose()
        {
            lock (notifiers)
            {
                if (!isDisposed)
                {
                    isDisposed = true;
                    notifiers.Clear();
                }
            }
        }
    }
}
