using System.Collections.Generic;
using System.Collections;
using System;

namespace UniEasy.DI
{
    public static class ReflectionUtility
    {
        public static IList CreateGenericList(Type elementType, object[] contentsAsObj)
        {
            var genericType = typeof(List<>).MakeGenericType(elementType);

            var list = (IList)Activator.CreateInstance(genericType);

            for (int i = 0; i < contentsAsObj.Length; i++)
            {
                list.Add(contentsAsObj[i]);
            }

            return list;
        }
    }
}
