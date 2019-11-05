// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using Microsoft.Fx.Portability;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ApiPortVS
{
    /// <summary>
    /// The report output format.
    /// </summary>
    public class SelectedResultFormat : ResultFormatInformation, INotifyPropertyChanged
    {
        private bool _isSelected;

        public bool IsSelected
        {
            get
            {
                return _isSelected;
            }

            set
            {
                if (_isSelected == value)
                {
                    return;
                }

                _isSelected = value;
                RaisePropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void RaisePropertyChanged([CallerMemberName]string property = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
        }

        public SelectedResultFormat()
        {
        }

        public SelectedResultFormat(ResultFormatInformation format, bool isSelected)
        {
            DisplayName = format.DisplayName;
            MimeType = format.MimeType;
            FileExtension = format.FileExtension;
            IsSelected = isSelected;
        }
    }
}
