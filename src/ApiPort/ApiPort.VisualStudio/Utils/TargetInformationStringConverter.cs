// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using ApiPortVS.Resources;
using System;
using System.Globalization;
using System.Windows.Data;

namespace ApiPortVS.Utils
{
    internal class TargetInformationStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var version = value as TargetPlatformVersion;

            if (version == null)
            {
                return string.Empty;
            }

            return string.Format(CultureInfo.InvariantCulture, LocalizedStrings.TargetPlatformVersionFormat, version.PlatformName, version.Version);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
