using UnityEngine;
using System;

namespace UniEasy.DI
{
    [Serializable]
    public class ScriptableObjectInstallerBase : ScriptableObject, IInstaller
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

        bool IInstaller.IsEnabled
        {
            get
            {
                return true;
            }
        }

        public virtual void InstallBindings()
        {
            throw new NotImplementedException();
        }
    }
}
