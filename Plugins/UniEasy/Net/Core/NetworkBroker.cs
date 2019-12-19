using System.Collections.Generic;
using System.Net.Sockets;
using UnityEngine;
using System.Text;
using System.Net;
using System;
using Common;

namespace UniEasy.Net
{
    public interface IRequestPublisher
    {
        void Publish<T>(RequestCode requestCode, T data);
    }

    public interface IRequestReceiver
    {
        IActionSubject<T> Receive<T>(RequestCode requestCode);
    }

    public interface IRequestResponser
    {
        void Response(RequestCode requestCode, byte[] dataBytes);
    }

    public interface INetworkBroker : IRequestResponser, IRequestReceiver, IRequestPublisher
    {
        void Connect();
        void Disconnect();
        void Update();
    }

    public class NetworkBroker : INetworkBroker, IDisposable
    {
        public static readonly INetworkBroker Default = new NetworkBroker();

        public string Ip = "127.0.0.1";
        public int Port = 9663;
        private bool isDisposed = false;
        private Socket clientSocket;
        private EndPoint remoteEP = new IPEndPoint(IPAddress.Any, 0);
        private Message msg = new Message();
        private ISession session;
        private IAsyncReceive udpAsyncReceive;
        private event Action<IAsyncReceive, int> OnAsyncReceive;
        private readonly List<Action> runOnMainThread = new List<Action>();
        private readonly Dictionary<RequestCode, object> notifiers = new Dictionary<RequestCode, object>();

        [DI.Inject]
        public NetworkBroker()
        {
        }

        public NetworkBroker(string ip, int port)
        {
            SetIpAndPort(ip, port);
        }

        public void SetIpAndPort(string ipStr, int port)
        {
            Ip = ipStr;
            Port = port;
        }

        public void Connect()
        {
            try
            {
                var asyncReceive = new MessageAsyncReceive(msg);
                asyncReceive.BeginReceive(ReceiveCallback);

                //clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                //clientSocket.Connect(Ip, Port);
                //session = new TcpSession(clientSocket, asyncReceive);

                udpAsyncReceive = new AsyncReceive();
                clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                clientSocket.Connect(Ip, Port);
                clientSocket.BeginReceiveFrom(udpAsyncReceive.Buffer, udpAsyncReceive.Offset, udpAsyncReceive.Size, SocketFlags.None, ref remoteEP, ReceiveFromCallback, null);
                session = new KcpSession(clientSocket, asyncReceive, clientSocket.RemoteEndPoint);
                OnAsyncReceive += session.Receive;
            }
            catch (Exception e)
            {
                Debug.LogWarning("Unable to connect to server, please check your network: " + e);
            }
        }

        private void ReceiveFromCallback(IAsyncResult ar)
        {
            try
            {
                int count = clientSocket.EndReceiveFrom(ar, ref remoteEP);
                OnAsyncReceive?.Invoke(udpAsyncReceive, count);
                clientSocket.BeginReceiveFrom(udpAsyncReceive.Buffer, udpAsyncReceive.Offset, udpAsyncReceive.Size, SocketFlags.None, ref remoteEP, ReceiveFromCallback, null);
            }
            catch
            {
                Disconnect();
            }
        }

        public void Update()
        {
            while (runOnMainThread.Count > 0)
            {
                runOnMainThread[0]?.Invoke();
                runOnMainThread.RemoveAt(0);
            }
        }

        private void ReceiveCallback(int count)
        {
            if (count > 0)
            {
                msg.Process(count, Response);
            }
        }

        public void Disconnect()
        {
            if (session != null)
            {
                OnAsyncReceive -= session.Receive;
                session.Close();
                session = null;
                clientSocket.Close();
                clientSocket = null;
            }
        }

        public void Publish<T>(RequestCode requestCode, T data)
        {
            if (session != null && session.IsConnected)
            {
                byte[] bytes = null;
                if (typeof(T) == typeof(byte[]))
                {
                    bytes = Message.Pack(requestCode, data as byte[]);
                }
                else
                {
                    bytes = Message.Pack(requestCode, data.ToString());
                }
                session.Send(bytes);
            }
        }

        public IActionSubject<T> Receive<T>(RequestCode requestCode)
        {
            object notifier;
            lock (notifiers)
            {
                if (!notifiers.ContainsKey(requestCode))
                {
                    IActionSubject<T> n = new ActionSubject<T>();
                    notifier = n;
                    notifiers.Add(requestCode, notifier);
                }
            }
            return notifiers[requestCode] as IActionSubject<T>;
        }

        public void Response(RequestCode requestCode, byte[] dataBytes)
        {
            lock (notifiers)
            {
                if (notifiers.ContainsKey(requestCode))
                {
                    Type[] types = notifiers[requestCode].GetType().GetGenericArguments();
                    if (types[0] == typeof(string))
                    {
                        runOnMainThread.Add(() => (notifiers[requestCode] as IActionSubject<string>).OnNext(Encoding.UTF8.GetString(dataBytes)));
                    }
                    else if (types[0] == typeof(byte[]))
                    {
                        runOnMainThread.Add(() => (notifiers[requestCode] as IActionSubject<byte[]>).OnNext(dataBytes));
                    }
                }
                else
                {
                    Debug.Log("The notifier corresponding to " + requestCode + " could not be found.");
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
