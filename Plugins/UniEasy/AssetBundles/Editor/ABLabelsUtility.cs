using UnityEditor;
using UnityEngine;
using System.IO;

namespace UniEasy.Editor
{
    public class ABLabelsUtility
    {
        [MenuItem("Tools/AssetBundles/Set Labels")]
        public static void SetABLabels()
        {
            AssetDatabase.RemoveUnusedAssetBundleNames();

            foreach (var rootPath in UniEasy.PathsUtility.GetABResourcesPaths())
            {
                var rootInfo = new DirectoryInfo(rootPath);
                var sceneInfos = rootInfo.GetDirectories();

                foreach (var sceneInfo in sceneInfos)
                {
                    SetABLabelsByRecursive(sceneInfo, sceneInfo.Name);
                }
            }

            AssetDatabase.Refresh();
            Debug.Log("Set the label of AssetBundles has Completed!");
        }

        private static void SetABLabelsByRecursive(FileSystemInfo fileSystemInfo, string sceneName)
        {
            if (!fileSystemInfo.Exists)
            {
                Debug.LogError("Directory or file: " + fileSystemInfo + " don't exists, please check it!");
                return;
            }

            var directoryInfo = fileSystemInfo as DirectoryInfo;
            var fileSystemInfos = directoryInfo.GetFileSystemInfos();
            foreach (var info in fileSystemInfos)
            {
                var fileInfo = info as FileInfo;
                if (fileInfo != null)
                {
                    SetFileABLabel(fileInfo, sceneName);
                }
                else
                {
                    SetABLabelsByRecursive(info, sceneName);
                }
            }
        }

        private static void SetFileABLabel(FileInfo fileInfo, string sceneName)
        {
            if (fileInfo.Extension == ".meta")
            {
                return;
            }

            var assetBundleName = GetABName(fileInfo, sceneName);
            var relativePathIndex = fileInfo.FullName.IndexOf("Assets");
            var relativePath = fileInfo.FullName.Substring(relativePathIndex);
            var assetImporter = AssetImporter.GetAtPath(relativePath);
            assetImporter.assetBundleName = assetBundleName;

            if (fileInfo.Extension == ".unity")
            {
                assetImporter.assetBundleVariant = "u3d";
            }
            else
            {
                assetImporter.assetBundleVariant = "ab";
            }
        }

        private static string GetABName(FileInfo fileInfo, string sceneName)
        {
            var unityPath = fileInfo.FullName.Replace("\\", "/");
            var sceneNamePostion = unityPath.LastIndexOf(sceneName + "/") + sceneName.Length;
            var fileNameArea = unityPath.Substring(sceneNamePostion + 1);
            if (fileNameArea.Contains("/"))
            {
                return sceneName + "/" + fileNameArea.Split('/')[0];
            }
            else
            {
                return sceneName + "/" + sceneName;
            }
        }
    }
}
