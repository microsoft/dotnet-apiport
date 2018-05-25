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
    public class CloudApiCatalogLookup2 : IApiCatalogLookup
    {
        private const string RecommendedChangeKey = "Recommended Changes";
        private const string SourceCompatibleEquivalent = "SourceCompatibleEquivalent";

        private readonly DateTimeOffset _lastModified;
        private readonly string _builtBy;
        private readonly Dictionary<string, Dictionary<string, Version>> _apiMapping;
        private readonly Dictionary<string, Dictionary<string, string>> _apiMetadata;
        private readonly Dictionary<string, FrameworkName> _latestTargetVersion;
        private readonly ICollection<string> _frameworkAssemblies;
        private readonly IReadOnlyCollection<FrameworkName> _publicTargets;
        private readonly IReadOnlyCollection<TargetInfo> _allTargets;
        private readonly IDictionary<string, ApiDefinition> _docIdToApi;

        public CloudApiCatalogLookup2(DotNetCatalog catalog)
        {
            _lastModified = catalog.LastModified;
            _builtBy = catalog.BuiltBy;

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

        public IEnumerable<string> DocIds
        {
            get { return _apiMapping.Keys; }
        }

        /// <summary>
        /// Gets the ApiDefinition for a docId.
        /// </summary>
        /// <returns>The corresponding ApiDefinition if it exists.
        /// If docId is null/empty or does not exist, returns null.</returns>
        public ApiDefinition GetApiDefinition(string docId)
        {
            ApiDefinition apiDefinition;

            if (string.IsNullOrEmpty(docId) || !_docIdToApi.TryGetValue(docId, out apiDefinition))
            {
                return null;
            }
            else
            {
                return apiDefinition;
            }
        }

        public bool IsFrameworkMember(string docId)
        {
            return _apiMapping.ContainsKey(docId);
        }

        public virtual bool IsMemberInTarget(string docId, FrameworkName targetName, out Version introducedVersion)
        {
            Dictionary<string, Version> targets;
            introducedVersion = null;

            // The docId is a member in the target if:
            //  - There is an entry for the API.
            //  - The entry for the API contains the target.
            //  - The version for when the API was introduced is before (or equal) to the target version.
            if (!_apiMapping.TryGetValue(docId, out targets))
                return false;

            if (!targets.TryGetValue(targetName.Identifier, out introducedVersion))
                return false;

            return targetName.Version >= introducedVersion;
        }

        public virtual bool IsMemberInTarget(string docId, FrameworkName targetName)
        {
            Version version;
            return IsMemberInTarget(docId, targetName, out version);
        }

        public string GetApiMetadata(string docId, string metadataKey)
        {
            Dictionary<string, string> metadata;
            string metadataValue = null;

            if (_apiMetadata.TryGetValue(docId, out metadata))
            {
                metadata.TryGetValue(metadataKey, out metadataValue);
            }
            return metadataValue;
        }

        public string GetRecommendedChange(string docId)
        {
            return GetApiMetadata(docId, RecommendedChangeKey);
        }

        public string GetSourceCompatibilityEquivalent(string docId)
        {
            return GetApiMetadata(docId, SourceCompatibleEquivalent);
        }

        public bool IsFrameworkAssembly(string assemblyIdentity)
        {
            return _frameworkAssemblies.Contains(assemblyIdentity);
        }

        public Version GetVersionIntroducedIn(string docId, FrameworkName target)
        {
            Dictionary<string, Version> targets;
            Version versionIntroducedIn;

            if (_apiMapping.TryGetValue(docId, out targets))
            {
                if (targets.TryGetValue(target.Identifier, out versionIntroducedIn))
                {
                    return versionIntroducedIn;
                }
            }

            return null;
        }

        public FrameworkName GetLatestVersion(string targetIdentifier)
        {
            FrameworkName name;
            if (_latestTargetVersion.TryGetValue(targetIdentifier, out name))
            {
                return name;
            }
            return null;
        }

        public IEnumerable<FrameworkName> GetPublicTargets()
        {
            return _publicTargets;
        }

        public IEnumerable<TargetInfo> GetAllTargets()
        {
            return _allTargets;
        }

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

                api = GetApiDefinition(parent);
                parent = api.Parent;
            }
        }

        public DateTimeOffset LastModified { get { return _lastModified; } }

        public string BuiltBy { get { return _builtBy; } }

        public IEnumerable<FrameworkName> GetSupportedVersions(string docId)
        {
            return _publicTargets.Where(t => IsMemberInTarget(docId, t)).ToList();
        }
    }
}
