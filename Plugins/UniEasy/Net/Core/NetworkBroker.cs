using System.Collections.Generic;
using System.Net.Sockets;
using UnityEngine;
using System;
using Common;

namespace UniEasy.Net
{
    public interface ISubject<T> : IObservable<T>, IObserver<T>, IDisposable { }

    public class EmptyDisposable : IDisposable
    {
        public static EmptyDisposable Singleton = new EmptyDisposable();
        private EmptyDisposable() { }
        public void Dispose() { }
    }

    public class Subject<T> : ISubject<T>
    {
        class Subscription : IDisposable
        {
            Subject<T> parent;
            IObserver<T> unsubscribeTarget;

            public Subscription(Subject<T> parent, IObserver<T> unsubscribeTarget)
            {
                this.parent = parent;
                this.unsubscribeTarget = unsubscribeTarget;
            }

            public void Dispose()
            {
                if (parent != null)
                {
                    parent.observerList.Remove(unsubscribeTarget);
                    unsubscribeTarget = null;
                    parent = null;
                }
            }
        }

        private bool isDisposed;
        private bool isStopped;
        private List<IObserver<T>> observerList = new List<IObserver<T>>();
        private Action onCompleted = null;
        private Action<Exception> onError = null;
        private Action<T> onNext = null;

        public Subject(Action<T> onNext = null, Action onCompleted = null, Action<Exception> onError = null)
        {
            this.onCompleted = onCompleted;
            this.onError = onError;
            this.onNext = onNext;
        }

        public void OnCompleted()
        {
            foreach (var observer in observerList)
            {
                observer.OnCompleted();
            }
            if (!isStopped && !isDisposed) { onCompleted?.Invoke(); }
            isStopped = true;
        }

        public void OnError(Exception error)
        {
            foreach (var observer in observerList)
            {
                observer.OnError(error);
            }
            if (!isStopped && !isDisposed) { onError?.Invoke(error); }
            isStopped = true;
        }

        public void OnNext(T value)
        {
            foreach (var observer in observerList)
            {
                observer.OnNext(value);
            }
            if (!isStopped && !isDisposed) { onNext?.Invoke(value); }
        }

        public IDisposable Subscribe(IObserver<T> observer)
        {
            try
            {
                if (!isStopped)
                {
                    observerList.Add(observer);
                    return new Subscription(this, observer);
                }
                else
                {
                    observer.OnCompleted();
                }
            }
            catch (Exception e)
            {
                observer.OnError(e);
            }
            return EmptyDisposable.Singleton;
        }

        public void Dispose()
        {
            isDisposed = true;
            observerList.Clear();
            onCompleted = null;
            onError = null;
            onNext = null;
        }
    }

    public interface IRequestPublisher
    {
        void Publish<T>(RequestCode requestCode, T data);
    }

    public interface IRequestReceiver
    {
        ISubject<T> Receive<T>(RequestCode requestCode);
    }

    public interface IRequestResponser
    {
        void Response<T>(RequestCode requestCode, T data);
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
        private Message msg = new Message();
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

        public void Update()
        {
            while (runOnMainThread.Count > 0)
            {
                runOnMainThread[0]?.Invoke();
                runOnMainThread.RemoveAt(0);
            }
        }

        private void Start()
        {
            if (clientSocket != null && clientSocket.Connected)
            {
                clientSocket.BeginReceive(msg.Data, msg.StartIndex, msg.RemainSize, SocketFlags.None, ReceiveCallback, null);
            }
        }

        private void ReceiveCallback(IAsyncResult ar)
        {
            try
            {
                if (clientSocket == null || !clientSocket.Connected) { return; }
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

        public void Publish<T>(RequestCode requestCode, T data)
        {
            if (clientSocket == null || !clientSocket.Connected) { return; }
            byte[] bytes = Message.Pack(requestCode, data.ToString());
            clientSocket.Send(bytes);
        }

        public ISubject<T> Receive<T>(RequestCode requestCode)
        {
            object notifier;
            lock (notifiers)
            {
                if (!notifiers.ContainsKey(requestCode))
                {
                    ISubject<T> n = new Subject<T>();
                    notifier = n;
                    notifiers.Add(requestCode, notifier);
                }
            }
            return notifiers[requestCode] as ISubject<T>;
        }

        public void Response<T>(RequestCode requestCode, T data)
        {
            lock (notifiers)
            {
                if (notifiers.ContainsKey(requestCode))
                {
                    runOnMainThread.Add(() => (notifiers[requestCode] as ISubject<T>).OnNext(data));
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
