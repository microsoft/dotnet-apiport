using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reflection;
using System.Text;

namespace PortAPIUI.ViewModel
{
    internal class ApiViewModel : ViewModelBase
    {
        public string _name;
        public Boolean _compatibility;


        public override string ToString()
        {
            return _name;
        }

        public ApiViewModel(string assembly)
        {
            _name = assembly;
        }

        public string Name
        {
            get
            {
                return _name;
            }
            set
            {
                _name = value;
                RaisePropertyChanged("Name");
            }
        }
        
    }
}
