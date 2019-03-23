using System;
using UniRx;

namespace UniEasy.ECS
{
    public class ScriptableComponent : UnityEngine.ScriptableObject, IComponent, IDisposable
    {
        private CompositeDisposable disposer = new CompositeDisposable();

        public CompositeDisposable Disposer
        {
            get { return disposer; }
            set { disposer = value; }
        }

        public virtual void OnDestroy()
        {
            Dispose();
        }

        public void Dispose()
        {
            Disposer.Dispose();
        }
    }
}
