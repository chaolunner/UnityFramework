using UnityEngine;
using System.IO;

namespace UniEasy
{
    public partial class PathsUtility
    {
        public static string AB_RESOURCES = "AB_Resources";

        public static string[] GetABResourcesPaths()
        {
            return Directory.GetDirectories(Application.dataPath, AB_RESOURCES, SearchOption.AllDirectories);
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
                case RuntimePlatform.OSXPlayer:
                case RuntimePlatform.WindowsEditor:
                case RuntimePlatform.OSXEditor:
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

#if UNITY_EDITOR
            switch (UnityEditor.EditorUserBuildSettings.activeBuildTarget)
            {
                case UnityEditor.BuildTarget.StandaloneWindows:
                case UnityEditor.BuildTarget.StandaloneWindows64:
                    platformName = "Windows";
                    break;
                case UnityEditor.BuildTarget.StandaloneOSX:
                    platformName = "OSX";
                    break;
                case UnityEditor.BuildTarget.iOS:
                    platformName = "iOS";
                    break;
                case UnityEditor.BuildTarget.Android:
                    platformName = "Android";
                    break;
                default:
                    break;
            }
#else
            switch (Application.platform)
            {
                case RuntimePlatform.WindowsPlayer:
                case RuntimePlatform.WindowsEditor:
                    platformName = "Windows";
                    break;
                case RuntimePlatform.OSXPlayer:
                case RuntimePlatform.OSXEditor:
                    platformName = "OSX";
                    break;
                case RuntimePlatform.IPhonePlayer:
                    platformName = "iOS";
                    break;
                case RuntimePlatform.Android:
                    platformName = "Android";
                    break;
                default:
                    break;
            }
#endif
            return platformName;
        }

        public static string GetWWWPath()
        {
            if (HttpServerSettings.GetOrCreateSettings().IsEnable)
            {
                return HttpServerSettings.GetOrCreateSettings().URL + "/" + GetPlatformName();
            }
            else
            {
                var outPath = string.Empty;

                switch (Application.platform)
                {
                    case RuntimePlatform.WindowsPlayer:
                    case RuntimePlatform.OSXPlayer:
                    case RuntimePlatform.WindowsEditor:
                    case RuntimePlatform.OSXEditor:
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
}