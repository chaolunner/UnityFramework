using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;
using System.Reflection;

namespace UniEasy.Editor
{
    public class EditorSceneManagerHelper
    {
        #region Static Fields

        private static MethodInfo getSceneByHandleMethodInfo;

        #endregion

        #region Static Methods

        public static Scene GetSceneByHandle(int instanceID)
        {
            if (getSceneByHandleMethodInfo == null)
            {
                getSceneByHandleMethodInfo = typeof(EditorSceneManager).GetMethod("GetSceneByHandle", BindingFlags.Static | BindingFlags.NonPublic);
            }
            if (getSceneByHandleMethodInfo != null)
            {
                return (Scene)getSceneByHandleMethodInfo.Invoke(null, new object[] { instanceID });
            }
            return default(Scene);
        }

        #endregion
    }
}
