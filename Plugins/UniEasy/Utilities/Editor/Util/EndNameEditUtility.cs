using UnityEditor.ProjectWindowCallback;
using System;

namespace UniEasy.Editor
{
    internal class EndNameEditUtility : EndNameEditAction
    {
        public event Action<int, string, string> EndNameEditEvent;

        public override void Action(int instanceID, string pathName, string resourceFile)
        {
            if (EndNameEditEvent != null)
            {
                EndNameEditEvent(instanceID, pathName, resourceFile);
            }
        }
    }
}
