using UnityEditor;
using UnityEngine;
using System.IO;

namespace UniEasy.Editor
{
    public class ABBuildUtility
    {
        [MenuItem("Tools/AssetBundles/Build AssetBundles")]
        public static void BuildAllAB()
        {
            BuildAllAB(BuildAssetBundleOptions.None, EditorUserBuildSettings.activeBuildTarget);
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
            Debug.Log("Build AssetBundles has Completed!");
        }
    }
}