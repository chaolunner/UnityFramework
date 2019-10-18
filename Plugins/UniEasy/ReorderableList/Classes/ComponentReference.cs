using UnityEngine;
using System;

namespace UniEasy
{
    [Serializable]
    public class ComponentReference
    {
        public string Name;
        public bool IsExpanded;
        public GameObject Target;
        public Component Component;
    }
}
