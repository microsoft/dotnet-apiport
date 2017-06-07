using ApiPortVS.Contracts;
using Autofac;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.ProjectSystem;
using System;

namespace ApiPortVS.VS2017
{
    public class ServiceProvider : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterAdapter<IServiceProvider, IProjectService>(serviceProvider => {
                var componentModel = serviceProvider.GetService(typeof(SComponentModel)) as IComponentModel;
                var projectServiceAccessor = componentModel.GetService<IProjectServiceAccessor>();
                return projectServiceAccessor.GetProjectService();
            });

            builder.RegisterAdapter<IProjectService, IProjectThreadingService>(service => service.Services.ThreadingPolicy);
            builder.RegisterType<VSThreadingService>()
                .As<IVSThreadingService>();
            builder.RegisterType<ProjectBuilder>()
                .As<IProjectBuilder>();

            base.Load(builder);
        }
    }
}
