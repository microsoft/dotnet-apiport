// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Globalization;
using System.Windows.Data;

namespace ApiPortVS.Utils
{
    internal class TargetPlatformDisplayNameConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var target = value as TargetPlatform;

            if (target == null)
            {
                return string.Empty;
            }

            if (target.AlternativeNames.Count > 0)
            {
                return FormattableString.Invariant($"{target.Name} ({string.Join(", ", target.AlternativeNames)})");
            }
            else
            {
                return target.Name;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
