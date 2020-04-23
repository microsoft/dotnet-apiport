// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Autofac.Features.OwnedInstances;
using Microsoft.VisualStudio.Shell;
using System;
using System.Runtime.InteropServices;
using System.Windows;

namespace ApiPortVS.Views
{
    [ClassInterface(ClassInterfaceType.AutoDual)]
    [Guid("6D5D3696-1D53-43B9-B848-67B23747A254")] // Every options page should have a unique one
    internal class OptionsPage : UIElementDialogPage
    {
        private readonly Owned<OptionsPageControl> _optionsPageControl;

        public OptionsPage()
        {
#if !DEBUG // Debug builds don't trigger the WPF bug described below
            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
#endif

            _optionsPageControl = ApiPortVSPackage.LocalServiceProvider
                .GetService(typeof(Owned<OptionsPageControl>)) as Owned<OptionsPageControl>;
        }

#if !DEBUG
        // A closed, won't-fix bug in WPF (http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems#id=202873&_a=edit)
        // results in a failed search for this assembly with no PublicKeyToken when this assembly is signed.
        // The below works around this bug by completing the search.
        private System.Reflection.Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            var assemblySimpleName = System.Reflection.Assembly.GetExecutingAssembly().GetName().Name;

            if (args.Name.IndexOf(assemblySimpleName, StringComparison.OrdinalIgnoreCase) != -1)
            {
                AppDomain.CurrentDomain.AssemblyResolve -= CurrentDomain_AssemblyResolve; // only need to do this once

                return System.Reflection.Assembly.GetExecutingAssembly(); // assumes the project was il merged
            }

            return null;
        }
#endif

        protected override UIElement Child { get { return _optionsPageControl.Value; } }

        protected override void Dispose(bool disposing)
        {
            if (_optionsPageControl != null)
            {
                _optionsPageControl.Dispose();
            }

            base.Dispose(disposing);
        }
    }
}
