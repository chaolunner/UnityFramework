using UnityEngine;
namespace UniEasy
{
    [System.Serializable, ContextMenuAttribute("Kernel/SerializableEvent")]
    public class SerializableEvent : ISerializableEvent
    {
        public Object Source { get; set; }
    }
}
