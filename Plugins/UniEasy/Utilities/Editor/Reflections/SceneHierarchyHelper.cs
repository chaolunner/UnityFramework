using System.Reflection;

namespace UniEasy.Editor
{
    public class SceneHierarchyHelper
    {
#if UNITY_2018_3_OR_NEWER

        #region Static Fields

        private static FieldInfo sceneHierarchy;
        private static FieldInfo treeView;

        #endregion

        #region Static Properties

        public static object SceneHierarchy
        {
            get
            {
                if (sceneHierarchy == null)
                {
                    sceneHierarchy = TypeHelper.SceneHierarchyWindowType.GetField("m_SceneHierarchy", BindingFlags.Instance | BindingFlags.NonPublic);
                }
                if (sceneHierarchy != null)
                {
                    return sceneHierarchy.GetValue(SceneHierarchyWindowHelper.SceneHierarchyWindow);
                }
                return null;
            }
        }

        public static object TreeView
        {
            get
            {
                if (treeView == null)
                {
                    treeView = TypeHelper.SceneHierarchyType.GetField("m_TreeView", BindingFlags.Instance | BindingFlags.NonPublic);
                }
                if (SceneHierarchy != null && treeView != null)
                {
                    return treeView.GetValue(SceneHierarchy);
                }
                return null;
            }
        }

        #endregion

#endif
    }
}
