using UnityEngine;
using System;

namespace UniEasy.DI
{
    [Serializable]
    public class ScriptableObjectInstaller : ScriptableObjectInstallerBase
    {
    }

    [Serializable]
    public class ScriptableObjectInstaller<TDerived> : ScriptableObjectInstaller
        where TDerived : ScriptableObjectInstaller<TDerived>
    {
        public static void InstallFromResource(string resourcePath, DiContainer container)
        {
            var installer = Resources.Load<TDerived>(resourcePath);
            container.Inject(installer);
            installer.InstallBindings();
        }
    }
}
