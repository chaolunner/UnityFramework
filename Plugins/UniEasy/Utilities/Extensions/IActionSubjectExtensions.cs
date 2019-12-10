using System;

namespace UniEasy
{
    public static class IActionSubjectExtensions
    {
        public static IDisposable Subscribe<T>(this IActionSubject<T> subject, Action<T> onNext)
        {
            return subject.Subscribe(new ActionSubject<T>(onNext));
        }
    }
}
