// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;

namespace Microsoft.Fx.Portability
{
    public class ApiDefinition
    {
        private string _docId;
        private string _returnType;
        private string _name;
        private string _fullName;
        private string _parent;

        public string DocId
        {
            get { return _docId ?? string.Empty; }
            set { _docId = value; }
        }

        public string ReturnType
        {
            get { return _returnType ?? string.Empty; }
            set { _returnType = value; }
        }

        public string Name
        {
            get { return _name ?? string.Empty; }
            set { _name = value; }
        }

        public string FullName
        {
            get { return _fullName ?? string.Empty; }
            set { _fullName = value; }
        }

        /// <summary>
        /// Gets or sets the docId of Api's parent
        /// </summary>
        public string Parent
        {
            get { return _parent ?? string.Empty; }
            set { _parent = value; }
        }

        public override bool Equals(object obj)
        {
            if (!(obj is ApiDefinition other))
            {
                return false;
            }

            return string.Equals(DocId, other.DocId, StringComparison.Ordinal);
        }

        public override int GetHashCode()
        {
            return DocId == null ? 0 : DocId.GetHashCode();
        }
    }
}
