namespace UniEasy.DI
{
    public abstract class Installer : InstallerBase
    {
    }

    public abstract class Installer<TDerived> : InstallerBase
        where TDerived : Installer<TDerived>
    {
        public static void Install(DiContainer container)
        {
            container.Instantiate<TDerived>().InstallBindings();
        }
    }
}
