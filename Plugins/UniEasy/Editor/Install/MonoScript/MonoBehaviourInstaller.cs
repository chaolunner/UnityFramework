using UnityEditor;

namespace UniEasy.Editor
{
    public class MonoBehaviourInstaller : ScriptAssetInstaller
    {
        [MenuItem("Assets/Create/Custom Script/MonoBehaviour Installer", false, 9)]
        public static void CreateMonoBehaviour()
        {
            new MonoBehaviourInstaller().Create();
        }

        public override string GetName()
        {
            return "NewBehaviourScript.cs";
        }

        public override string GetContents()
        {
            return "using System.Collections.Generic;" +
            "\nusing System.Collections;" +
            "\nusing UnityEngine;" +
            "\n" +
            "\npublic class NewBehaviourScript : MonoBehaviour {" +
            "\n" +
            "\n\tvoid Start () {" +
            "\n\t" +
            "\n\t}" +
            "\n\t" +
            "\n\tvoid Update () {" +
            "\n\t" +
            "\n\t}" +
            "\n}";
        }
    }
}
