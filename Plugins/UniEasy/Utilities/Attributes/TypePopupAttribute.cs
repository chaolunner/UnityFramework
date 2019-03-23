using UnityEngine;
using System;

namespace UniEasy
{
    public class TypePopupAttribute : PropertyAttribute
    {
        public Type Type;

        public TypePopupAttribute(Type type)
        {
            Type = type;
        }
    }
}
