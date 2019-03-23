using System.Collections;
using System.Reflection;
using UnityEditor;

namespace UniEasy.Editor
{
    public class SceneHierarchyWindowHelper
    {
        #region Static Fields

        private static EditorWindow sceneHierarchyWindow;
        private static FieldInfo searchFilter;
        private static PropertyInfo treeView;
        private static MethodInfo searchChanged;
        private static MethodInfo reloadData;
        private static MethodInfo createGameObjectContextClick;
        private static MethodInfo createMultiSceneHeaderContextClick;

        #endregion

        #region Static Properties

        protected static EditorWindow SceneHierarchyWindow
        {
            get
            {
                if (sceneHierarchyWindow == null)
                {
                    sceneHierarchyWindow = EditorWindow.GetWindow(TypeHelper.SceneHierarchyWindow, false, "Hierarchy", true);
                }
                if (sceneHierarchyWindow != null)
                {
                    return sceneHierarchyWindow;
                }
                return null;
            }
        }

        protected static object TreeView
        {
            get
            {
                if (treeView == null)
                {
                    treeView = TypeHelper.SceneHierarchyWindow.GetProperty("treeView", BindingFlags.NonPublic | BindingFlags.Instance);
                }
                if (treeView != null)
                {
                    return treeView.GetValue(SceneHierarchyWindow, null);
                }
                return null;
            }
        }

        protected static object Data
        {
            get
            {
                if (TreeView != null)
                {
                    return TreeView.GetType().GetProperty("data").GetValue(TreeView, null);
                }
                return null;
            }
        }

        public static string SearchFilter
        {
            get
            {
                if (searchFilter == null)
                {
                    searchFilter = TypeHelper.SceneHierarchyWindow.GetField("m_SearchFilter", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                }
                if (searchFilter != null)
                {
                    return searchFilter.GetValue(SceneHierarchyWindow).ToString();
                }
                return null;
            }
            set
            {
                if (searchFilter == null)
                {
                    searchFilter = TypeHelper.SceneHierarchyWindow.GetField("m_SearchFilter", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                }
                if (searchFilter != null)
                {
                    searchFilter.SetValue(SceneHierarchyWindow, value);
                }
            }
        }

        protected static FieldInfo Rows
        {
            get
            {
                if (Data != null)
                {
                    return Data.GetType().GetField("m_Rows", BindingFlags.NonPublic | BindingFlags.Instance);
                }
                return null;
            }
        }

        protected static FieldInfo RowCount
        {
            get
            {
                if (Data != null)
                {
                    return Data.GetType().GetField("m_RowCount", BindingFlags.NonPublic | BindingFlags.Instance);
                }
                return null;
            }
        }

        #endregion

        #region Static Methods

        public static void CreateGameObjectContextClick(GenericMenu menu, int contextClickedItemID)
        {
            if (createGameObjectContextClick == null)
            {
                createGameObjectContextClick = TypeHelper.SceneHierarchyWindow.GetMethod("CreateGameObjectContextClick", BindingFlags.Instance | BindingFlags.NonPublic);
            }
            if (createGameObjectContextClick != null)
            {
                createGameObjectContextClick.Invoke(SceneHierarchyWindow, new object[] { menu, contextClickedItemID });
            }
        }

        public static void CreateMultiSceneHeaderContextClick(GenericMenu menu, int contextClickedItemID)
        {
            if (createMultiSceneHeaderContextClick == null)
            {
                createMultiSceneHeaderContextClick = TypeHelper.SceneHierarchyWindow.GetMethod("CreateMultiSceneHeaderContextClick", BindingFlags.Instance | BindingFlags.NonPublic);
            }
            if (createMultiSceneHeaderContextClick != null)
            {
                createMultiSceneHeaderContextClick.Invoke(SceneHierarchyWindow, new object[] { menu, contextClickedItemID });
            }
        }

        public static void Show()
        {
            SceneHierarchyWindow.Show();
        }

        public static void Repaint()
        {
            SceneHierarchyWindow.Repaint();
        }

        public static void SetRows(object value)
        {
            Rows.SetValue(Data, value);
        }

        public static void SetRowCount(int count)
        {
            RowCount.SetValue(Data, count);
        }

        public static IEnumerable GetRows()
        {
            return Rows.GetValue(Data) as IEnumerable;
        }

        public static void ReloadData()
        {
            if (reloadData == null)
            {
                reloadData = TypeHelper.SceneHierarchyWindow.GetMethod("ReloadData");
            }
            if (reloadData != null)
            {
                reloadData.Invoke(SceneHierarchyWindow, null);
            }
        }

        public static void SearchChanged()
        {
            if (searchChanged == null)
            {
                searchChanged = TypeHelper.SceneHierarchyWindow.GetMethod("SearchChanged");
            }
            if (searchChanged != null)
            {
                searchChanged.Invoke(SceneHierarchyWindow, null);
            }
        }

        #endregion
    }
}
