// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.ObjectPool;
using Microsoft.Fx.Portability.ObjectModel;
using Microsoft.Fx.Portability.Reporting;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

#if NETSTANDARD2_0
using Microsoft.AspNetCore.Hosting.Internal;
#endif

namespace Microsoft.Fx.Portability.Reports
{
    public sealed class HtmlRazorReportWriter : IReportWriter, IDisposable
    {
        private static readonly ResultFormatInformation _formatInformation = new ResultFormatInformation
        {
            DisplayName = "HTML",
            MimeType = "text/html",
            FileExtension = ".html"
        };

        private readonly ITargetMapper _targetMapper;
        private readonly ServiceProvider _serviceProvider;
        private readonly IServiceScopeFactory _factory;

        public HtmlRazorReportWriter(ITargetMapper targetMapper)
        {
            _targetMapper = targetMapper;
            _serviceProvider = BuildProvider();
            _factory = _serviceProvider.GetRequiredService<IServiceScopeFactory>();
        }

        public ResultFormatInformation Format => _formatInformation;

        public async Task WriteStreamAsync(Stream stream, AnalyzeResponse response)
        {
            using (var scope = _factory.CreateScope())
            {
                var model = new RazorHtmlObject(response, _targetMapper);
                var helper = scope.ServiceProvider.GetRequiredService<RazorViewToStringRenderer>();

                await helper.RenderViewAsync("Views/ReportTemplate.cshtml", model, stream);
            }
        }

        private static ServiceProvider BuildProvider()
        {
            var services = new ServiceCollection();
            var fileProvider = new PhysicalFileProvider(Path.GetDirectoryName(typeof(HtmlRazorReportWriter).Assembly.Location));

#if NETSTANDARD2_0
            services.AddSingleton<IHostingEnvironment>(new HostingEnvironment
            {
                ApplicationName = Assembly.GetEntryAssembly()?.GetName().Name ?? string.Empty,
                WebRootFileProvider = fileProvider,
            });

            services.Configure<RazorViewEngineOptions>(options =>
            {
                options.FileProviders.Clear();
                options.FileProviders.Add(fileProvider);
            });
#endif

            var listener = new DiagnosticListener("Microsoft.AspNetCore.Mvc.Razor");

            services.AddSingleton<DiagnosticSource>(listener);

#if NETCOREAPP3_1
            services.AddSingleton<DiagnosticListener>(listener);
#endif

            services.AddSingleton<ObjectPoolProvider, DefaultObjectPoolProvider>();
            services.AddLogging();
            services
                .AddMvc()
                .ConfigureApplicationPartManager(partsManager => ConfigureRazorViews(partsManager, fileProvider));

            services.AddTransient<RazorViewToStringRenderer>();
            return services.BuildServiceProvider();
        }

        /// <summary>
        /// Checks if the precompiled razor views have been added as an
        /// ApplicationPart. If not, adds them.
        /// On .NET 461, Microsoft.Fx.Portability.Reports.Html.dll
        /// and Microsoft.Fx.Portability.Reports.Html.Views.dll are
        /// not added to the ApplicationPartsManager by default.
        /// For more information: https://github.com/aspnet/Razor/issues/2262.
        /// </summary>
        private static void ConfigureRazorViews(ApplicationPartManager partsManager, PhysicalFileProvider fileProvider)
        {
            // Using StringComparison.OrdinalIgnoreCase because on .NET FX,
            // binaries are output as .DLL sometimes rather than .dll, which
            // leaves us unable to find the files.
            var comparison = StringComparison.OrdinalIgnoreCase;
#if !NET461
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                comparison = StringComparison.Ordinal;
            }
#endif

            var assemblyName = typeof(HtmlRazorReportWriter).Assembly.GetName().Name;
            if (partsManager.ApplicationParts.Any(x => string.Equals(x.Name, assemblyName, comparison)))
            {
                return;
            }

            var applicationParts = fileProvider.GetDirectoryContents(string.Empty)
                .Where(x => x.Exists
                    && Path.GetExtension(x.Name).Equals(".dll", comparison)
                    && x.Name.StartsWith(assemblyName, comparison))
                .SelectMany(file =>
                {
                    var assembly = Assembly.LoadFrom(file.PhysicalPath);

                    return file.Name.EndsWith("Views.dll", comparison)
                        ? CompiledRazorAssemblyApplicationPartFactory.GetDefaultApplicationParts(assembly)
                        : new[] { new AssemblyPart(assembly) };
                });

            foreach (var part in applicationParts)
            {
                partsManager.ApplicationParts.Add(part);
            }
        }

        public void Dispose()
        {
            _serviceProvider.Dispose();
        }
    }
}
