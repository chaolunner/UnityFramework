using CustomMenu = UnityEngine.ContextMenu;
using UnityEditor;
using UnityEngine;

namespace UniEasy.Editor
{
    public class BlockContextMenu
    {
        static public EasyBlock CreateBlock(GameObject parent)
        {
            var block = ScriptableObject.CreateInstance<EasyBlock>();
            block.name = parent.name;
            foreach (Transform child in parent.transform)
            {
                var obj = new BlockObject(child.name);
                if (PrefabUtility.GetPrefabType(child.gameObject) != PrefabType.None)
                {
#if UNITY_2018_2_OR_NEWER
                    obj.GameObject = PrefabUtility.GetCorrespondingObjectFromSource(child.gameObject) as GameObject;
#else
                    obj.GameObject = PrefabUtility.GetPrefabParent(child.gameObject) as GameObject;
#endif
                    obj.LocalPosition = child.localPosition;
                    obj.LocalRotation = child.localRotation;
                    obj.LocalScale = child.localScale;
                    block.ToDictionary().Add(child.GetSiblingIndex(), obj);
                }
            }
            return block;
        }

        [CustomMenu("GameObject/UniEasy/Export Block", false, 51)]
        static public void ExportBlock(object obj)
        {
            var block = CreateBlock(obj as GameObject);
            if (block.ToDictionary().Count > 0)
            {
                ScriptableObjectUtility.CreateAssetWithSavePrompt(block, block.name);
            }
        }

        [CustomMenu("GameObject/UniEasy/Export Block Group", false, 52)]
        static public void ExportBlockGroup(object obj)
        {
            var root = obj as GameObject;
            if (root.transform.childCount > 0)
            {
                var path = EditorUtility.SaveFolderPanel("Please select export folder", Application.dataPath, "Rescources");
                if (!string.IsNullOrEmpty(path))
                {
                    if (path.Contains(Application.dataPath))
                    {
                        var assetPath = "";

                        foreach (var go in FormatUtility.FormatGameObjectsName(root.transform))
                        {
                            var asset = CreateBlock(go);
                            if (asset.ToDictionary().Count > 0)
                            {
                                assetPath = path.Replace(Application.dataPath, "Assets");
                                assetPath += "/" + go.name + ".asset";
                                ScriptableObjectUtility.CreateAsset(asset, assetPath);
                            }
                        }
                        ProjectWindowUtil.ShowCreatedAsset(AssetDatabase.LoadAssetAtPath<Object>(assetPath));
                    }
                    else
                    {
                        Debug.LogError("Sorry, we cannot export file out of 'Assets' folder!");
                    }
                }
            }
        }
    }
}
