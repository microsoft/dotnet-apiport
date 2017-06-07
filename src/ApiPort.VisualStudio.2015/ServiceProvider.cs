using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using Microsoft.VisualStudio.ProjectSystem;
using Microsoft.VisualStudio.ComponentModelHost;
using ApiPortVS.Contracts;

namespace ApiPortVS.VS2015
{
    public class ServiceProvider : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterAdapter<IServiceProvider, ProjectService>(serviceProvider => {
                var componentModel = serviceProvider.GetService(typeof(SComponentModel)) as IComponentModel;
                var projectServiceAccessor = componentModel.GetService<IProjectServiceAccessor>();
                return projectServiceAccessor.GetProjectService();
            })
            .SingleInstance();

            builder.RegisterAdapter<ProjectService, IThreadHandling>(service => service.Services.ThreadingPolicy);
            builder.RegisterType<VSThreadingService>()
                .As<IVSThreadingService>()
                .SingleInstance();
            builder.RegisterType<ProjectBuilder>()
                .As<IProjectBuilder>()
                .SingleInstance();

            base.Load(builder);
        }
    }
}
