// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Fx.Portability.Reports.Html.Resources;
using System;
using System.Globalization;
using System.Reflection;

namespace Microsoft.Fx.Portability.Reports.Html
{
    public class MissingResourceException : Exception
    {
        public MissingResourceException(string resourceName)
            : base((string.Format(CultureInfo.CurrentCulture, LocalizedStrings.CouldNotLocate, resourceName)
                + Environment.NewLine + LocalizedStrings.ExistingResources
                + string.Join(", ", typeof(MissingResourceException).GetTypeInfo().Assembly.GetManifestResourceNames()))
                  .ToString(CultureInfo.CurrentCulture))
        { }
    }
}
