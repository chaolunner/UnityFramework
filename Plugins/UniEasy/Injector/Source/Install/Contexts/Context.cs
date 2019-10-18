using System.Collections.Generic;
using UnityEngine;

namespace UniEasy.DI
{
    public class Context : MonoBehaviour
    {
        [Reorderable, BackgroundColor("#00808080")]
        public List<MonoInstaller> Installers = new List<MonoInstaller>();
    }
}
