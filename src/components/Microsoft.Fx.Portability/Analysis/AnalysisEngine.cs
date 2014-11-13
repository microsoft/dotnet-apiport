using Microsoft.Fx.Portability.ObjectModel;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Versioning;

namespace Microsoft.Fx.Portability.Analysis
{
    public class AnalysisEngine : IAnalysisEngine
    {
        private readonly IApiCatalogLookup _catalog;
        private readonly IApiRecommendations _recommendations;

        public AnalysisEngine(IApiCatalogLookup catalog, IApiRecommendations recommendations)
        {
            _catalog = catalog;
            _recommendations = recommendations;
        }

        public IList<MemberInfo> FindMembersNotInTargets(IEnumerable<FrameworkName> targets, IDictionary<MemberInfo, ICollection<AssemblyInfo>> dependencies)
        {
            Trace.TraceInformation("Computing members not in target");
            Stopwatch sw = new Stopwatch();
            sw.Start();

            if (dependencies == null || dependencies.Keys == null || targets == null)
            {
                return new List<MemberInfo>();
            }

            // Find the missing members by:
            //  -- Find the members that are defined in framework assemblies AND which are framework members.
            //  -- For each member, identity which is the status for that docId for each of the targets.
            //  -- Keep only the members that are not supported on at least one of the targets.

            var missingMembers = dependencies.Keys
                .Where(dep => _catalog.IsFrameworkAssembly(GetAssemblyIdentityWithoutCultureAndVersion(dep.DefinedInAssemblyIdentity)) && _catalog.IsFrameworkMember(dep.MemberDocId))
                .AsParallel()
                .Select(memberInfo => ProcessMemberInfo(_catalog, targets, memberInfo))
                .Where(memberInfo => !memberInfo.IsSupportedAcrossTargets)
                .ToList();

            sw.Stop();
            Trace.TraceInformation("Computing members not in target took '{0}'", sw.Elapsed);

            return missingMembers;
        }

        /// <summary>
        /// Identitifies the status of an API for all of the targets
        /// </summary>
        private MemberInfo ProcessMemberInfo(IApiCatalogLookup catalog, IEnumerable<FrameworkName> targets, MemberInfo member)
        {
            List<Version> targetStatus;

            member.IsSupportedAcrossTargets = IsSupportedAcrossTargets(catalog, member.MemberDocId, targets, out targetStatus);
            member.TargetStatus = targetStatus;
            member.RecommendedChanges = _recommendations.GetRecommendedChanges(member.MemberDocId);
            member.SourceCompatibleChange = _recommendations.GetSourceCompatibleChanges(member.MemberDocId);

            return member;
        }

        /// <summary>
        /// Computes a list of strings that describe the status of the api on all the targets (not supported or version when it was introduced)
        /// </summary>
        private bool IsSupportedAcrossTargets(IApiCatalogLookup catalog, string memberDocId, IEnumerable<FrameworkName> targets, out List<Version> targetStatus)
        {
            targetStatus = new List<Version>();
            bool isSupported = true;
            foreach (var target in targets)
            {
                // For each target we should get the status of the api:
                //   - 'null' if not supported
                //   - Version introduced in
                Version status;

                if (!IsSupportedOnTarget(catalog, memberDocId, target, out status))
                {
                    isSupported = false;
                }

                targetStatus.Add(status);
            }

            return isSupported;
        }

        public IEnumerable<string> FindUnreferencedAssemblies(IEnumerable<string> unreferencedAssemblies, IEnumerable<AssemblyInfo> specifiedUserAssemblies)
        {
            if (unreferencedAssemblies == null)
                yield break;

            // Find the unreferenced assemblies that are not framework assemblies.
            var userUnreferencedAssemblies = unreferencedAssemblies.AsParallel().
                Where(asm => asm != null && !_catalog.IsFrameworkAssembly(GetAssemblyIdentityWithoutCultureAndVersion(asm)))
                .ToList();

            // For each of the user unreferenced assemblies figure out if it was actually specified as an input
            foreach (var userAsm in userUnreferencedAssemblies)
            {
                // if somehow a null made it through...
                if (userAsm == null)
                    continue;

                // If the unresolved assembly was not actually specified, we need to tell the user that.
                if (specifiedUserAssemblies != null && specifiedUserAssemblies.Any(ua => ua != null && StringComparer.OrdinalIgnoreCase.Equals(ua.AssemblyIdentity, userAsm)))
                    continue;

                yield return userAsm;
            }
        }

        private static string GetAssemblyIdentityWithoutCultureAndVersion(string assemblyIdentity)
        {
            return new System.Reflection.AssemblyName(assemblyIdentity).ToString();
        }

        private static bool IsSupportedOnTarget(IApiCatalogLookup catalog, string memberDocId, FrameworkName target, out Version status)
        {
            status = null;

            // If the member is part of the API surface, no need to report it.
            if (catalog.IsMemberInTarget(memberDocId, target, out status))
            {
                return true;
            }

            var sourceEquivalent = catalog.GetSourceCompatibilityEquivalent(memberDocId);

            if (!string.IsNullOrEmpty(sourceEquivalent) && catalog.IsMemberInTarget(sourceEquivalent, target, out status))
            {
                return true;
            }

            return false;
        }
    }
}
