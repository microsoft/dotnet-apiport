// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Fx.Portability.ObjectModel;
using Microsoft.Fx.Portability.Reporting;
using RazorEngine.Configuration;
using RazorEngine.Templating;
using RazorEngine.Text;
using System.IO;
using System.Reflection;
using System.Text;
using System;
using Microsoft.Fx.Portability.Reports.Html;
using Microsoft.Fx.Portability.Reports.Html.Resources;

namespace Microsoft.Fx.Portability.Reports
{
    public class HtmlReportWriter : IReportWriter
    {
        private static readonly IRazorEngineService s_razorService = CreateService();

        private readonly ITargetMapper _targetMapper;
        private static readonly ResultFormatInformation _formatInformation = new ResultFormatInformation
        {
            DisplayName = "HTML",
            MimeType = "text/html",
            FileExtension = ".html"
        };

        public HtmlReportWriter(ITargetMapper targetMapper)
        {
            _targetMapper = targetMapper;
        }

        public ResultFormatInformation Format => _formatInformation;

        public void WriteStream(Stream stream, AnalyzeResponse response)
        {
            const string ReportTemplateName = "ReportTemplate";

            using (var writer = new StreamWriter(stream))
            {
                var reportObject = new RazorHtmlObject(response, _targetMapper);
                var mainTemplate = Resolve(ReportTemplateName);
                var razor = s_razorService.RunCompile(mainTemplate, ReportTemplateName, typeof(RazorHtmlObject), reportObject);

                writer.Write(razor);
            }
        }

        private static IRazorEngineService CreateService()
        {
            var config = new TemplateServiceConfiguration
            {
                BaseTemplateType = typeof(HtmlSupportTemplateBase<>)
            };

            return RazorEngineService.Create(config);
        }

        private static string Resolve(string name)
        {
            var fullName = FormattableString.Invariant($"{typeof(HtmlReportWriter).Assembly.GetName().Name}.Resources.{name}.cshtml");
            using (var template = typeof(HtmlReportWriter).GetTypeInfo().Assembly.GetManifestResourceStream(fullName))
            {
                if (template == default(Stream))
                {
                    throw new MissingResourceException(fullName);
                }

                using (var reader = new StreamReader(template, Encoding.UTF8))
                {
                    return reader.ReadToEnd();
                }
            }
        }

        public class HtmlHelper
        {
            // Disabling warning because marking this as static results in Razor
            // compilation errors when generating the Razor page.
#pragma warning disable CA1822 // Mark members as static

            public string ConvertMarkdownToHtml(string markdown)
            {
                return CommonMark.CommonMarkConverter.Convert(markdown);
            }

            public IEncodedString Raw(string rawString)
            {
                return new RawString(rawString);
            }

            public IEncodedString Partial(string name)
            {
                var template = Resolve(name);
                var razor = s_razorService.RunCompile(template, name);

                return Raw(razor);
            }

            public IEncodedString Partial<T>(string name, T model)
            {
                var template = Resolve(name);
                var razor = s_razorService.RunCompile(template, name, typeof(T), model);

                return Raw(razor);
            }

            public IEncodedString TargetSupportCell(TargetSupportedIn supportStatus)
            {
                var supported = supportStatus.SupportedIn != null
                             && supportStatus.Target.Version >= supportStatus.SupportedIn;

                var className = supported ? "IconSuccessEncoded" : "IconErrorEncoded";
                var title = supported ? LocalizedStrings.Supported : LocalizedStrings.NotSupported;

                return Raw($"<td class=\"{className}\" title=\"{title}\"></td>");
            }

            public IEncodedString BreakingChangeCountCell(int breaks, int warningThreshold, int errorThreshold)
            {
                var className = "";
                if (breaks <= warningThreshold)
                {
                    className = "NoBreakingChanges";
                }
                else
                {
                    className = breaks <= errorThreshold ? "FewBreakingChanges" : "ManyBreakingChanges";
                }

                return Raw($"<td class=\"textCentered {className}\">{breaks}</td>");
            }
#pragma warning restore CA1822 // Mark members as static
        }

        public abstract class HtmlSupportTemplateBase<T> : TemplateBase<T>
        {
            public HtmlSupportTemplateBase()
            {
                Html = new HtmlHelper();
            }

            public HtmlHelper Html { get; set; }
        }
    }
}
