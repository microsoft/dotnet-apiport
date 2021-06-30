// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Fx.Portability.Reports.Html;
using Microsoft.Fx.Portability.Reports.Html.Resources;

using static System.FormattableString;

namespace Microsoft.Fx.Portability.Reports
{
    public static class HtmlHelper
    {
        public static IHtmlContent ConvertMarkdownToHtml<T>(this IHtmlHelper<T> helper, string markdown)
        {
            return helper.Raw(CommonMark.CommonMarkConverter.Convert(markdown));
        }

        public static IHtmlContent Raw<T>(this IHtmlHelper<T> helper, string rawString)
        {
            return new HtmlString(rawString);
        }

        public static IHtmlContent TargetSupportCell<T>(this IHtmlHelper<T> helper, TargetSupportedIn supportStatus)
        {
            var supported = supportStatus.SupportedIn != null
                         && supportStatus.Target.Version >= supportStatus.SupportedIn;

            var imageId = supported ? "icon-supported" : "icon-unsupported";
            var title = supported ? LocalizedStrings.Supported : LocalizedStrings.NotSupported;
            var icon = $"<svg class=\"support-icon\" title=\"{title}\"><use href=#{imageId}></use></svg>";

            return helper.Raw(Invariant($"<td class=\"textCentered\" title=\"{title}\">{icon}</td>"));
        }

        public static IHtmlContent BreakingChangeCountCell<T>(this IHtmlHelper<T> helper, int breaks, int warningThreshold, int errorThreshold)
        {
            var className = string.Empty;
            if (breaks <= warningThreshold)
            {
                className = "NoBreakingChanges";
            }
            else
            {
                className = breaks <= errorThreshold ? "FewBreakingChanges" : "ManyBreakingChanges";
            }

            return helper.Raw(Invariant($"<td class=\"textCentered {className}\">{breaks}</td>"));
        }
    }
}
