using UnityEngine;
using System;
using UniRx;

namespace UniEasy.ECS
{
    public class ComponentBehaviour : MonoBehaviour, IComponent, IDisposable
    {
        public CompositeDisposable Disposer { get; set; } = new CompositeDisposable();

        /// <summary>
        /// *Don't code any logic here!
        /// </summary>
        public virtual void OnEnable()
        {
        }

        public virtual void OnDestroy()
        {
            Dispose();
        }

        public virtual void Dispose()
        {
            Disposer.Dispose();
        }
    }
}
