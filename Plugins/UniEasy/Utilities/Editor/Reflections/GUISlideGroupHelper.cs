using System.Reflection;
using UnityEngine;
using System;

namespace UniEasy.Editor
{
    public class GUISlideGroupHelper
    {
        #region Static Fields

        private static MethodInfo resetMethodInfo;
        private static MethodInfo getRectMethodInfo;

        #endregion

        #region Fields

        private object target;

        #endregion

        #region Constructors

        public GUISlideGroupHelper()
        {
            target = Activator.CreateInstance(TypeHelper.GUISlideGroupType);
        }

        #endregion

        #region Methods

        public void Reset()
        {
            if (resetMethodInfo == null)
            {
                resetMethodInfo = TypeHelper.GUISlideGroupType.GetMethod("Reset", BindingFlags.Instance | BindingFlags.Public);
            }
            if (resetMethodInfo != null)
            {
                resetMethodInfo.Invoke(target, null);
            }
        }

        public Rect GetRect(int id, Rect r)
        {
            if (getRectMethodInfo == null)
            {
                getRectMethodInfo = TypeHelper.GUISlideGroupType.GetMethod("GetRect", BindingFlags.Instance | BindingFlags.Public);
            }
            if (getRectMethodInfo != null)
            {
                return (Rect)getRectMethodInfo.Invoke(target, new object[] { id, r });
            }
            return Rect.zero;
        }

        #endregion
    }
}
