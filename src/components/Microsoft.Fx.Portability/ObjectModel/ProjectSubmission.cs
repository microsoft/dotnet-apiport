// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;

namespace Microsoft.Fx.Portability.ObjectModel
{
    public class ProjectSubmission
    {
        public string Name { get; set; }

        public DateTimeOffset SubmittedDate { get; set; }

        public long Length { get; set; }

        public override bool Equals(object obj)
        {
            var other = obj as ProjectSubmission;

            if (other == null) return false;

            return string.Equals(Name, other.Name, StringComparison.Ordinal)
                && SubmittedDate == other.SubmittedDate
                && Length == other.Length;
        }

        public override int GetHashCode()
        {
            return new { Name, SubmittedDate, Length }.GetHashCode();
        }
    }
}
