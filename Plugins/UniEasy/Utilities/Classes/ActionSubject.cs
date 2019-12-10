using System.Collections.Generic;
using System;

namespace UniEasy
{
    public interface IActionSubject<T> : IObservable<T>, IObserver<T>, IDisposable { }

    public class ActionSubject<T> : IActionSubject<T>
    {
        class Subscription : IDisposable
        {
            ActionSubject<T> parent;
            IObserver<T> unsubscribeTarget;

            public Subscription(ActionSubject<T> parent, IObserver<T> unsubscribeTarget)
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
        private List<IObserver<T>> observerList = new List<IObserver<T>>();
        private Action onCompleted = null;
        private Action<Exception> onError = null;
        private Action<T> onNext = null;

        public ActionSubject(Action<T> onNext = null, Action onCompleted = null, Action<Exception> onError = null)
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

        public void OnNext(T value)
        {
            foreach (var observer in observerList)
            {
                observer.OnNext(value);
            }
            if (!isStopped && !isDisposed) { onNext?.Invoke(value); }
        }

        public IDisposable Subscribe(IObserver<T> observer)
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
