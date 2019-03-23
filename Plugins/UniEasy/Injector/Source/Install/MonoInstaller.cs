using UnityEngine;

namespace UniEasy.DI
{
    public class MonoInstaller : MonoInstallerBase
    {

    }

    public class MonoInstaller<TDerived> : MonoInstaller
        where TDerived : MonoInstaller<TDerived>
    {
        public static void InstallFromResource(string resourcePath, DiContainer container)
        {
            var installer = Resources.Load<TDerived>(resourcePath);
            container.Inject(installer);
            installer.InstallBindings();
        }
    }
}
