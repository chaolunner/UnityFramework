using UnityEditor;

namespace UniEasy.Editor
{
    public class RuntimeComponentInstaller : ScriptAssetInstaller
    {
        [MenuItem("Assets/Create/Custom Script/RuntimeComponent Installer", false, 23)]
        static public void CreateComponentBehaviour()
        {
            new RuntimeComponentInstaller().Create();
        }

        public override string GetName()
        {
            return "NewRuntimeComponent.cs";
        }

        public override string GetContents()
        {
            return "using UnityEngine;" +
            "\nusing UniEasy.ECS;" +
            "\n" +
            "\npublic class NewRuntimeComponent : RuntimeComponent" +
            "\n{" +
            "\n}";
        }
    }
}
