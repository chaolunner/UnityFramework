using UnityEngine;
using Common;

namespace UniEasy.Net
{
    public class RequestBase : MonoBehaviour
    {
        private RequestCode requestCode = RequestCode.None;

        public virtual void Awake()
        {
            NetworkSystem.GetInstance().AddRequest(requestCode, this);
        }

        public virtual void SendRequest()
        {

        }

        public virtual void OnResponse(string data)
        {

        }

        public virtual void OnDestroy()
        {
            NetworkSystem.GetInstance().RemoveRequest(requestCode);
        }
    }
}
