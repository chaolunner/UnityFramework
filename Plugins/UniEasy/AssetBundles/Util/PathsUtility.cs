using UnityEngine;

namespace UniEasy
{
    public partial class PathsUtility
    {
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