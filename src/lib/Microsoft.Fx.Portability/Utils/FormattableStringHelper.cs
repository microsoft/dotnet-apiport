// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Globalization;

namespace Microsoft.Fx.Portability.Utils
{
    public static class FormattableStringHelper
    {
        public static string ToCurrentCulture(FormattableString formattableString) => formattableString.ToString(CultureInfo.CurrentCulture);
    }
}
