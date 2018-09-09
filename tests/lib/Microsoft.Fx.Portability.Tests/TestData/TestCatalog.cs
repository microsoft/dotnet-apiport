// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Fx.Portability.ObjectModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Versioning;

namespace Microsoft.Fx.Portability.TestData
{
    public class TestCatalog : IApiCatalogLookup
    {
        internal const string DocId1 = "DocId1";

        internal static readonly FrameworkName Target1 = new FrameworkName("Target 1, version=1.0");
        internal static readonly FrameworkName Target2 = new FrameworkName("Target 2, version=1.0");
        internal static readonly FrameworkName Target3 = new FrameworkName("Target 3, version=2.0");

        internal static readonly Dictionary<FrameworkName, TargetInfo> AllTargets = new Dictionary<FrameworkName, TargetInfo>()
        {
            { Target1, new TargetInfo() { DisplayName = Target1, IsReleased = true } },
            { Target2, new TargetInfo() { DisplayName = Target2, IsReleased = true } },
            { Target3, new TargetInfo() { DisplayName = Target3, IsReleased = false } }
        };

        public DateTimeOffset LastModified { get { return DateTimeOffset.UtcNow; } }

        public string BuiltBy
        {
            get { return "Test catalog"; }
        }

        public IEnumerable<TargetInfo> GetAllTargets()
        {
            return AllTargets.Values;
        }

        public static string GetApiMetadata(string docId, string metadataKey)
        {
            return string.Empty;
        }

        public FrameworkName GetLatestVersion(string targetIdentifier)
        {
            foreach (var frameworkName in AllTargets.Keys)
            {
                if (frameworkName.Identifier.Equals(targetIdentifier, StringComparison.Ordinal))
                {
                    return frameworkName;
                }
            }

            return null;
        }

        public IEnumerable<FrameworkName> GetPublicTargets()
        {
            return AllTargets.Where(x => x.Value.IsReleased).Select(x => x.Key);
        }

        public Version GetVersionIntroducedIn(string docId, FrameworkName target)
        {
            if (docId == DocId1)
            {
                return Target1.Version;
            }

            return null;
        }

        public bool IsFrameworkAssembly(string assemblyIdentity)
        {
            return assemblyIdentity == "mscorlib";
        }

        public bool IsFrameworkMember(string docId)
        {
            return docId == DocId1;
        }

        public bool IsMemberInTarget(string docId, FrameworkName targetName, out Version introducedVersion)
        {
            if (docId == DocId1 && targetName.Identifier == Target1.Identifier)
            {
                introducedVersion = Target1.Version;
                return true;
            }

            introducedVersion = null;
            return false;
        }

        public bool IsMemberInTarget(string docId, FrameworkName targetName)
        {
            return IsMemberInTarget(docId, targetName, out var v);
        }

        public string GetRecommendedChange(string docId)
        {
            throw new NotImplementedException();
        }

        public string GetSourceCompatibilityEquivalent(string docId)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<FrameworkName> GetSupportedVersions(string docId)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<string> DocIds
        {
            get { throw new NotImplementedException(); }
        }

        public Microsoft.Fx.Portability.ApiDefinition GetApiDefinition(string docId)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<string> GetAncestors(string docId)
        {
            throw new NotImplementedException();
        }
    }
}
