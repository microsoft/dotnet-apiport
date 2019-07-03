using GalaSoft.MvvmLight;
using System;

namespace PortAPIUI.ViewModel
{
    internal class ApiViewModel : ViewModelBase
    {
        public string _assemblyName;
        public string _apiName;
        public Boolean _compatibility;

        public ApiViewModel(string name, string apiName, Boolean compatibility)
        {
            _assemblyName = name;
            _apiName = apiName;
            _compatibility = compatibility;
        }

        public string AssemblyName
        {
            get
            {
                return _assemblyName;
            }
            set
            {
                _assemblyName = value;
                RaisePropertyChanged("AssemblyName");
            }
        }
        public string APIName
        {
            get
            {
                return _apiName;
            }
            set
            {
                _apiName = value;
                RaisePropertyChanged("APIName");
            }
        }
        public Boolean Compatibility
        {
            get
            {
                return _compatibility;
            }
            set
            {
                _compatibility = value;
                RaisePropertyChanged("Compatibility");
            }
        }


    }
}
