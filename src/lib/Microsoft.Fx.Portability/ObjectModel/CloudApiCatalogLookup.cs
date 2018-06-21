// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Versioning;

namespace Microsoft.Fx.Portability.ObjectModel
{
    /// <summary>
    /// This represents an object that contains the precomputed set of all target APIs that we ever shipped in the framework
    /// </summary>
    public class CloudApiCatalogLookup : IApiCatalogLookup
    {
        private const string RecommendedChangeKey = "Recommended Changes";
        private const string SourceCompatibleEquivalent = "SourceCompatibleEquivalent";

        private readonly Dictionary<string, Dictionary<string, Version>> _apiMapping;
        private readonly Dictionary<string, Dictionary<string, string>> _apiMetadata;
        private readonly Dictionary<string, FrameworkName> _latestTargetVersion;
        private readonly ICollection<string> _frameworkAssemblies;
        private readonly IReadOnlyCollection<FrameworkName> _publicTargets;
        private readonly IReadOnlyCollection<TargetInfo> _allTargets;
        private readonly IDictionary<string, ApiDefinition> _docIdToApi;

        public CloudApiCatalogLookup(DotNetCatalog catalog)
        {
            LastModified = catalog.LastModified;
            BuiltBy = catalog.BuiltBy;

            // we want to recreate the fast look-up data structures.
            _apiMapping = catalog.Apis.AsParallel()
                .ToDictionary(key => key.DocId,
                                value => value.Targets.ToDictionary(innerkey => innerkey.Identifier,
                                                                        innervalue => innervalue.Version, StringComparer.OrdinalIgnoreCase));

            _apiMetadata = catalog.Apis.AsParallel()
                            .Where(api => api.Metadata != null)
                            .ToDictionary(
                                key => key.DocId,
                                value => value.Metadata.ToDictionary(
                                            innerKey => innerKey.MetadataKey,
                                            innerValue => innerValue.Value,
                                            StringComparer.Ordinal),
                                StringComparer.Ordinal);

            _publicTargets = catalog.SupportedTargets
                                                .Where(sp => sp.IsReleased)
                                                .Select(sp => sp.DisplayName)
                                                .ToList();

            _latestTargetVersion = catalog.SupportedTargets
                                                .GroupBy(sp => sp.DisplayName.Identifier)
                                                .ToDictionary(key => key.Key, value => value.OrderByDescending(p => p.DisplayName.Version).First().DisplayName, StringComparer.OrdinalIgnoreCase);

            _allTargets = catalog.SupportedTargets;

            _frameworkAssemblies = new HashSet<string>(catalog.FrameworkAssemblyIdenties, StringComparer.OrdinalIgnoreCase);

            _docIdToApi = catalog.Apis.ToDictionary(key => key.DocId, key => new ApiDefinition { DocId = key.DocId, Name = key.Name, ReturnType = key.Type, FullName = key.FullName, Parent = key.Parent });
        }

        public IEnumerable<string> DocIds => _apiMapping.Keys;

        /// <summary>
        /// Gets the ApiDefinition for a docId.
        /// </summary>
        /// <returns>The corresponding ApiDefinition if it exists.
        /// If docId is null/empty or does not exist, returns null.</returns>
        public ApiDefinition GetApiDefinition(string docId)
        {
            if (!string.IsNullOrEmpty(docId) && _docIdToApi.TryGetValue(docId, out var apiDefinition))
            {
                return apiDefinition;
            }

            return null;
        }

        public bool IsFrameworkMember(string docId) => _apiMapping.ContainsKey(docId);

        public virtual bool IsMemberInTarget(string docId, FrameworkName targetName, out Version introducedVersion)
        {
            // The docId is a member in the target if:
            //  - There is an entry for the API.
            //  - The entry for the API contains the target.
            //  - The version for when the API was introduced is before (or equal) to the target version.
            if (_apiMapping.TryGetValue(docId, out var targets) && targets.TryGetValue(targetName.Identifier, out introducedVersion))
            {
                return targetName.Version >= introducedVersion;
            }

            introducedVersion = null;
            return false;
        }

        public virtual bool IsMemberInTarget(string docId, FrameworkName targetName)
        {
            return IsMemberInTarget(docId, targetName, out _);
        }

        public string GetApiMetadata(string docId, string metadataKey)
        {
            if (_apiMetadata.TryGetValue(docId, out var metadata) && metadata.TryGetValue(metadataKey, out var metadataValue))
            {
                return metadataValue;
            }

            return null;
        }

        public string GetRecommendedChange(string docId) => GetApiMetadata(docId, RecommendedChangeKey);

        public string GetSourceCompatibilityEquivalent(string docId) => GetApiMetadata(docId, SourceCompatibleEquivalent);

        public bool IsFrameworkAssembly(string assemblyIdentity) => _frameworkAssemblies.Contains(assemblyIdentity);

        public Version GetVersionIntroducedIn(string docId, FrameworkName target)
        {
            if (_apiMapping.TryGetValue(docId, out var targets) && targets.TryGetValue(target.Identifier, out var versionIntroducedIn))
            {
                return versionIntroducedIn;
            }

            return null;
        }

        public FrameworkName GetLatestVersion(string targetIdentifier)
        {
            if (_latestTargetVersion.TryGetValue(targetIdentifier, out var name))
            {
                return name;
            }

            return null;
        }

        public IEnumerable<FrameworkName> GetPublicTargets() => _publicTargets;

        public IEnumerable<TargetInfo> GetAllTargets() => _allTargets;

        /// <summary>
        /// Retrieves the ancestors for a given docId. 
        /// This retrieves the Api's parent first and then the parent's ancestor
        /// until it reaches the root.
        /// If the docId does not exist or is null, it will return an empty
        /// Enumerable.
        /// </summary>
        public IEnumerable<string> GetAncestors(string docId)
        {
            if (string.IsNullOrEmpty(docId))
            {
                yield break;
            }

            var api = GetApiDefinition(docId);

            if (api == null)
            {
                yield break;
            }

            var parent = api.Parent;

            while (!string.IsNullOrEmpty(parent))
            {
                yield return parent;

                parent = GetApiDefinition(parent).Parent;
            }
        }

        public DateTimeOffset LastModified { get; }

        public string BuiltBy { get; }

        public IEnumerable<FrameworkName> GetSupportedVersions(string docId)
        {
            return _publicTargets.Where(t => IsMemberInTarget(docId, t)).ToList();
        }
    }
}
