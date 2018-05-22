// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Windows;
using System.Windows.Controls;

namespace ApiPortVS
{
    public partial class AnalysisOutputToolWindowControl : UserControl
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AnalysisOutputToolWindowControl"/> class.
        /// </summary>
        public AnalysisOutputToolWindowControl()
        {
            InitializeComponent();
        }

        private void ListView_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if ((bool)e.NewValue && sender is ListView listView)
            {
                listView.SelectedIndex = 0;
            }
        }
    }
}
