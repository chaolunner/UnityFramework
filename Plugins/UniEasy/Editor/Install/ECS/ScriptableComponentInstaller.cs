using UnityEditor;

namespace UniEasy.Editor
{
    public class ScriptableComponentInstaller : ScriptAssetInstaller
    {
        [MenuItem("Assets/Create/Custom Script/ScriptableComponent Installer", false, 24)]
        static public void CreateComponentBehaviour()
        {
            new ScriptableComponentInstaller().Create();
        }

        public override string GetName()
        {
            return "NewScriptableComponent.cs";
        }

        public override string GetContents()
        {
            return "using UnityEngine;" +
            "\nusing UniEasy.ECS;" +
            "\n" +
            "\npublic class NewScriptableComponent : ScriptableComponent" +
            "\n{" +
            "\n}";
        }
    }
}
