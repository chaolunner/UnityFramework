using System.Linq;
using System;

namespace UniRx
{
    public static partial class Observable
    {
        public static IObservable<TSource> SafeMerge<TSource>(params IObservable<TSource>[] sources)
        {
            return Merge(sources.Where(source => source != null));
        }
    }
}
