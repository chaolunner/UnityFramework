using System.Collections.Generic;
using UnityEngine;

namespace UniEasy.DI
{
    public class Context : MonoBehaviour
    {
        [Reorderable(elementName: null), BackgroundColor("#00808080")]
        public List<MonoInstaller> Installers = new List<MonoInstaller>();
    }
}
