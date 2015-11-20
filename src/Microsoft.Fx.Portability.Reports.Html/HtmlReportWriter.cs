// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using MarkdownDeep;
using Microsoft.Fx.Portability.ObjectModel;
using Microsoft.Fx.Portability.Reporting;
using Microsoft.Fx.Portability.Reporting.ObjectModel;
using RazorEngine.Configuration;
using RazorEngine.Templating;
using RazorEngine.Text;
using System.IO;
using System.Reflection;
using System.Text;

namespace Microsoft.Fx.Portability.Reports
{
    public class HtmlReportWriter : IReportWriter
    {
        private static readonly IRazorEngineService s_razorService = CreateService();

        private readonly ITargetMapper _targetMapper;
        private readonly ResultFormatInformation _formatInformation;

        public HtmlReportWriter(ITargetMapper targetMapper)
        {
            _targetMapper = targetMapper;

            _formatInformation = new ResultFormatInformation
            {
                DisplayName = "HTML",
                MimeType = "text/html",
                FileExtension = ".html"
            };
        }

        public ResultFormatInformation Format { get { return _formatInformation; } }

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
            var names = typeof(HtmlReportWriter).GetTypeInfo().Assembly.GetManifestResourceNames();
            using (var template = typeof(HtmlReportWriter).GetTypeInfo().Assembly.GetManifestResourceStream("Microsoft.Fx.Portability.Reports.Resources." + name + ".cshtml"))
            using (var reader = new StreamReader(template, Encoding.UTF8))
            {
                return reader.ReadToEnd();
            }
        }

        public class HtmlHelper
        {
            private readonly Markdown _md = new Markdown();

            public string ConvertMarkdownToHtml(string markdown)
            {
                return _md.Transform(markdown);
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