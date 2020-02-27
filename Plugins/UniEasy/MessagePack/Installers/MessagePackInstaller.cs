using MessagePack.Resolvers;
using MessagePack;
using UniEasy.DI;

namespace UniEasy
{
    public class MessagePackInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            var resolver = CompositeResolver.Create(GeneratedResolver.Instance, StandardResolver.Instance);
            var options = MessagePackSerializerOptions.Standard.WithResolver(resolver);
            MessagePackSerializer.DefaultOptions = options;
        }
    }
}
