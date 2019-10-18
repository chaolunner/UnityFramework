using UnityEngine;
using System;

namespace UniEasy
{
    public class ReorderableAttribute : PropertyAttribute
    {
        public string DisplayName { get; protected set; }

        public string ElementName { get; protected set; }

        public ReorderableAttribute()
        {
            DisplayName = string.Empty;
            ElementName = string.Empty;
        }

        public ReorderableAttribute(string displayName = "", string elementName = "")
        {
            DisplayName = displayName;
            ElementName = elementName;
        }
    }
}
