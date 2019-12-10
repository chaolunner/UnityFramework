using System;

namespace UniEasy
{
    public static class IFuncSubjectExtensions
    {
        public static IDisposable Subscribe<T, R>(this IFuncSubject<T, R> subject, Func<T, R> onNext)
        {
            return subject.Subscribe(new FuncSubject<T, R>(onNext));
        }
    }
}
