using UnityEngine;

namespace UniEasy
{
    [ContextMenuAttribute("Kernel/ISerializableEvent")]
    public interface ISerializableEvent
    {
        Object Source { get; set; }
    }
}
