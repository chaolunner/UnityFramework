namespace UniEasy.DI
{
    public abstract class InstallerBase : IInstaller
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
                return true;
            }
        }

        public abstract void InstallBindings();
    }
}
