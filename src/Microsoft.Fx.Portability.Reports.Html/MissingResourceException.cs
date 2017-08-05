using System;
using System.Globalization;
using System.Reflection;

namespace Microsoft.Fx.Portability.Reports.Html
{
    public class MissingResourceException : Exception
    {
        public MissingResourceException(string resourceName)
            : base(((FormattableString)$"Could not locate: {resourceName}"
                + Environment.NewLine + "Existing resources: "
                + string.Join(", ", typeof(MissingResourceException).GetTypeInfo().Assembly.GetManifestResourceNames()))
                  .ToString(CultureInfo.CurrentCulture))
        { }
    }
}
