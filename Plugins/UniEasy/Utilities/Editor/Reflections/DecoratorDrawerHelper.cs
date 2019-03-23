using System.Reflection;
using UnityEngine;
using System;

namespace UniEasy.Editor
{
    public class DecoratorDrawerHelper
    {
        #region Static Fields

        private static FieldInfo attributeFieldInfo;

        public static FieldInfo AttributeFieldInfo
        {
            get
            {
                if (attributeFieldInfo == null)
                {
                    attributeFieldInfo = TypeHelper.DecoratorDrawerType.GetField("m_Attribute", BindingFlags.Instance | BindingFlags.NonPublic);
                }
                return attributeFieldInfo;
            }
        }

        #endregion

        #region Static Methods

        public static PropertyAttribute GetAttribute(UnityEditor.DecoratorDrawer drawer)
        {
            if (AttributeFieldInfo != null)
            {
                return (PropertyAttribute)AttributeFieldInfo.GetValue(drawer);
            }
            return null;
        }

        public static void SetAttribute(UnityEditor.DecoratorDrawer drawer, PropertyAttribute attr)
        {
            if (AttributeFieldInfo != null)
            {
                AttributeFieldInfo.SetValue(drawer, attr);
            }
        }

        #endregion
    }
}
