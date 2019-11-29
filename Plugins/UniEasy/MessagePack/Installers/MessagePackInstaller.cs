using MessagePack.Resolvers;
using MessagePack;
using UniEasy.DI;

namespace UniEasy
{
    public class MessagePackInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            StaticCompositeResolver.Register(GeneratedResolver.Instance, StandardResolver.Instance);
            var options = MessagePackSerializerOptions.Standard.WithResolver(StaticCompositeResolver.Instance);
            MessagePackSerializer.DefaultOptions = options;
        }
    }
}
