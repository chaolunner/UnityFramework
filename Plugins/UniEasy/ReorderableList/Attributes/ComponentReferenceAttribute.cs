using UnityEngine;
using System;

namespace UniEasy
{
    public class ComponentReferenceAttribute : PropertyAttribute
    {
        public Type DefaultType;

        public ComponentReferenceAttribute() { }

        public ComponentReferenceAttribute(Type type)
        {
            DefaultType = type;
        }
    }
}
