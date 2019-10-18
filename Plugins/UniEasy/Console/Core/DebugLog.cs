using UnityEngine.UI;
using UniEasy.ECS;
using UnityEngine;
using UniRx;

namespace UniEasy.Console
{
    public class DebugLog : ComponentBehaviour
    {
        public LogType LogType;
        public StringReactiveProperty Message = new StringReactiveProperty();
        public Image MessageBackground;
        public Text MessageText;
        public IntReactiveProperty Count = new IntReactiveProperty();
        public Image CountBackground;
        public Text CountText;
    }
}
