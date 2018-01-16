using ApiPortVS.Attributes;
using ApiPortVS.Contracts;
using ApiPortVS.Resources;
using Microsoft.Fx.Portability;
using Microsoft.VisualStudio.Shell;
using System;
using System.ComponentModel.Composition;
using System.Globalization;
using System.Runtime.InteropServices;

namespace ApiPortVS.Views
{
    // <summary>
    /// Advanced options page located in Tools -> Options window
    /// </summary>
    [Export(typeof(AdvancedOptionsPage))]
    [Guid("6c64e4d4-8b1b-479e-8952-645582f63c6a")]
    public class AdvancedOptionsPage : DialogPage
    {
        private readonly IApiPortService _service;
        private readonly IOutputWindowWriter _outputWindow;

        public AdvancedOptionsPage()
            : base()
        {
            _outputWindow = ApiPortVSPackage.LocalServiceProvider.GetService(typeof(IOutputWindowWriter)) as IOutputWindowWriter
                ?? throw new InvalidOperationException(LocalizedStrings.UnableToResolveOutputWindow);
            _service = ApiPortVSPackage.LocalServiceProvider.GetService(typeof(IApiPortService)) as IApiPortService
                ?? throw new InvalidOperationException(LocalizedStrings.UnableToResolveIApiPortServiceError);

            Endpoint = _service.Endpoint.ToString();
        }

        [LocalizedCategory(nameof(LocalizedStrings.Connection))]
        [LocalizedDisplayName(nameof(LocalizedStrings.EndpointDisplayName))]
        [LocalizedDescription(nameof(LocalizedStrings.EndpointDescription))]
        public string Endpoint { get; set; }

        protected override void OnApply(PageApplyEventArgs e)
        {
            // The user selects Cancel on the Options pane
            if (e.ApplyBehavior != ApplyKind.Apply)
            {
                Endpoint = _service.Endpoint.ToString();
                return;
            }

            if (string.IsNullOrEmpty(Endpoint)
                || !Uri.TryCreate(Endpoint, UriKind.Absolute, out Uri endpoint))
            {
                _outputWindow.WriteLine(string.Format(CultureInfo.CurrentCulture, LocalizedStrings.InvalidEndpointGiven, Endpoint));

                Endpoint = _service.Endpoint.ToString();
                return;
            }

            _service.UpdateEndpoint(endpoint);
            _outputWindow.WriteLine(string.Format(CultureInfo.CurrentCulture, LocalizedStrings.UpdatingEndpoint, Endpoint));
        }
    }
}
