using System.Collections.Generic;

namespace UniEasy.DI
{
    public static class MiscExtensions
    {
        public static IEnumerable<T> Yield<T>(this T item)
        {
            yield return item;
        }
    }
}
