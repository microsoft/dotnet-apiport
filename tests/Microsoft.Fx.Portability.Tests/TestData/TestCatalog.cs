// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Fx.Portability.ObjectModel;
using System;
using System.Collections.Generic;
using System.Runtime.Versioning;

namespace Microsoft.Fx.Portability.TestData
{
    public class TestCatalog : IApiCatalogLookup
    {
        public string BuiltBy
        {
            get { return "Test catalog"; }
        }

        public IEnumerable<TargetInfo> GetAllTargets()
        {
            yield return new TargetInfo() { DisplayName = new FrameworkName("Target 1, version=1.0"), IsReleased = true };
            yield return new TargetInfo() { DisplayName = new FrameworkName("Target 2, version=1.0"), IsReleased = true };
            yield return new TargetInfo() { DisplayName = new FrameworkName("Target 3, version=2.0"), IsReleased = false };
        }

        public string GetApiMetadata(string docId, string metadataKey)
        {
            return string.Empty;
        }

        public FrameworkName GetLatestVersion(string targetIdentifier)
        {
            if (targetIdentifier == "Target 1")
                return new FrameworkName("Target 1, version=1.0");
            if (targetIdentifier == "Target 2")
                return new FrameworkName("Target 2, version=1.0");
            if (targetIdentifier == "Target 3")
                return new FrameworkName("Target 3, version=2.0");

            return null;
        }

        public IEnumerable<FrameworkName> GetPublicTargets()
        {
            yield return new FrameworkName("Target 1, version=1.0");
            yield return new FrameworkName("Target 2, version=1.0");
        }

        public Version GetVersionIntroducedIn(string docId, FrameworkName target)
        {
            if (docId == "DocId1")
                return new Version("1.0.0.0");

            return null;
        }

        public bool IsFrameworkAssembly(string assemblyIdentity)
        {
            if (assemblyIdentity == "mscorlib")
                return true;

            return false;
        }

        public bool IsFrameworkMember(string docId)
        {
            if (docId == "DocId1")
                return true;

            return false;
        }

        public bool IsMemberInTarget(string docId, FrameworkName targetName, out Version introducedVersion)
        {
            if (docId == "DocId1" && targetName.Identifier == "Target 1")
            {
                introducedVersion = new Version("1.0.0.0");
                return true;
            }

            introducedVersion = null;
            return false;
        }

        public bool IsMemberInTarget(string docId, FrameworkName targetName)
        {
            Version v;
            return IsMemberInTarget(docId, targetName, out v);
        }

        public string GetRecommendedChange(string docId)
        {
            throw new NotImplementedException();
        }

        public string GetSourceCompatibilityEquivalent(string docId)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<FrameworkName> GetSupportedVersions(string DocId)
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
