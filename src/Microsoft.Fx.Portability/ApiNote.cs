// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;

namespace Microsoft.Fx.Portability
{
    public class ApiNote : IComparable<ApiNote>
    {
        public string Id { get; set; }

        public string Title { get; set; }
        
        public string Markdown { get; set; }
        
        public IEnumerable<string> ApplicableApis { get; set; }
        
        public IEnumerable<string> Related { get; set; }

        public int CompareTo(ApiNote other)
        {
            if (other == null)
            {
                return -1;
            }

            return string.CompareOrdinal(Title, other.Title);
        }
    }
}
