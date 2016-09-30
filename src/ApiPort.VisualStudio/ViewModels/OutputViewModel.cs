// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Win32;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Windows.Input;

namespace ApiPortVS.ViewModels
{
    public class OutputViewModel
    {
        public ObservableCollection<string> Paths { get; }

        public ICommand OpenFile { get; }

        public ICommand SaveAs { get; }

        public OutputViewModel()
        {
            Paths = new ObservableCollection<string>();
            OpenFile = new DelegateCommand<string>(path => Process.Start(path));
            SaveAs = new DelegateCommand<string>(SaveFileAs);
        }

        private void SaveFileAs(string path)
        {
            try
            {
                var extension = Path.GetExtension(path);

                var fileSaveDialog = new SaveFileDialog
                {
                    AddExtension = true,
                    CheckPathExists = true,
                    DefaultExt = extension,
                    Filter = $"*{extension}|*{extension}",
                    FileName = path
                };

                var result = fileSaveDialog.ShowDialog();

                if (result == true)
                {
                    File.Copy(path, fileSaveDialog.FileName);
                }
            }
            catch (Exception) { }
        }

        private class DelegateCommand<T> : ICommand
            where T : class
        {
            private readonly Action<T> _action;

#pragma warning disable CS0067
            public event EventHandler CanExecuteChanged;
#pragma warning restore CS0067

            public DelegateCommand(Action<T> action)
            {
                _action = action;
            }

            public bool CanExecute(object parameter) => true;

            public void Execute(object parameter)
            {
                var obj = parameter as T;

                if (obj != null)
                {
                    _action(obj);
                }
            }
        }
    }
}
