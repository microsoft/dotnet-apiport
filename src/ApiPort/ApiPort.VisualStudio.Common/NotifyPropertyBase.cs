// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Windows.Threading;

namespace ApiPortVS
{
    public class NotifyPropertyBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private readonly SynchronizationContext _context;

        public NotifyPropertyBase()
        {
            _context = new DispatcherSynchronizationContext(Dispatcher.CurrentDispatcher);
        }

        protected void UpdateProperty<T>(ref T field, T value, [CallerMemberName] string propertyName = "")
        {
            if (!EqualityComparer<T>.Default.Equals(field, value))
            {
                field = value;

                OnPropertyUpdated(propertyName);
            }
        }

        protected void OnPropertyUpdated([CallerMemberName]string propertyName = "")
        {
            var propertyChanged = PropertyChanged;
            if (propertyChanged == null)
            {
                return;
            }

            if (_context == SynchronizationContext.Current)
            {
                propertyChanged.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
            else
            {
                _context.Post(_ => propertyChanged.Invoke(this, new PropertyChangedEventArgs(propertyName)), null);
            }
        }
    }
}
