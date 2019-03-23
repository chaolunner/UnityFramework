using UnityEngine;
using System;

namespace UniEasy.DI
{
    [System.Diagnostics.DebuggerStepThrough]
    public class MonoInstallerBase : MonoBehaviour, IInstaller
    {
        [Inject]
        DiContainer container = null;

        protected DiContainer Container
        {
            get
            {
                return container;
            }
        }

        public virtual bool IsEnabled
        {
            get
            {
                return this.enabled;
            }
        }

        public virtual void Start()
        {
            // Define this method so we expose the enabled check box
        }

        public virtual void InstallBindings()
        {
            throw new NotImplementedException();
        }
    }
}
