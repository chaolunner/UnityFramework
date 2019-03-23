using UnityEditor;

namespace UniEasy.Editor
{
    public class ScriptableObjectInstaller : ScriptAssetInstaller
    {
        [MenuItem("Assets/Create/Custom Script/ScriptableObject Installer", false, 43)]
        static public void CreateScriptableObjectInstaller()
        {
            new ScriptableObjectInstaller().Create();
        }

        public override string GetName()
        {
            return "NewScriptableObjectInstaller.cs";
        }

        public override string GetContents()
        {
            return "using UnityEngine;"
            + "\nusing UniEasy.DI;"
            + "\n"
            + "\npublic class NewScriptableObjectInstaller : ScriptableObjectInstaller"
            + "\n{"
            + "\n    public override void InstallBindings()"
            + "\n    {"
            + "\n    }"
            + "\n}";
        }
    }
}
