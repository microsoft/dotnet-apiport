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

        public string DocId
        {
            get { return _docId ?? String.Empty; }
            set { _docId = value; }
        }
        public string ReturnType
        {
            get { return _returnType ?? String.Empty; }
            set { _returnType = value; }
        }
        public string Name
        {
            get { return _name ?? String.Empty; }
            set { _name = value; }
        }

        public string FullName
        {
            get { return _fullName ?? String.Empty; }
            set { _fullName = value; }
        }

        public override bool Equals(object obj)
        {
            var other = obj as ApiDefinition;

            if (other == null)
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
