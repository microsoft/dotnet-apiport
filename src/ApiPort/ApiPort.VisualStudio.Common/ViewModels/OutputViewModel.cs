// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using ApiPortVS.Resources;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace ApiPortVS.ViewModels
{
    public class OutputViewModel
    {
        public ObservableCollection<string> Paths { get; }

        public ICommand OpenFile { get; }

        public ICommand SaveAs { get; }

        public ICommand OpenDirectory { get; }

        public OutputViewModel()
            : this(Enumerable.Empty<string>())
        { }

        public OutputViewModel(IEnumerable<string> existingReports)
        {
            Paths = new ObservableCollection<string>(existingReports);
            SaveAs = new DelegateCommand<string>(SaveFileAs);
            OpenFile = new DelegateCommand<string>(path =>
            {
                if (File.Exists(path))
                {
                    Process.Start(path);
                }
                else
                {
                    MessageBox.Show(LocalizedStrings.ReportNotAvailable);
                    Paths.Remove(path);
                }
            });
            OpenDirectory = new DelegateCommand<string>(path =>
            {
                var directory = Path.GetDirectoryName(path);

                if (Directory.Exists(directory))
                {
                    Process.Start(directory);
                }
                else
                {
                    MessageBox.Show(LocalizedStrings.ReportDirectoryNotAvailable);

                    var pathsToRemove = Paths.Where(p => string.Equals(Path.GetDirectoryName(p), directory, StringComparison.OrdinalIgnoreCase)).ToList();

                    foreach (var p in pathsToRemove)
                    {
                        Paths.Remove(p);
                    }
                }
            });
        }

        private static void SaveFileAs(string path)
        {
            try
            {
                var extension = Path.GetExtension(path);

                var fileSaveDialog = new SaveFileDialog
                {
                    AddExtension = true,
                    CheckPathExists = true,
                    DefaultExt = extension,
                    Filter = FormattableString.Invariant($"*{extension}|*{extension}"),
                    FileName = path
                };

                var result = fileSaveDialog.ShowDialog();

                if (result == true)
                {
                    File.Copy(path, fileSaveDialog.FileName);
                }
            }
            catch (Exception)
            {
            }
        }

        private class DelegateCommand<T> : ICommand
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
                if (parameter is T obj)
                {
                    _action(obj);
                }
            }
        }
    }
}
