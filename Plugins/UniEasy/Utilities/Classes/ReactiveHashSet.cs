using System.Runtime.Serialization;
using System.Collections.Generic;
using System.Collections;
using System;
using UniRx;

namespace UniEasy
{
    public struct HashSetAddEvent<T> : IEquatable<HashSetAddEvent<T>>
    {
        public T Value { get; private set; }

        public HashSetAddEvent(T value) : this()
        {
            Value = value;
        }

        public override string ToString()
        {
            return string.Format("Value:{0}", Value);
        }

        public override int GetHashCode()
        {
            return EqualityComparer<T>.Default.GetHashCode(Value) << 2;
        }

        public bool Equals(HashSetAddEvent<T> other)
        {
            return EqualityComparer<T>.Default.Equals(Value, other.Value);
        }
    }

    public struct HashSetRemoveEvent<T> : IEquatable<HashSetRemoveEvent<T>>
    {
        public T Value { get; private set; }

        public HashSetRemoveEvent(T value) : this()
        {
            Value = value;
        }

        public override string ToString()
        {
            return string.Format("Value:{0}", Value);
        }

        public override int GetHashCode()
        {
            return EqualityComparer<T>.Default.GetHashCode(Value) << 2;
        }

        public bool Equals(HashSetRemoveEvent<T> other)
        {
            return EqualityComparer<T>.Default.Equals(Value, other.Value);
        }
    }

    public struct HashSetReplaceEvent<T> : IEquatable<HashSetReplaceEvent<T>>
    {
        public T OldValue { get; private set; }

        public T NewValue { get; private set; }

        public HashSetReplaceEvent(T oldValue, T newValue) : this()
        {
            OldValue = oldValue;
            NewValue = newValue;
        }

        public override string ToString()
        {
            return string.Format("OldValue:{0} NewValue:{1}", OldValue, NewValue);
        }

        public override int GetHashCode()
        {
            return EqualityComparer<T>.Default.GetHashCode(OldValue) << 2 ^ EqualityComparer<T>.Default.GetHashCode(NewValue) >> 2;
        }

        public bool Equals(HashSetReplaceEvent<T> other)
        {
            return EqualityComparer<T>.Default.Equals(OldValue, other.OldValue) && EqualityComparer<T>.Default.Equals(NewValue, other.NewValue);
        }
    }

    public interface IReadOnlyReactiveHashSet<T> : IEnumerable<T>
    {
        int Count { get; }

        bool Contains(T value);

        IObservable<HashSetAddEvent<T>> ObserveAdd();

        IObservable<int> ObserveCountChanged();

        IObservable<HashSetRemoveEvent<T>> ObserveRemove();

        IObservable<HashSetReplaceEvent<T>> ObserveReplace();

        IObservable<Unit> ObserveReset();
    }

    public interface IReactiveHashSet<T> : IReadOnlyReactiveHashSet<T>
    {
    }

    public class ReactiveHashSet<T> : IReactiveHashSet<T>, IEnumerable, ICollection<T>, IEnumerable<T>, IDisposable
#if !UNITY_METRO
    , ISerializable, IDeserializationCallback
#endif
    {
        [NonSerialized]
        bool isDisposed = false;

#if !UniRxLibrary
        [UnityEngine.SerializeField]
#endif
        readonly HashSet<T> inner;

        public ReactiveHashSet()
        {
            inner = new HashSet<T>();
        }

        public ReactiveHashSet(IEqualityComparer<T> comparer)
        {
            inner = new HashSet<T>(comparer);
        }

        public ReactiveHashSet(HashSet<T> innerHashSet)
        {
            inner = innerHashSet;
        }

        public int Count
        {
            get
            {
                return inner.Count;
            }
        }

        public void Add(T value)
        {
            inner.Add(value);

            if (hashSetAdd != null)
            {
                hashSetAdd.OnNext(new HashSetAddEvent<T>(value));
            }
            if (countChanged != null)
            {
                countChanged.OnNext(Count);
            }
        }

        public void Clear()
        {
            var beforeCount = Count;
            inner.Clear();

            if (collectionReset != null)
            {
                collectionReset.OnNext(Unit.Default);
            }
            if (beforeCount > 0)
            {
                if (countChanged != null)
                {
                    countChanged.OnNext(Count);
                }
            }
        }

        public bool Remove(T value)
        {
            if (inner.Contains(value))
            {
                var isSuccessRemove = inner.Remove(value);
                if (isSuccessRemove)
                {
                    if (hashSetRemove != null)
                    {
                        hashSetRemove.OnNext(new HashSetRemoveEvent<T>(value));
                    }
                    if (countChanged != null)
                    {
                        countChanged.OnNext(Count);
                    }
                }
                return isSuccessRemove;
            }
            else
            {
                return false;
            }
        }

        public bool Contains(T value)
        {
            return inner.Contains(value);
        }

        public HashSet<T>.Enumerator GetEnumerator()
        {
            return inner.GetEnumerator();
        }

        void DisposeSubject<TSubject>(ref Subject<TSubject> subject)
        {
            if (subject != null)
            {
                try
                {
                    subject.OnCompleted();
                }
                finally
                {
                    subject.Dispose();
                    subject = null;
                }
            }
        }

        #region IDisposable Support

        private bool disposedValue = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    DisposeSubject(ref countChanged);
                    DisposeSubject(ref collectionReset);
                    DisposeSubject(ref hashSetAdd);
                    DisposeSubject(ref hashSetRemove);
                    DisposeSubject(ref hashSetReplace);
                }

                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }

        #endregion

        #region Observe

        [NonSerialized]
        Subject<int> countChanged = null;

        public IObservable<int> ObserveCountChanged()
        {
            if (isDisposed)
            {
                return Observable.Empty<int>();
            }
            return countChanged ?? (countChanged = new Subject<int>());
        }

        [NonSerialized]
        Subject<Unit> collectionReset = null;

        public IObservable<Unit> ObserveReset()
        {
            if (isDisposed)
            {
                return Observable.Empty<Unit>();
            }
            return collectionReset ?? (collectionReset = new Subject<Unit>());
        }

        [NonSerialized]
        Subject<HashSetAddEvent<T>> hashSetAdd = null;

        public IObservable<HashSetAddEvent<T>> ObserveAdd()
        {
            if (isDisposed)
            {
                return Observable.Empty<HashSetAddEvent<T>>();
            }
            return hashSetAdd ?? (hashSetAdd = new Subject<HashSetAddEvent<T>>());
        }

        [NonSerialized]
        Subject<HashSetRemoveEvent<T>> hashSetRemove = null;

        public IObservable<HashSetRemoveEvent<T>> ObserveRemove()
        {
            if (isDisposed)
            {
                return Observable.Empty<HashSetRemoveEvent<T>>();
            }
            return hashSetRemove ?? (hashSetRemove = new Subject<HashSetRemoveEvent<T>>());
        }

        [NonSerialized]
        Subject<HashSetReplaceEvent<T>> hashSetReplace = null;

        public IObservable<HashSetReplaceEvent<T>> ObserveReplace()
        {
            if (isDisposed)
            {
                return Observable.Empty<HashSetReplaceEvent<T>>();
            }
            return hashSetReplace ?? (hashSetReplace = new Subject<HashSetReplaceEvent<T>>());
        }

        #endregion

        #region implement explicit

        bool ICollection<T>.IsReadOnly
        {
            get
            {
                return ((ICollection<T>)inner).IsReadOnly;
            }
        }

#if !UNITY_METRO

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            ((ISerializable)inner).GetObjectData(info, context);
        }

        public void OnDeserialization(object sender)
        {
            ((IDeserializationCallback)inner).OnDeserialization(sender);
        }

#endif

        void ICollection<T>.CopyTo(T[] array, int arrayIndex)
        {
            ((ICollection<T>)inner).CopyTo(array, arrayIndex);
        }

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            return ((ICollection<T>)inner).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return inner.GetEnumerator();
        }

        #endregion
    }
}
