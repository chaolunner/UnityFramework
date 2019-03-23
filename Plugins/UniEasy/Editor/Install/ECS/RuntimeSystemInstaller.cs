using UnityEditor;

namespace UniEasy.Editor
{
    public class RuntimeSystemInstaller : ScriptAssetInstaller
    {
        [MenuItem("Assets/Create/Custom Script/RuntimeSystem Installer", false, 25)]
        static public void CreateComponentBehaviour()
        {
            new RuntimeSystemInstaller().Create();
        }

        public override string GetName()
        {
            return "NewRuntimeSystem.cs";
        }

        public override string GetContents()
        {
            return "using UnityEngine;" +
            "\nusing UniEasy.ECS;" +
            "\nusing UniEasy;" +
            "\nusing System;" +
            "\nusing UniRx;" +
            "\n" +
            "\npublic class NewRuntimeSystem : RuntimeSystem" +
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
