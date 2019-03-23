using UnityEditor;

namespace UniEasy.Editor
{
    public class MonoInstaller : ScriptAssetInstaller
    {
        [MenuItem("Assets/Create/Custom Script/Mono Installer", false, 42)]
        static public void CreateMonoInstaller()
        {
            new MonoInstaller().Create();
        }

        public override string GetName()
        {
            return "NewMonoInstaller.cs";
        }

        public override string GetContents()
        {
            return "using UnityEngine;"
            + "\nusing UniEasy.DI;"
            + "\n"
            + "\npublic class NewMonoInstaller : MonoInstaller"
            + "\n{"
            + "\n    public override void InstallBindings()"
            + "\n    {"
            + "\n    }"
            + "\n}";
        }
    }
}
