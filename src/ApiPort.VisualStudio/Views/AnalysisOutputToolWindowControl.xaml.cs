// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using ApiPortVS.ViewModels;
using System.Windows.Controls;

namespace ApiPortVS
{
    public partial class AnalysisOutputToolWindowControl : UserControl
    {
        public static OutputViewModel Model { get; } = new OutputViewModel();

        /// <summary>
        /// Initializes a new instance of the <see cref="AnalysisOutputToolWindowControl"/> class.
        /// </summary>
        public AnalysisOutputToolWindowControl()
        {
            this.InitializeComponent();

            DataContext = Model;
        }
    }
}