using UnityEngine.Networking;

namespace UniEasy
{
    public delegate void ABLoadStart(string abName, UnityWebRequest uwr);
    public delegate void ABLoadUpdate(string abName, UnityWebRequest uwr);
    public delegate void ABLoadCompleted(string abName);

    public class ABDefine
    {
        public static string ASSETBUNDLE_MANIFEST = "AssetBundleManifest";
    }
}
