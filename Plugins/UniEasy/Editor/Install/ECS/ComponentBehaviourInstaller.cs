using UnityEditor;

namespace UniEasy.Editor
{
    public class ComponentBehaviourInstaller : ScriptAssetInstaller
    {
        [MenuItem("Assets/Create/Custom Script/ComponentBehaviour Installer", false, 21)]
        static public void CreateComponentBehaviour()
        {
            new ComponentBehaviourInstaller().Create();
        }

        public override string GetName()
        {
            return "NewComponentBehaviour.cs";
        }

        public override string GetContents()
        {
            return "using UnityEngine;" +
            "\nusing UniEasy.ECS;" +
            "\n" +
            "\npublic class NewComponentBehaviour : ComponentBehaviour" +
            "\n{" +
            "\n}";
        }
    }
}
