using System;
using UniRx;

namespace UniEasy.ECS
{
    [Serializable, ContextMenu("ECS/RuntimeComponent")]
    public class RuntimeComponent : IComponent, IDisposable, IDisposableContainer
    {
        public CompositeDisposable Disposer { get; set; } = new CompositeDisposable();

        public void Dispose()
        {
            Disposer.Dispose();
        }
    }
}
