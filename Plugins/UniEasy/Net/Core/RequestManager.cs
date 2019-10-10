using Common;
using UnityEngine;
using System.Collections.Generic;

namespace UniEasy.Net
{
    public class RequestManager : ManagerBase
    {
        private Dictionary<RequestCode, RequestBase> requestDict = new Dictionary<RequestCode, RequestBase>();

        public void Add(RequestCode requestCode, RequestBase request)
        {
            requestDict.Add(requestCode, request);
        }

        public void Remove(RequestCode requestCode)
        {
            requestDict.Remove(requestCode);
        }

        public void HandleResponse(RequestCode requestCode, string data)
        {
            RequestBase request;
            if (requestDict.TryGetValue(requestCode, out request))
            {
                request.OnResponse(data);
            }
            else
            {
                Debug.Log(requestCode + " could not be found.");
            }
        }
    }
}
