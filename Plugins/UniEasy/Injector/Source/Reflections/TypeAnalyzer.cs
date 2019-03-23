using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using System;

namespace UniEasy.DI
{
    public class TypeAnalyzer
    {
        static Dictionary<Type, EasyInjectInfo> typeInfo = new Dictionary<Type, EasyInjectInfo>();

        static public EasyInjectInfo GetInfo<T>()
        {
            return GetInfo(typeof(T));
        }

        static public EasyInjectInfo GetInfo(Type type)
        {
            EasyInjectInfo info;
            if (!typeInfo.TryGetValue(type, out info))
            {
                info = CreateTypeInfo(type);
                typeInfo.Add(type, info);
            }
            return info;
        }

        static EasyInjectInfo CreateTypeInfo(Type type)
        {
            var constructor = GetInjectConstructor(type);

            return new EasyInjectInfo(
                GetFieldInjectables(type).ToList(),
                GetPropertyInjectables(type).ToList(),
                GetPostInjectMethods(type),
                constructor,
                GetConstructorInjectables(type, constructor).ToList()
            );
        }

        static IEnumerable<InjectableInfo> GetConstructorInjectables(Type parentType, ConstructorInfo constructorInfo)
        {
            if (constructorInfo == null)
            {
                return Enumerable.Empty<InjectableInfo>();
            }

            return constructorInfo.GetParameters().Select(
                paramInfo => CreateInjectableInfoForParam(parentType, paramInfo));
        }

        static IEnumerable<InjectableInfo> GetFieldInjectables(Type type)
        {
            var fieldInfos = type.GetAllInstanceFields()
                .Where(f => f.HasAttribute(typeof(InjectAttribute))).ToArray();

            for (int i = 0; i < fieldInfos.Length; i++)
            {
                yield return CreateForMember(fieldInfos[i], type);
            }
        }

        static IEnumerable<InjectableInfo> GetPropertyInjectables(Type type)
        {
            var propInfos = type.GetAllInstanceProperties()
                .Where(p => p.HasAttribute(typeof(InjectAttribute))).ToArray();

            for (int i = 0; i < propInfos.Length; i++)
            {
                yield return CreateForMember(propInfos[i], type);
            }
        }

        static InjectableInfo CreateForMember(MemberInfo memberInfo, Type parentType)
        {
            var injectAttributes = memberInfo.AllAttributes<InjectAttribute>().ToList();
            var injectAttr = injectAttributes.SingleOrDefault();
            object identifier = null;
            if (injectAttr != null)
            {
                identifier = injectAttr.Id;
            }

            Type memberType;
            Action<object, object> setter;
            if (memberInfo is FieldInfo)
            {
                var fieldInfo = memberInfo as FieldInfo;
                setter = ((object injectable, object value) => fieldInfo.SetValue(injectable, value));
                memberType = fieldInfo.FieldType;
            }
            else
            {
                var propInfo = memberInfo as PropertyInfo;
                setter = ((object injectable, object value) => propInfo.SetValue(injectable, value, null));
                memberType = propInfo.PropertyType;
            }
            return new InjectableInfo(memberType, identifier, setter, parentType);
        }

        static ConstructorInfo GetInjectConstructor(Type parentType)
        {
            var constructors = parentType.Constructors();

            if (constructors.IsEmpty())
            {
                return null;
            }

            if (constructors.HasMoreThan(1))
            {
                var explicitConstructor = (from c in constructors
                                           where c.HasAttribute<InjectAttribute>()
                                           select c).SingleOrDefault();

                if (explicitConstructor != null)
                {
                    return explicitConstructor;
                }

                // If there is only one public constructor then use that
                // This makes decent sense but is also necessary on WSA sometimes since the WSA generated
                // constructor can sometimes be private with zero parameters
                var singlePublicConstructor = constructors.Where(x => !x.IsPrivate).OnlyOrDefault();

                if (singlePublicConstructor != null)
                {
                    return singlePublicConstructor;
                }

                return null;
            }

            return constructors[0];
        }

        static List<PostInjectableInfo> GetPostInjectMethods(Type type)
        {
            // Note that unlike with fields and properties we use GetCustomAttributes
            // This is so that we can ignore inherited attributes, which is necessary
            // otherwise a base class method marked with [Inject] would cause all overridden
            // derived methods to be added as well
            var methods = type.GetAllInstanceMethods()
                .Where(t => t.GetCustomAttributes(typeof(InjectAttribute), false).Any()).ToList();

            var heirarchyList = type.Yield().Concat(type.GetParentTypes()).Reverse().ToList();

            // Order by base classes first
            // This is how constructors work so it makes more sense
            var values = methods.OrderBy(m => heirarchyList.IndexOf(m.DeclaringType));

            var postInjectInfos = new List<PostInjectableInfo>();

            var methodInfos = values.ToArray();
            for (int i = 0; i < methodInfos.Length; i++)
            {
                var paramsInfo = methodInfos[i].GetParameters();

                postInjectInfos.Add(
                    new PostInjectableInfo(
                        methodInfos[i],
                        paramsInfo.Select(paramInfo =>
                           CreateInjectableInfoForParam(type, paramInfo)).ToList()));
            }

            return postInjectInfos;
        }

        static InjectableInfo CreateInjectableInfoForParam(Type parentType, ParameterInfo paramInfo)
        {
            var injectAttributes = paramInfo.AllAttributes<InjectAttribute>().ToList();

            var injectAttr = injectAttributes.SingleOrDefault();

            object identifier = null;

            if (injectAttr != null)
            {
                identifier = injectAttr.Id;
            }

            return new InjectableInfo(
                paramInfo.ParameterType,
                identifier,
                null,
                parentType);
        }
    }
}
