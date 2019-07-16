using UnityEditor;
using System.IO;

namespace UniEasy.Editor
{
    public class ABBuildUtility
    {
        [MenuItem("Tools/AssetBundles/Build StandaloneWindows64")]
        public static void BuildAllABForWindows64()
        {
            BuildAllAB(BuildAssetBundleOptions.None, BuildTarget.StandaloneWindows64);
        }

        [MenuItem("Tools/AssetBundles/Build iOS")]
        public static void BuildAllABForIOS()
        {
            BuildAllAB(BuildAssetBundleOptions.None, BuildTarget.iOS);
        }

        [MenuItem("Tools/AssetBundles/Build Android")]
        public static void BuildAllABForAndroid()
        {
            BuildAllAB(BuildAssetBundleOptions.None, BuildTarget.Android);
        }

        private static void BuildAllAB(BuildAssetBundleOptions options, BuildTarget target)
        {
            var outPath = UniEasy.PathsUtility.GetABOutPath();
            if (!Directory.Exists(outPath))
            {
                Directory.CreateDirectory(outPath);
            }
            BuildPipeline.BuildAssetBundles(outPath, options, target);
            AssetDatabase.Refresh();
        }
    }
}