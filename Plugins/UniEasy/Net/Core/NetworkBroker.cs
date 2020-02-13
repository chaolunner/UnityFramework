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

    public struct ReceiveData
    {
        public byte[] Value;
        public SessionMode Mode;

        public string StringValue
        {
            get
            {
                return Encoding.UTF8.GetString(Value);
            }
        }

        public ReceiveData(byte[] value, SessionMode mode)
        {
            Value = value;
            Mode = mode;
        }
    }

    public interface IRequestReceiver
    {
        IActionSubject<ReceiveData> Receive(RequestCode requestCode);
    }

    public interface IRequestResponser
    {
        void Response(RequestCode requestCode, byte[] dataBytes);
    }

    public interface INetworkBroker : IRequestResponser, IRequestReceiver, IRequestPublisher
    {
        SessionMode Mode { get; set; }
        void Connect();
        void Disconnect();
        void Update();
    }

    public class NetworkBroker : INetworkBroker, IDisposable
    {
        public static readonly INetworkBroker Default = new NetworkBroker();

        private NetworkMode mode = NetworkSettings.Mode;
        private string ip = NetworkSettings.Ip;
        private int port = NetworkSettings.Port;
        private bool isDisposed = false;
        private Socket clientSocket;
        private EndPoint remoteEP = new IPEndPoint(IPAddress.Any, 0);
        private Message msg = new Message();
        private ISession session;
        private IAsyncReceive udpAsyncReceive;
        private event Action<IAsyncReceive, int> OnAsyncReceive;
        private readonly List<Action> runOnMainThread = new List<Action>();
        private readonly Dictionary<RequestCode, IActionSubject<ReceiveData>> notifiers = new Dictionary<RequestCode, IActionSubject<ReceiveData>>();

        [DI.Inject]
        public NetworkBroker() { }

        public SessionMode Mode
        {
            get
            {
                if (session != null)
                {
                    return session.Mode;
                }
                return SessionMode.None;
            }
            set
            {
                if (session != null)
                {
                    session.Mode = value;
                }
            }
        }

        public void Connect()
        {
            try
            {
                var asyncReceive = new MessageAsyncReceive(msg);
                asyncReceive.BeginReceive(ReceiveCallback);

                if (mode == NetworkMode.Tcp)
                {
                    clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    session = new TcpSession(clientSocket, asyncReceive);
                    clientSocket.Connect(ip, port);
                }
                else if (mode == NetworkMode.Udp)
                {
                    clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                    session = new KcpSession(clientSocket, asyncReceive, clientSocket.RemoteEndPoint);
                    clientSocket.Connect(ip, port);
                    udpAsyncReceive = new AsyncReceive();
                    clientSocket.BeginReceiveFrom(udpAsyncReceive.Buffer, udpAsyncReceive.Offset, udpAsyncReceive.Size, SocketFlags.None, ref remoteEP, ReceiveFromCallback, null);
                    OnAsyncReceive += session.Receive;
                }
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
            // You can enable offline mode here or try to connect again.
            if (session != null && !session.IsConnected && session.Mode == SessionMode.Online)
            {
                Mode = SessionMode.Offline;
                return;
            }

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
            if (session != null && (session.IsConnected || session.Mode == SessionMode.Offline))
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

        public IActionSubject<ReceiveData> Receive(RequestCode requestCode)
        {
            lock (notifiers)
            {
                if (!notifiers.ContainsKey(requestCode))
                {
                    IActionSubject<ReceiveData> notifier = new ActionSubject<ReceiveData>();
                    notifiers.Add(requestCode, notifier);
                }
            }
            return notifiers[requestCode];
        }

        public void Response(RequestCode requestCode, byte[] dataBytes)
        {
            lock (notifiers)
            {
                if (notifiers.ContainsKey(requestCode))
                {
                    var data = new ReceiveData(dataBytes, Mode);
                    if (Mode == SessionMode.Offline)
                    {
                        notifiers[requestCode].OnNext(data);
                    }
                    else
                    {
                        runOnMainThread.Add(() => notifiers[requestCode].OnNext(data));
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
