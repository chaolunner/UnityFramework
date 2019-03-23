using UnityEngine;

namespace UniEasy
{
    public class ReorderableAttribute : PropertyAttribute
    {
        public string DisplayName { get; protected set; }

        public string ElementName { get; protected set; }

        public bool IsDrawObjectReference { get; protected set; }

        public ReorderableAttribute()
        {
            DisplayName = string.Empty;
            ElementName = string.Empty;
            IsDrawObjectReference = true;
        }

        public ReorderableAttribute(string displayName = "", string elementName = "", bool isDrawObjectReference = true)
        {
            DisplayName = displayName;
            ElementName = elementName;
            IsDrawObjectReference = isDrawObjectReference;
        }
    }
}
