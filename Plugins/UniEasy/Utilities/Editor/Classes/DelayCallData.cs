namespace UniEasy.Editor
{
    public class DelayCallData
    {
        public delegate void CallbackFunction();

        public CallbackFunction Callback;

        public float StartTime;

        public float Delay;

        public DelayCallData(CallbackFunction callback, float startTime, float delay)
        {
            Callback = callback;
            StartTime = startTime;
            Delay = delay;
        }
    }
}
