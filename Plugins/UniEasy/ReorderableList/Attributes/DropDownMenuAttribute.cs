using UnityEngine;
using System;

namespace UniEasy
{
    public class DropdownMenuAttribute : PropertyAttribute
    {
        public Type Type;

        public DropdownMenuAttribute(Type type)
        {
            Type = type;
        }
    }
}
