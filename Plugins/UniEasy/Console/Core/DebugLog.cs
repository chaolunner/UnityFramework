using UnityEngine.UI;
using UniEasy.ECS;
using UniRx;

namespace UniEasy.Console
{
    public class DebugLog : ComponentBehaviour
    {
        public int Index = 0;
        public ReactiveProperty<LogData> LogData = new ReactiveProperty<LogData>();
        public Image MessageBackground;
        public Text MessageText;
        public IntReactiveProperty Count = new IntReactiveProperty();
        public Image CountBackground;
        public Text CountText;
    }
}
