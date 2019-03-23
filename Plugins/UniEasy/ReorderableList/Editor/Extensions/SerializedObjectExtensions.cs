using UnityEditor;

namespace UniEasy.Editor
{
    public static partial class SerializedObjectExtensions
    {
        public static InspectorMode InspectorMode(this SerializedObject serializedObject)
        {
            return SerializedObjectHelper.GetInspectorMode(serializedObject);
        }
    }
}
