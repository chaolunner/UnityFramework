using System.Collections.Generic;
using System;

namespace UniEasy
{
    [Serializable]
    public class EasyStringList : EasyList<string>
    {
        public EasyStringList(List<string> list)
        {
            inner = list;
        }

        public EasyStringList(params string[] values)
        {
            inner = new List<string>();
            inner.AddRange(values);
        }
    }
}
