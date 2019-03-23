using UnityEditor;
using UnityEngine;
using System.IO;

namespace UniEasy.Editor
{
    public class ScriptableObjectUtility
    {
        static public T LoadAtPath<T>(string assetPath) where T : ScriptableObject
        {
            return (T)AssetDatabase.LoadAssetAtPath<ScriptableObject>(assetPath);
        }

        static public T CreateAsset<T>(T asset, string savePath) where T : ScriptableObject
        {
            var directoryName = Path.GetDirectoryName(savePath);
            if (!Directory.Exists(directoryName))
            {
                Directory.CreateDirectory(directoryName);
            }
            // You cannot create assets directly in the streamingassets folder,
            // The assets be created will have some problems and cannot be read,
            // So we need to create assets outside first, then move them to the streamingassets folder.
            if (savePath.Contains(Application.streamingAssetsPath))
            {
                var fileName = Path.GetFileName(savePath);
                var tempPath = string.Format("Assets/{0}", fileName);
                var result = DirectCreateAsset<T>(asset, tempPath);
                var newPath = savePath.Substring(savePath.IndexOf("Assets"));
                AssetDatabase.DeleteAsset(newPath);
                AssetDatabase.MoveAsset(tempPath, newPath);
                AssetDatabase.Refresh();
                return result;
            }
            return DirectCreateAsset<T>(asset, savePath);
        }

        static public T DirectCreateAsset<T>(T asset, string savePath) where T : ScriptableObject
        {
            if (File.Exists(savePath))
            {
                var o = LoadAtPath<ScriptableObject>(savePath);
                var json = JsonUtility.ToJson(asset);
                JsonUtility.FromJsonOverwrite(json, o);
            }
            else
            {
                AssetDatabase.CreateAsset(asset as Object, AssetDatabase.GenerateUniqueAssetPath(savePath));
            }
            AssetDatabase.Refresh();
            var result = LoadAtPath<T>(savePath);
            EditorUtility.SetDirty(result);
            return result;
        }

        public static void CreateAssetWithSavePrompt<T>(T asset, string defaultName = "New ScriptableObject", System.Func<T, bool> postCreated = null) where T : ScriptableObject
        {
            var path = EditorUtility.SaveFilePanel("Save 'ScriptableObject' file to folder", PathsUtility.TryGetSelectedFolderPathInProjectsTab(), defaultName, "asset");
            if (!string.IsNullOrEmpty(path))
            {
                if (path.Contains(Application.dataPath))
                {
                    var savePath = path.Replace(Application.dataPath, "Assets");
                    CreateAsset(asset, savePath);
                    if (postCreated != null && !postCreated(asset))
                    {
                    }
                    else
                    {
                        ProjectWindowUtil.ShowCreatedAsset(AssetDatabase.LoadAssetAtPath<ScriptableObject>(savePath));
                    }
                }
                else
                {
                    Debug.LogError("Sorry, we cannot save 'ScriptableObject' file out of 'Assets' folder!");
                }
            }
        }

        static public void CreateAssetWithSavePrompt(System.Type type, System.Func<ScriptableObject, bool> postCreated = null)
        {
            CreateAssetWithSavePrompt(ScriptableObject.CreateInstance(type), "New ScriptableObject", postCreated);
        }

        public static void CreateAssetWithSavePrompt(string path, System.Type type)
        {
            var go = ScriptableObject.CreateInstance(type);
            var endNameEdit = ScriptableObject.CreateInstance<EndNameEditUtility>();
            endNameEdit.EndNameEditEvent += (instanceID, pathName, resourceFile) =>
            {
                AssetDatabase.CreateAsset(EditorUtility.InstanceIDToObject(instanceID), AssetDatabase.GenerateUniqueAssetPath(pathName));
            };
            ProjectWindowUtil.StartNameEditingIfProjectWindowExists(
                go.GetInstanceID(),
                endNameEdit,
                string.Format("{0}.asset", path),
                EditorGUIUtility.IconContent("ScriptableObject Icon", "").image as Texture2D,
                "");
        }
    }
}
