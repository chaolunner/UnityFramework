using UnityEditor.Animations;
using UnityEditor;

namespace UniEasy.Editor
{
    public class RemoveActionsUtility
    {
        [MenuItem("Assets/Mecanator/Remove Select Actions", true)]
        [MenuItem("Assets/Mecanator/Clear Actions", true)]
        public static bool IsSelectedAnimatorAsset()
        {
            foreach (var instanceID in Selection.instanceIDs)
            {
                var assetPath = AssetDatabase.GetAssetPath(instanceID);
                var animatorController = AssetDatabase.LoadAssetAtPath<AnimatorController>(assetPath);

                if (animatorController == null)
                {
                    return false;
                }
            }
            return true;
        }

        [MenuItem("Assets/Mecanator/Remove Select Actions", false)]
        public static void RemoveSelectActions()
        {
            AssetsUtility.RemoveSelectAssets<StateMachineAction>();
        }

        [MenuItem("Assets/Mecanator/Clear Actions", false)]
        public static void ClearActions()
        {
            AssetsUtility.ClearAssets<StateMachineAction>();
        }
    }
}
