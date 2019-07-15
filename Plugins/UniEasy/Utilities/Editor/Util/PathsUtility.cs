using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.Linq;
using System.IO;

namespace UniEasy.Editor
{
    public static class PathsUtility
    {
        public static string TryGetSelectedFilePathInProjectsTab()
        {
            var selectedPaths = GetSelectedFilePathsInProjectsTab();

            if (selectedPaths.Count == 1)
            {
                return selectedPaths[0];
            }

            return null;
        }

        private static List<string> GetSelectedFilePathsInProjectsTab()
        {
            return GetSelectedPathsInProjectsTab().Where(x => File.Exists(x)).ToList();
        }

        private static List<string> GetSelectedPathsInProjectsTab()
        {
            var paths = new List<string>();
            var selectedAssets = Selection.GetFiltered(typeof(Object), SelectionMode.Assets);

            foreach (var item in selectedAssets)
            {
                var relativePath = AssetDatabase.GetAssetPath(item);

                if (!string.IsNullOrEmpty(relativePath))
                {
                    var fullPath = Path.GetFullPath(Path.Combine(Application.dataPath, Path.Combine("..", relativePath)));

                    paths.Add(fullPath);
                }
            }

            return paths;
        }

        // Returns the best guess directory in projects pane
        // Useful when adding to Assets -> Create context menu
        // Returns null if it can't find one
        // Note that the path is relative to the Assets folder for use in AssetDatabase.GenerateUniqueAssetPath etc.
        public static string TryGetSelectedFolderPathInProjectsTab()
        {
            var selectedPaths = GetSelectedFolderPathsInProjectsTab();

            if (selectedPaths.Count == 1)
            {
                return selectedPaths[0];
            }

            return null;
        }

        // Note that the path is relative to the Assets folder
        private static List<string> GetSelectedFolderPathsInProjectsTab()
        {
            return GetSelectedPathsInProjectsTab().Where(paths => Directory.Exists(paths)).ToList();
        }

        public static string ConvertFullAbsolutePathToAssetPath(string fullPath)
        {
            if (fullPath == Application.dataPath) { return "Assets"; }
            return "Assets/" + Path.GetFullPath(fullPath)
                .Remove(0, Path.GetFullPath(Application.dataPath).Length + 1)
                .Replace("\\", "/");
        }

        public static string AB_RESOURCES = "AB_Resources";

        public static string GetABResourcesPath()
        {
            return Application.dataPath + "/" + AB_RESOURCES;
        }

        public static string GetABOutPath()
        {
            return GetPlatformPath() + "/" + GetPlatformName();
        }

        private static string GetPlatformPath()
        {
            var platformPath = string.Empty;

            switch (Application.platform)
            {
                case RuntimePlatform.WindowsPlayer:
                case RuntimePlatform.WindowsEditor:
                    platformPath = Application.streamingAssetsPath;
                    break;
                case RuntimePlatform.IPhonePlayer:
                case RuntimePlatform.Android:
                    platformPath = Application.persistentDataPath;
                    break;
                default:
                    break;
            }

            return platformPath;
        }

        public static string GetPlatformName()
        {
            var platformName = string.Empty;

            switch (Application.platform)
            {
                case RuntimePlatform.WindowsPlayer:
                case RuntimePlatform.WindowsEditor:
                    platformName = "Windows";
                    break;
                case RuntimePlatform.IPhonePlayer:
                    platformName = "Iphone";
                    break;
                case RuntimePlatform.Android:
                    platformName = "Android";
                    break;
                default:
                    break;
            }

            return platformName;
        }

        public static string GetWWWPath()
        {
            var outPath = string.Empty;

            switch (Application.platform)
            {
                case RuntimePlatform.WindowsPlayer:
                case RuntimePlatform.WindowsEditor:
                    outPath = "file://" + GetABOutPath();
                    break;
                case RuntimePlatform.Android:
                    outPath = "jar:file://" + GetABOutPath();
                    break;
                case RuntimePlatform.IPhonePlayer:
                    outPath = GetABOutPath() + "/Raw/";
                    break;
                default:
                    break;
            }

            return outPath;
        }
    }
}
