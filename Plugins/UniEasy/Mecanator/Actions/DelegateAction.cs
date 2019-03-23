using UnityEngine;

namespace UniEasy
{
    public abstract class DelegateAction : ScriptableObject
    {
        public abstract void Execute(object data);
    }
}
