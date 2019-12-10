using System;

namespace UniEasy
{
    public class EmptyDisposable : IDisposable
    {
        public static EmptyDisposable Singleton = new EmptyDisposable();
        private EmptyDisposable() { }
        public void Dispose() { }
    }
}
