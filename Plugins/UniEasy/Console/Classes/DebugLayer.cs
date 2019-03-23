using System;

namespace UniEasy.Console
{
    [Serializable]
    public struct DebugLayer
    {
        public string LayerName;
        public bool IsEnable;

        public DebugLayer(string layerName, bool isEnable = true)
        {
            LayerName = layerName;
            IsEnable = isEnable;
        }
    }
}
