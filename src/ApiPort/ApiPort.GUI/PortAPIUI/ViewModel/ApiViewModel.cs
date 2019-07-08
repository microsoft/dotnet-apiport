// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using GalaSoft.MvvmLight;
using System;

namespace PortAPIUI.ViewModel
{
    internal class ApiViewModel : ViewModelBase
    {
        private string assemblyName1;
        private string apiName;
        private bool compatibility1;

        public ApiViewModel(string name, string apiName, bool compatibility)
        {
            AssemblyName1 = name;
            ApiName = apiName;
            Compatibility1 = compatibility;
        }

        public string AssemblyName
        {
            get
            {
                return AssemblyName1;
            }

            set
            {
                AssemblyName1 = value;
                RaisePropertyChanged(nameof(AssemblyName));
            }
        }

        public string APIName
        {
            get
            {
                return ApiName;
            }

            set
            {
                ApiName = value;
                RaisePropertyChanged(nameof(APIName));
            }
        }

        public bool Compatibility
        {
            get
            {
                return Compatibility1;
            }

            set
            {
                Compatibility1 = value;
                RaisePropertyChanged(nameof(Compatibility));
            }
        }

        public string AssemblyName1 { get => assemblyName1; set => assemblyName1 = value; }

        public string ApiName { get => apiName; set => apiName = value; }

        public bool Compatibility1 { get => compatibility1; set => compatibility1 = value; }
    }
}
