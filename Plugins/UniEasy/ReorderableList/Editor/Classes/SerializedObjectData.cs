using UnityEditor;

namespace UniEasy.Editor
{
    public class SerializedObjectData
    {
        public SerializedObject Object;
        public string Name;
        public string Type;
        public bool Foldout;

        public SerializedObjectData(SerializedObject serializedObject, string name, string type, bool foldout)
        {
            Object = serializedObject;
            Name = name;
            Type = type;
            Foldout = foldout;
        }
    }
}
