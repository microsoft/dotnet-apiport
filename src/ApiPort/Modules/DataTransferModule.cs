using Autofac;
using Microsoft.Fx.Portability;

namespace ApiPort.Modules
{
    /// <summary>
    /// Module that is registered when user wants to see the data transmitted.
    /// https://github.com/Microsoft/dotnet-apiport/blob/master/docs/Console/README.md#see-the-data-being-transmitted
    /// </summary>
    public class DataTransferModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<FileOutputApiPortService>()
                .As<IApiPortService>()
                .SingleInstance();

            base.Load(builder);
        }
    }
}
