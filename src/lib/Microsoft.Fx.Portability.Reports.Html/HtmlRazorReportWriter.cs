// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Internal;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.ObjectPool;
using Microsoft.Fx.Portability.ObjectModel;
using Microsoft.Fx.Portability.Reporting;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;

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

        public void WriteStream(Stream stream, AnalyzeResponse response)
        {
            using (var scope = _factory.CreateScope())
            {
                var model = new RazorHtmlObject(response, _targetMapper);
                var helper = scope.ServiceProvider.GetRequiredService<RazorViewToStringRenderer>();

                helper.RenderViewAsync("Views/ReportTemplate.cshtml", model, stream).GetAwaiter().GetResult();
            }
        }

        private static ServiceProvider BuildProvider()
        {
            var services = new ServiceCollection();
            var fileProvider = new PhysicalFileProvider(Directory.GetCurrentDirectory());

            services.AddSingleton<IHostingEnvironment>(new HostingEnvironment
            {
                ApplicationName = Assembly.GetEntryAssembly().GetName().Name,
                WebRootFileProvider = fileProvider
            });

            services.Configure<RazorViewEngineOptions>(options =>
            {
                options.FileProviders.Clear();
                options.FileProviders.Add(fileProvider);
            });

            services.AddSingleton<ObjectPoolProvider, DefaultObjectPoolProvider>();
            services.AddSingleton<DiagnosticSource>(new DiagnosticListener("Microsoft.AspNetCore"));
            services.AddLogging();
            services.AddMvc();
            services.AddMvc();
            services.AddTransient<RazorViewToStringRenderer>();

            return services.BuildServiceProvider();
        }

        public void Dispose()
        {
            _serviceProvider.Dispose();
        }
    }
}
