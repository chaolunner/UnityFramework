using System.Collections.Generic;
using System.Collections;
using System;

namespace UniEasy.Editor
{
    public class RuntimeSerializedPropertyIterator : IEnumerator, IDisposable, IEnumerator<object>
    {
        public int index;
        public RuntimeSerializedProperty target;
        public RuntimeSerializedProperty end;
        public object current;
        public bool disposing;
        public int position;

        object IEnumerator<object>.Current
        {
            get
            {
                return current;
            }
        }

        object IEnumerator.Current
        {
            get
            {
                return current;
            }
        }

        public RuntimeSerializedPropertyIterator()
        {
        }

        public void Dispose()
        {
            disposing = true;
            position = -1;
        }

        public bool MoveNext()
        {
            uint num = (uint)position;
            position = -1;
            switch (num)
            {
                case 0:
                    if (!target.IsArray)
                    {
                        end = target.GetEndProperty();
                        if (target.NextVisible(true) && !RuntimeSerializedProperty.EqualContents(target, end))
                        {
                            current = target;
                            if (!disposing)
                            {
                                position = 2;
                            }
                            return true;
                        }
                        position = -1;
                        return false;
                    }
                    index = 0;
                    break;
                case 1:
                    index++;
                    break;
                case 2:
                    if (target.NextVisible(true) && !RuntimeSerializedProperty.EqualContents(target, end))
                    {
                        current = target;
                        if (!disposing)
                        {
                            position = 2;
                        }
                        return true;
                    }
                    position = -1;
                    return false;
                default:
                    return false;
            }
            if (index >= target.ArraySize)
            {
                position = -1;
                return false;
            }
            current = target.GetArrayElementAtIndex(index);
            if (!disposing)
            {
                position = 1;
            }
            return true;
        }

        public void Reset()
        {
            throw new NotSupportedException();
        }
    }
}
