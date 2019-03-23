namespace UniEasy.DI
{
    public interface IInstaller
    {
        void InstallBindings();

        bool IsEnabled
        {
            get;
        }
    }
}
