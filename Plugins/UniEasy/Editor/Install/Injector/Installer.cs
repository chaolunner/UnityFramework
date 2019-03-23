using UnityEditor;

namespace UniEasy.Editor
{
    public class Installer : ScriptAssetInstaller
    {
        [MenuItem("Assets/Create/Custom Script/Installer", false, 41)]
        static public void CreateInstaller()
        {
            new Installer().Create();
        }

        public override string GetName()
        {
            return "NewInstaller.cs";
        }

        public override string GetContents()
        {
            return "using UnityEngine;"
            + "\nusing UniEasy.DI;"
            + "\n"
            + "\npublic class NewInstaller : Installer"
            + "\n{"
            + "\n    public override void InstallBindings()"
            + "\n    {"
            + "\n    }"
            + "\n}";
        }
    }
}
