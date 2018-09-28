// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

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
            builder.RegisterAdapter<IServiceProvider, IProjectService>(serviceProvider =>
            {
                var componentModel = serviceProvider.GetService(typeof(SComponentModel)) as IComponentModel;
                var projectServiceAccessor = componentModel.GetService<IProjectServiceAccessor>();
                return projectServiceAccessor.GetProjectService();
            })
            .SingleInstance();

            builder.RegisterAdapter<IProjectService, IProjectThreadingService>(service => service.Services.ThreadingPolicy);
            builder.RegisterType<VSThreadingService>()
                .As<IVSThreadingService>()
                .SingleInstance();
            builder.RegisterType<ProjectBuilder2017>()
                .As<IProjectBuilder>()
                .SingleInstance();
        }
    }
}
