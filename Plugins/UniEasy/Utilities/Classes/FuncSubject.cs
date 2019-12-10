using System.Collections.Generic;
using System;

namespace UniEasy
{
    public interface IFuncObservable<T, R>
    {
        IDisposable Subscribe(IFuncObserver<T, R> observer);
    }

    public interface IFuncObserver<T, R>
    {
        void OnCompleted();
        void OnError(Exception error);
        List<R> OnNext(T value);
    }

    public interface IFuncSubject<T, R> : IFuncObservable<T, R>, IFuncObserver<T, R>, IDisposable { }

    public class FuncSubject<T, R> : IFuncSubject<T, R>
    {
        class Subscription : IDisposable
        {
            FuncSubject<T, R> parent;
            IFuncObserver<T, R> unsubscribeTarget;

            public Subscription(FuncSubject<T, R> parent, IFuncObserver<T, R> unsubscribeTarget)
            {
                this.parent = parent;
                this.unsubscribeTarget = unsubscribeTarget;
            }

            public void Dispose()
            {
                if (parent != null)
                {
                    parent.observerList.Remove(unsubscribeTarget);
                    unsubscribeTarget = null;
                    parent = null;
                }
            }
        }

        private bool isDisposed;
        private bool isStopped;
        private List<IFuncObserver<T, R>> observerList = new List<IFuncObserver<T, R>>();
        private Action onCompleted = null;
        private Action<Exception> onError = null;
        private Func<T, R> onNext = null;

        public FuncSubject(Func<T, R> onNext = null, Action onCompleted = null, Action<Exception> onError = null)
        {
            this.onCompleted = onCompleted;
            this.onError = onError;
            this.onNext = onNext;
        }

        public void OnCompleted()
        {
            foreach (var observer in observerList)
            {
                observer.OnCompleted();
            }
            if (!isStopped && !isDisposed) { onCompleted?.Invoke(); }
            isStopped = true;
        }

        public void OnError(Exception error)
        {
            foreach (var observer in observerList)
            {
                observer.OnError(error);
            }
            if (!isStopped && !isDisposed) { onError?.Invoke(error); }
            isStopped = true;
        }

        public List<R> OnNext(T value)
        {
            var list = new List<R>();
            foreach (var observer in observerList)
            {
                list.AddRange(observer.OnNext(value));
            }
            if (!isStopped && !isDisposed && onNext != null)
            {
                list.Add(onNext.Invoke(value));
            }
            return list;
        }

        public IDisposable Subscribe(IFuncObserver<T, R> observer)
        {
            try
            {
                if (!isStopped)
                {
                    observerList.Add(observer);
                    return new Subscription(this, observer);
                }
                else
                {
                    observer.OnCompleted();
                }
            }
            catch (Exception e)
            {
                observer.OnError(e);
            }
            return EmptyDisposable.Singleton;
        }

        public void Dispose()
        {
            isDisposed = true;
            observerList.Clear();
            onCompleted = null;
            onError = null;
            onNext = null;
        }
    }
}
