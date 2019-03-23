using System.Collections.Generic;
using System;

namespace UniEasy
{
    [Serializable]
    public class EasyObjectList : EasyList<EasyObject>
    {
        public EasyObjectList(List<EasyObject> list)
        {
            inner = list;
        }

        public EasyObjectList(EasyObject[] values)
        {
            inner = new List<EasyObject>();
            inner.AddRange(values);
        }
    }
}
