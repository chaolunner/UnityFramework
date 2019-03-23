using System.Collections.Generic;
using System;

namespace UniEasy
{
    public static partial class StringExtensions
    {
        #region Static Fields

        private static Dictionary<string, Type> TypesIndex = new Dictionary<string, Type>();

        #endregion

        #region Static Methods

        public static Type GetTypeWithAssembly(this string typeName)
        {
            var type = Type.GetType(typeName);
            if (type != null)
            {
                return type;
            }
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                type = assembly.GetType(typeName);
                if (type != null)
                {
                    return type;
                }
            }
            return null;
        }

        public static Type GetTypeFromCached(this string typeName)
        {
            Type type = null;
            if (!TypesIndex.TryGetValue(typeName, out type))
            {
                type = GetTypeWithAssembly(typeName);
                TypesIndex.Add(typeName, type);
            }
            return type;
        }

        #endregion
    }
}
