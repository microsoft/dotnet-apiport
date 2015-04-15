using Microsoft.Fx.Portability.ObjectModel;
using Microsoft.Fx.Portability.Reporting.ObjectModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Versioning;
using Xunit;

namespace Microsoft.Fx.Portability.Tests
{
    public class ReportingResultTests
    {
        private MemberInfo MissingMember(string typeDocId, string memberName)
        {
            return new MemberInfo
            {
                TypeDocId = typeDocId,
                MemberDocId = string.Format("M:{0}.{1}", typeDocId.Substring(2), memberName),
                IsSupportedAcrossTargets = false
            };
        }

        private List<FrameworkName> TargetPlatforms(int count)
        {
            return Enumerable.Range(1, count)
                .Select(n => new FrameworkName(string.Format("Target{0}", n), new Version(1, 0)))
                .ToList();
        }

        [Fact]
        public void AddMissingDependency_MissingMemberOfSupportedType_TypeIsNotMarkedMissing()
        {
            var targets = TargetPlatforms(2);
            var type = new MemberInfo
            {
                MemberDocId = "T:Spam.Spam",
                IsSupportedAcrossTargets = true,
                TargetStatus = targets.Select(t => new Version(t.Version.ToString())).ToList()
            };

            var missingMember = MissingMember(type.MemberDocId, "Eggs");
            missingMember.TargetStatus = new List<Version>
            {
                new Version("1.0"),
                null
            };

            var reportingResult = new ReportingResult(targets, new List<MemberInfo>() { type }, Guid.NewGuid().ToString(), AnalyzeRequestFlags.None);
            reportingResult.AddMissingDependency(null, missingMember, "Add more spam.");

            var typeIsMarkedMissing = reportingResult.GetMissingTypes()
                .First(t => string.Equals(t.TypeName, type.MemberDocId, StringComparison.Ordinal))
                .IsMissing;
            Assert.False(typeIsMarkedMissing);
        }

        [Fact]
        public void AddMissingDependency_MemberOfUnidentifiedType_TypeAddedToMissingTypes()
        {
            var targets = TargetPlatforms(2);
            var typeDocId = "T:Spam.Spam";
            var missingMember = MissingMember(typeDocId, "Eggs");
            missingMember.TargetStatus = new List<Version>
            {
                new Version("1.0"),
                null
            };

            var reportingResult = new ReportingResult(targets, new List<MemberInfo>(), Guid.NewGuid().ToString(), AnalyzeRequestFlags.None);
            reportingResult.AddMissingDependency(null, missingMember, "Add more spam.");

            var typeWasAdded = reportingResult.GetMissingTypes().Any(t => string.Equals(t.DocId, typeDocId, StringComparison.Ordinal));
            Assert.True(typeWasAdded);
        }

        [Fact]
        public void AddMissingDependency_MemberOfUnidentifiedType_TypeInheritsMemberTargetStatus()
        {
            var targets = TargetPlatforms(2);
            var typeDocId = "T:Spam.Spam";
            var missingMember = MissingMember(typeDocId, "Eggs");
            missingMember.TargetStatus = new List<Version>
            {
                new Version("1.0"),
                null
            };

            var reportingResult = new ReportingResult(targets, new List<MemberInfo>(), Guid.NewGuid().ToString(), AnalyzeRequestFlags.None);
            reportingResult.AddMissingDependency(null, missingMember, "Add more spam.");

            var type = reportingResult.GetMissingTypes()
                .First(t => string.Equals(t.TypeName, typeDocId, StringComparison.Ordinal));

            Assert.True(missingMember.TargetStatus.Count == type.TargetVersionStatus.Count()
                     && missingMember.TargetStatus.All(v => type.TargetVersionStatus.Contains(v)));
        }
    }
}
