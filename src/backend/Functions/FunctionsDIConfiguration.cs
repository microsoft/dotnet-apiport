// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using DependencyInjection;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Azure.WebJobs.Host.Config;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Fx.Portability.Azure.Storage;
using Microsoft.Fx.Portability.ObjectModel;
using Microsoft.WindowsAzure.Storage;
using System;

namespace Functions
{
    public class InjectConfiguration : IExtensionConfigProvider
    {
        public virtual void Initialize(ExtensionConfigContext context)
        {
            var services = new ServiceCollection();
            RegisterServices(services);
            var serviceProvider = services.BuildServiceProvider(true);

            context
                .AddBindingRule<InjectAttribute>()
                .Bind(new InjectBindingProvider(serviceProvider));

            var registry = context.Config.GetService<IExtensionRegistry>();
            var filter = new ScopeCleanupFilter();
            registry.RegisterExtension(typeof(IFunctionInvocationFilter), filter);
            registry.RegisterExtension(typeof(IFunctionExceptionFilter), filter);
        }

        private static void RegisterServices(IServiceCollection services)
        {
            var connection = Environment.GetEnvironmentVariable("Connection")
                ?? "UseDevelopmentStorage=true";
            services.AddSingleton(CloudStorageAccount.Parse(connection));
            services.AddScoped<IStorage, AzureStorage>(CreateStorage);
        }

        private static AzureStorage CreateStorage(IServiceProvider arg)
        {
            var account = arg.GetService<CloudStorageAccount>();
            return new AzureStorage(account);
        }
    }
}
