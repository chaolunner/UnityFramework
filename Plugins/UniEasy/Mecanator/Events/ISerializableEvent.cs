using System.Collections.Generic;
using UnityEngine;

namespace UniEasy
{
    [ContextMenuAttribute("Kernel/ISerializableEvent")]
    public interface ISerializableEvent
    {
        Object Source { get; set; }
        List<GameObject> References { get; set; }
    }
}
