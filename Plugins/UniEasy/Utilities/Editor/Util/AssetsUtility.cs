using System.Text.RegularExpressions;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.Linq;
using System.IO;

namespace UniEasy.Editor
{
    public class AssetsUtility
    {
        private static int Index;
        private static string Text;
        private static string Guid;
        private static string[] Files;
        private static string MovePath;
        private static Object[] MoveAssetObjects;
        private static List<Object> FoundAssets = new List<Object>();
        private static List<Object> MatchedAssets = new List<Object>();
        private static List<string> AnalysableTypes = new List<string>() { ".fbx", ".mat", ".prefab" };
        private static List<string> ReferencedTypes = new List<string>() { ".fbx", ".mat", ".png", ".tga", ".jpg", ".anim", ".controller", ".overridecontroller" };
        private static List<string> QueryTypes = new List<string>() { ".mat", ".asset", ".unity", "prefab" };

        private static string SearchPatternStr = "*.*";
        private static string FindingAssetsStr = "Finding Assets";
        private static string MatchingAssetsStr = "Matching Assets";
        private static string MovingAssetsStr = "Moving Assets";
        private static string MovingAssetsPath = "{0}/{1}";
        private static string MovingAssetsInfo = "Moving {0} to {1}";

        public static event System.Action<List<Object>> OnMatchAssetsCompleted;
        public static event System.Action<List<Object>> OnFindAssetsCompleted;
        public static event System.Action OnMoveAssetsCompleted;
        public static event System.Action OnMoveAssetsCanceled;

        public static void RemoveSelectAssets<T>()
        {
            ClearAssets((asset, instanceID) =>
            {
                return asset is T && asset.GetInstanceID() == instanceID;
            });
        }

        public static void ClearAssets<T>()
        {
            ClearAssets((asset, instanceID) =>
            {
                return asset is T;
            });
        }

        private static void ClearAssets(System.Func<Object, int, bool> func)
        {
            var removeList = new List<Object>();

            foreach (var instanceID in Selection.instanceIDs)
            {
                var assetPath = AssetDatabase.GetAssetPath(instanceID);
                var assets = AssetDatabase.LoadAllAssetsAtPath(assetPath);

                foreach (var asset in assets)
                {
                    if (asset == null)
                    {
                        // TODO: Not sure how to deal missing assets
                        continue;
                    }
                    if (func != null && func(asset, instanceID) && !removeList.Contains(asset) && !AssetDatabase.IsMainAsset(asset))
                    {
                        removeList.Add(asset);
                    }
                }
            }

            foreach (var item in removeList)
            {
                if (item == null)
                {
                    continue;
                }
                Object.DestroyImmediate(item, true);
            }
            AssetDatabase.SaveAssets();
        }

        public static bool IsAnalysable(Object assetObject)
        {
            if (assetObject != null)
            {
                return AnalysableTypes.Contains(Path.GetExtension(AssetDatabase.GetAssetPath(assetObject)).ToLower());
            }
            return false;
        }

        public static void AnalysisAsset(Object assetObject, string searchPath)
        {
            EditorSettings.serializationMode = SerializationMode.ForceText;
            var path = AssetDatabase.GetAssetPath(assetObject);
            if (!string.IsNullOrEmpty(path))
            {
                Text = File.ReadAllText(path);
                Files = Directory.GetFiles(searchPath, SearchPatternStr, SearchOption.AllDirectories)
                    .Where(filePath => ReferencedTypes.Contains(Path.GetExtension(filePath).ToLower())).ToArray();
                Index = 0;

                MatchedAssets.Clear();
                EditorApplication.update += OnMatchingAssets;
            }
        }

        private static void OnMatchingAssets()
        {
            var file = Files[Index];
            var path = PathsUtility.ConvertFullAbsolutePathToAssetPath(file);
            var isCancel = EditorUtility.DisplayCancelableProgressBar(MatchingAssetsStr, file, (float)Index / Files.Length);

            if (Regex.IsMatch(Text, AssetDatabase.AssetPathToGUID(path)))
            {
                MatchedAssets.Add(AssetDatabase.LoadAssetAtPath(path, typeof(Object)));
            }

            Index++;

            if (isCancel)
            {
                MatchedAssets.Clear();
            }
            if (Index >= Files.Length)
            {
                OnMatchAssetsCompleted(MatchedAssets);
            }
            if (isCancel || Index >= Files.Length)
            {
                EditorUtility.ClearProgressBar();
                EditorApplication.update -= OnMatchingAssets;
                Index = 0;
            }
        }

        public static void MoveAssets(string movePath, params Object[] assetObjects)
        {
            if (!string.IsNullOrEmpty(movePath))
            {
                MovePath = PathsUtility.ConvertFullAbsolutePathToAssetPath(movePath);
                MoveAssetObjects = assetObjects;
                EditorApplication.update += OnMovingAssets;
            }
            else
            {
                OnMoveAssetsCanceled();
            }
        }

        private static void OnMovingAssets()
        {
            var assetObject = MoveAssetObjects[Index];
            var isCancel = EditorUtility.DisplayCancelableProgressBar(MovingAssetsStr, string.Format(MovingAssetsInfo, assetObject.name, MovePath), (float)Index / MoveAssetObjects.Length);
            var path = AssetDatabase.GetAssetPath(assetObject);
            var fileName = Path.GetFileName(path);

            AssetDatabase.MoveAsset(path, string.Format(MovingAssetsPath, MovePath, fileName));

            Index++;

            if (isCancel || Index >= MoveAssetObjects.Length)
            {
                if (isCancel)
                {
                    OnMoveAssetsCanceled();
                }
                else
                {
                    OnMoveAssetsCompleted();
                }
                EditorUtility.ClearProgressBar();
                EditorApplication.update -= OnMovingAssets;
                Index = 0;
            }
        }

        public static void FindReferencesInPath(Object assetObject, string findPath)
        {
            EditorSettings.serializationMode = SerializationMode.ForceText;
            var path = AssetDatabase.GetAssetPath(assetObject);
            if (!string.IsNullOrEmpty(path))
            {
                Guid = AssetDatabase.AssetPathToGUID(path);
                Files = Directory.GetFiles(findPath, SearchPatternStr, SearchOption.AllDirectories)
                    .Where(filePath => QueryTypes.Contains(Path.GetExtension(filePath).ToLower())).ToArray();
                Index = 0;

                FoundAssets.Clear();
                EditorApplication.update += OnFindingAssets;
            }
        }

        private static void OnFindingAssets()
        {
            var file = Files[Index];
            var path = PathsUtility.ConvertFullAbsolutePathToAssetPath(file);
            var isCancel = EditorUtility.DisplayCancelableProgressBar(FindingAssetsStr, file, (float)Index / Files.Length);

            if (Regex.IsMatch(File.ReadAllText(file), Guid))
            {
                FoundAssets.Add(AssetDatabase.LoadAssetAtPath<Object>(path));
            }

            Index++;

            if (isCancel)
            {
                FoundAssets.Clear();
            }
            if (Index >= Files.Length)
            {
                OnFindAssetsCompleted(FoundAssets);
            }
            if (isCancel || Index >= Files.Length)
            {
                EditorUtility.ClearProgressBar();
                EditorApplication.update -= OnFindingAssets;
                Index = 0;
            }
        }
    }
}
