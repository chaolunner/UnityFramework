using UnityEngine;
using System;

namespace UniEasy
{
    [Serializable]
    public class BlockObject
    {
        public string BlockName;
        public GameObject GameObject;
        public Vector3 LocalPosition;
        public Quaternion LocalRotation;
        public Vector3 LocalScale;

        public BlockObject(string name)
        {
            BlockName = name;
        }
    }
}
