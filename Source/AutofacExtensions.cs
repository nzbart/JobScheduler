using Autofac;

namespace JobScheduler
{
    static class AutofacExtensions
    {
        public static void RegisterSelf(this ContainerBuilder builder)
        {
            IContainer container = null;
            builder.Register(c => container).AsSelf();
            builder.RegisterBuildCallback(c => container = c);
        }
    }
}
