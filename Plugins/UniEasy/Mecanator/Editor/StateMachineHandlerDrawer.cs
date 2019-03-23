using UnityEditor;

namespace UniEasy.Editor
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(StateMachineHandler), true)]
    public class StateMachineHandlerDrawer : ReorderableListDrawer
    {
    }
}
