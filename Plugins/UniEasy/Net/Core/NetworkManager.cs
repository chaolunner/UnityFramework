using System;
using Common;
using UnityEngine;
using System.Net.Sockets;

namespace UniEasy.Net
{
    public class NetworkManager : ManagerBase
    {
        public string Ip = "127.0.0.1";
        public int Port = 8989;
        private Socket clientSocket;
        private Message msg = new Message();

        public override void Initialize()
        {
            base.Initialize();
            Connect();
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
            Disconnect();
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
                msg.Process(count, OnMessageProcessed);
                Start();
            }
            catch (Exception e)
            {
                Debug.Log(e);
            }
        }

        private void OnMessageProcessed(RequestCode requestCode, string data)
        {
            NetworkSystem.GetInstance().HandleResponse(requestCode, data);
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

        public void SendRequest(RequestCode requestCode, ActionCode actionCode, string data)
        {
            byte[] bytes = Message.Pack(requestCode, actionCode, data);
            clientSocket.Send(bytes);
        }
    }
}
