using Common;
using UnityEngine;

namespace UniEasy.Net
{
    public class NetworkSystem : MonoBehaviour
    {
        private static NetworkSystem instance;
        private NetworkManager netwrokManager;
        private RequestManager requestManager;

        public static NetworkSystem GetInstance()
        {
            if (instance == null)
            {
                instance = new GameObject("NetworkSystem").AddComponent<NetworkSystem>();
                DontDestroyOnLoad(instance);
            }
            return instance;
        }

        private void Start()
        {
            netwrokManager = new NetworkManager();
            requestManager = new RequestManager();

            netwrokManager.Initialize();
            requestManager.Initialize();
        }

        private void OnDestroy()
        {
            netwrokManager.OnDestroy();
            requestManager.OnDestroy();
        }

        public void HandleResponse(RequestCode requestCode, string data)
        {
            requestManager.HandleResponse(requestCode, data);
        }

        public void AddRequest(RequestCode requestCode, RequestBase request)
        {
            requestManager.Add(requestCode, request);
        }

        public void RemoveRequest(RequestCode requestCode)
        {
            requestManager.Remove(requestCode);
        }
    }
}
