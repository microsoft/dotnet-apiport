// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Fx.Portability.Resources;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Microsoft.Fx.Portability
{
    internal class UrlBuilder
    {
        private readonly string _url;
        private readonly bool _queryStarted;

        public string Url { get { return _url; } }

        private UrlBuilder(string url, bool queryStarted)
        {
            _url = url;
            _queryStarted = queryStarted;
        }

        public static UrlBuilder Create(string endpoint)
        {
            return new UrlBuilder(endpoint, false);
        }

        public UrlBuilder AddQuery<T>(string name, T? value)
            where T : struct
        {
            if (value.HasValue)
            {
                return AddQuery(name, value.Value);
            }
            else
            {
                return this;
            }
        }

        public UrlBuilder AddQueryList(string name, IEnumerable<object> collection)
        {
            if (collection == null)
            {
                return this;
            }

            return collection.Where(i => i != null).Aggregate(this, (aggregate, next) => aggregate.AddQuery(name, next));
        }

        public UrlBuilder AddQuery(string name, object value)
        {
            return AddQuery("{0}{1}{2}={3}", name, value);
        }

        public UrlBuilder AddODataQuery(string name, object value)
        {
            return AddQuery("{0}{1}${2}={3}", name, value);
        }

        public UrlBuilder AddPath(string path)
        {
            if (_queryStarted)
            {
                throw new InvalidOperationException(LocalizedStrings.CannotAddToRunningQueryPath);
            }

            var newUrl = string.Format(CultureInfo.InvariantCulture, "{0}/{1}", _url, Uri.EscapeDataString(path));

            return new UrlBuilder(newUrl, false);
        }

        private UrlBuilder AddQuery(string formatString, string name, object value)
        {
            if (value == null)
            {
                return this;
            }

            var newUrl = string.Format(CultureInfo.InvariantCulture, formatString, _url, _queryStarted ? '&' : '?', Uri.EscapeDataString(name), Uri.EscapeDataString(value.ToString()));

            return new UrlBuilder(newUrl, true);
        }
    }
}
