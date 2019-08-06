using UnityEditor;
using UnityEngine;
using System.IO;

namespace UniEasy.Editor
{
    public class ABCacheUtility
    {
        [MenuItem("Tools/AssetBundles/Clear Cache")]
        public static void ClearCache()
        {
            if (Caching.ClearCache())
            {
                Debug.Log("Successfully cleaned the cache.");
            }
            else
            {
                Debug.Log("Cache is being used.");
            }
            var outPath = UniEasy.PathsUtility.GetABOutPath() + "Cache";
            if (!string.IsNullOrEmpty(outPath) && Directory.Exists(outPath))
            {
                Directory.Delete(outPath, true);
                File.Delete(outPath + ".meta");
                AssetDatabase.Refresh();
            }
        }
    }
}
