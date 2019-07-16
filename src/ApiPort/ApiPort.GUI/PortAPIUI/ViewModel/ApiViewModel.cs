// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using GalaSoft.MvvmLight;
using Newtonsoft.Json.Linq;
using System;

namespace PortAPIUI.ViewModel
{
    internal class ApiViewModel : ViewModelBase
    {
        private string assemblyName;
        private string apiName;
        private bool compatibility;
        private string changes;


        public ApiViewModel(string name, string apiName, bool compatibility, string changes)
        {
            AssemblyName = name;
            APIName = apiName;
            Compatibility = compatibility;
            Changes = changes;
        }



        public string AssemblyName
        {
            get
            {
                return assemblyName;
            }

            set
            {
                assemblyName = value;
                RaisePropertyChanged(nameof(AssemblyName));
            }
        }

        public string APIName
        {
            get
            {
                return apiName;
            }

            set
            {
                apiName = value;
                RaisePropertyChanged(nameof(APIName));
            }
        }

        public bool Compatibility
        {
            get
            {
                return compatibility;
            }

            set
            {
                compatibility = value;
                RaisePropertyChanged(nameof(Compatibility));
            }
        }
        public string Changes
        {
            get
            {
                return changes;
            }

            set
            {
                changes = value;
                RaisePropertyChanged(nameof(Changes));
            }
        }

    }
}
