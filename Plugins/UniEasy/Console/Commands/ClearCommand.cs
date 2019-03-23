#if (NETFX_CORE || NET_4_6 || NET_STANDARD_2_0 || UNITY_WSA_10_0)
using System;
#endif
using UniRx;

namespace UniEasy.Console
{
    public class ClearCommand
    {
        public static readonly string Name = "Clear";
        public static readonly string Description = "Clears console output.";
        public static readonly string Usage = "Clear";

        private static Subject<bool> onClear;

        [Command("Clear", "Clears console output.", "Clear")]
        public static string Execute(params string[] args)
        {
#if UNITY_EDITOR
            var clearMethod = TypeHelper.LogEntriesType.GetMethod("Clear", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);
            clearMethod.Invoke(null, null);
#endif
            if (onClear != null)
            {
                onClear.OnNext(true);
            }
            return null;
        }

        public static IObservable<bool> OnClearAsObservable()
        {
            return onClear ?? (onClear = new Subject<bool>());
        }
    }
}
