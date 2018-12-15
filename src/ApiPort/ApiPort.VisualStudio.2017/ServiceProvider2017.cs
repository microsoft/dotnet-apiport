// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using ApiPortVS.Contracts;
using ApiPortVS.VS2017;
using Autofac;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.ProjectSystem;
using Microsoft.VisualStudio.Shell;
using System;

namespace ApiPortVS
{
    public static class ServiceProvider2017
    {
        public static void AddVS2017(this ContainerBuilder builder, IComponentModel componentModel)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            var projectServiceAccessor = componentModel.GetService<IProjectServiceAccessor>();

            builder.RegisterInstance(projectServiceAccessor.GetProjectService());

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
