using UnityEditor;
using UnityEngine;
using System.IO;

namespace UniEasy.Editor
{
    public class ABDeleteUtility
    {
        [MenuItem("Tools/AssetBundles/Delete All")]
        public static void DeleteAllAB()
        {
            var outPath = UniEasy.PathsUtility.GetABOutPath();
            if (!string.IsNullOrEmpty(outPath) && Directory.Exists(outPath))
            {
                Directory.Delete(outPath, true);
                File.Delete(outPath + ".meta");
                AssetDatabase.Refresh();
            }
            Debug.Log("Delete all AssetBundles has Completed!");
        }
    }
}
