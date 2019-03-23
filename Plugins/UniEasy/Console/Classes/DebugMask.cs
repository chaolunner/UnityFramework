using System.Collections.Generic;
using System;

namespace UniEasy.Console
{
    [Serializable]
    public class DebugMask : EasyHashSet<DebugLayer>
    {
        public DebugMask()
        {
            this.target = new HashSet<DebugLayer>();
        }

        public DebugMask(params string[] layerNames)
        {
            this.target = new HashSet<DebugLayer>();
            for (int i = 0; i < layerNames.Length; i++)
            {
                this.target.Add(new DebugLayer(layerNames[i], true));
            }
        }

        public DebugMask(HashSet<DebugLayer> hashSet)
        {
            this.target = hashSet;
        }

        public DebugMask AddMask(bool isEnable, string layerName)
        {
            this.target.Add(new DebugLayer(layerName, isEnable));
            return this;
        }
    }
}
