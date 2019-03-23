using System.Collections.Generic;
using UnityEngine;

namespace UniEasy
{
    [System.Serializable]
    public class InspectableObjectData : EasyDictionary<int, Object>
    {
        public string Type;
        public string Data;

        public InspectableObjectData()
        {
            Type = string.Empty;
            Data = string.Empty;
            base.target = new Dictionary<int, Object>();
        }

        public InspectableObjectData(string type, string data)
        {
            Type = type;
            Data = data;
            base.target = new Dictionary<int, Object>();
        }
    }
}
