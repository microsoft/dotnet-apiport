// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Runtime.Versioning;

namespace Microsoft.Fx.Portability.ObjectModel
{
    public interface IApiCatalogLookup
    {
        string BuiltBy { get; }
        ApiDefinition GetApiDefinition(string docId);
        IEnumerable<string> DocIds { get; }
        IEnumerable<TargetInfo> GetAllTargets();
        string GetRecommendedChange(string docId);
        string GetSourceCompatibilityEquivalent(string docId);
        FrameworkName GetLatestVersion(string targetIdentifier);
        IEnumerable<FrameworkName> GetPublicTargets();
        Version GetVersionIntroducedIn(string docId, FrameworkName target);
        bool IsFrameworkAssembly(string assemblyIdentity);
        bool IsFrameworkMember(string docId);
        bool IsMemberInTarget(string docId, FrameworkName targetName, out Version introducedVersion);
        bool IsMemberInTarget(string docId, FrameworkName targetName);
        IEnumerable<FrameworkName> GetSupportedVersions(string docId);
    }
}