using UnityEditor;

namespace UniEasy.Editor
{
    public class SystemBehaviourInstaller : ScriptAssetInstaller
    {
        [MenuItem("Assets/Create/Custom Script/SystemBehaviour Installer", false, 22)]
        static public void CreateSystemBehaviour()
        {
            new SystemBehaviourInstaller().Create();
        }

        public override string GetName()
        {
            return "NewSystemBehaviour.cs";
        }

        public override string GetContents()
        {
            return "using UnityEngine;" +
            "\nusing UniEasy.ECS;" +
            "\nusing UniEasy;" +
            "\nusing System;" +
            "\nusing UniRx;" +
            "\n" +
            "\npublic class NewSystemBehaviour : SystemBehaviour" +
            "\n{" +
            "\n\tpublic override void Initialize (IEventSystem eventSystem, IPoolManager poolManager, GroupFactory groupFactory, PrefabFactory prefabFactory)" +
            "\n\t{" +
            "\n\t\tbase.Initialize (eventSystem, poolManager, groupFactory, prefabFactory);" +
            "\n\t}" +
            "\n" +
            "\n\tpublic override void OnEnable ()" +
            "\n\t{" +
            "\n\t\tbase.OnEnable ();" +
            "\n\t}" +
            "\n}";
        }
    }
}
