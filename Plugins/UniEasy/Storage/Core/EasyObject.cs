using UnityEngine;
using System;

namespace UniEasy
{
    [Serializable]
    public class EasyObject
    {
        [SerializeField]
        private TypeCode typeCode;
        [SerializeField]
        private string target;

        public object GetObject()
        {
            if (!string.IsNullOrEmpty(target))
            {
                return Convert.ChangeType(target, typeCode);
            }
            return default(object);
        }

        public EasyObject()
        {
        }

        public EasyObject(object value)
        {
            var type = value.GetType();
            if (type.IsSerializable)
            {
                target = Convert.ChangeType(value, typeof(string)).ToString();
            }
            typeCode = Type.GetTypeCode(type);
        }
    }
}
