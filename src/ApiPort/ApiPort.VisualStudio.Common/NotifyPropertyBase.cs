// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading;

namespace ApiPortVS
{
    public class NotifyPropertyBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected SynchronizationContext Context { get; }

        public NotifyPropertyBase()
        {
            // SynchronizationContext.Current will be null if we're not on the UI thread
            Context = SynchronizationContext.Current ?? throw new ArgumentNullException("SynchronizationContext.Current");
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

            if (Context == SynchronizationContext.Current)
            {
                propertyChanged.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
            else
            {
                Context.Post(_ => propertyChanged.Invoke(this, new PropertyChangedEventArgs(propertyName)), null);
            }
        }
    }
}
