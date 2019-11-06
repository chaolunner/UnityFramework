using System;

namespace UniEasy.Net
{
    public static class ISubjectExtensions
    {
        public static IDisposable Subscribe<T>(this ISubject<T> subject, Action<T> onNext)
        {
            return subject.Subscribe(new Subject<T>(onNext));
        }
    }
}
