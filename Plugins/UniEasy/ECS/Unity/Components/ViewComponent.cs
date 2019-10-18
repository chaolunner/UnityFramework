using System.Collections.Generic;
using UnityEngine;

namespace UniEasy.ECS
{
    [ContextMenuAttribute("ECS/ViewComponent")]
    public class ViewComponent : RuntimeComponent
    {
        [Reorderable]
        public List<Transform> Transforms;
    }
}
