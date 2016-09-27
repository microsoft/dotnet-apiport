// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace ApiPortVS.Utils
{
    public class HideWhenTrueBoolConverter : IValueConverter
    {
        private const Visibility Hidden = Visibility.Hidden;
        private const Visibility Visible = Visibility.Visible;

        public HideWhenTrueBoolConverter()
        { }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is bool && ((bool)value) ? Hidden : Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is Visibility && EqualityComparer<Visibility>.Default.Equals((Visibility)value, Hidden);
        }
    }
}
