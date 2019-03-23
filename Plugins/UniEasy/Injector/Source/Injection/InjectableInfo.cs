using System;

namespace UniEasy.DI
{
    public class InjectableInfo
    {
        public readonly Type MemberType;
        public readonly Action<object, object> Setter;
        public readonly object Identifier;
        public readonly Type ObjectType;

        public InjectableInfo(Type memberType, object identifier, Action<object, object> setter, Type objectType)
        {
            Identifier = identifier;
            MemberType = memberType;
            Setter = setter;
            ObjectType = objectType;
        }

        public InjectContext CreateInjectContext(DiContainer container, object targetInstance)
        {
            var context = new InjectContext();

            context.MemberType = MemberType;
            context.Container = container;
            context.ObjectType = ObjectType;
            context.ObjectInstance = targetInstance;
            context.Identifier = Identifier;

            return context;
        }
    }
}
