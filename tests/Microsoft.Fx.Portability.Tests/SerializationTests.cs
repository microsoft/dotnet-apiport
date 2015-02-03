using Microsoft.Fx.Portability.ObjectModel;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Json;
using System.Runtime.Versioning;

namespace Microsoft.Fx.Portability.Tests
{
    [TestClass]
    public class SerializationTests
    {
        [TestMethod]
        public void SerializeAnalyzeRequest()
        {
            var request = new AnalyzeRequest
            {
                ApplicationName = "name",
                Dependencies = GetDependencies(),
                Targets = new List<string> { "target1", "target2" },
                UnresolvedAssemblies = new List<string> { "assembly1", "assembly2" },
                UserAssemblies = new List<AssemblyInfo> { new AssemblyInfo { AssemblyIdentity = "name1" }, new AssemblyInfo { AssemblyIdentity = "name2" } },
                Version = AnalyzeRequest.CurrentVersion
            };

            var newtonsoft = request.Serialize().Deserialize<AnalyzeRequest>();
            var dcjs = DeserializeObjectDcjs<AnalyzeRequest>(SerializeDcjs(request));

            CompareAnalyzeRequest(request, newtonsoft);
            CompareAnalyzeRequest(request, dcjs);
        }

        [TestMethod]
        public void SerializeAnalyzeResponse()
        {
            var response = new AnalyzeResponse
            {
                MissingDependencies = new List<MemberInfo> { new MemberInfo { MemberDocId = "doc1" }, new MemberInfo { MemberDocId = "doc2" } },
                SubmissionId = Guid.NewGuid().ToString(),
                Targets = new List<FrameworkName> { new FrameworkName("target1", Version.Parse("1.0.0.0")) },
                UnresolvedUserAssemblies = new List<string> { "assembly1", "assembly2", "assembly3" }
            };

            var newtonsoft = response.Serialize().Deserialize<AnalyzeResponse>();
            var dcjs = DeserializeObjectDcjs<AnalyzeResponse>(SerializeDcjs(response));

            CompareAnalyzeResponse(response, newtonsoft);
            CompareAnalyzeResponse(response, dcjs);
        }


        [TestMethod]
        public void SerializeAnalyzeV1Response()
        {
            var response = new AnalyzeResponse
            {
                MissingDependencies = new List<MemberInfo> { new MemberInfo { MemberDocId = "doc1" }, new MemberInfo { MemberDocId = "doc2" } },
                SubmissionId = Guid.NewGuid().ToString(),
                Targets = new List<FrameworkName> { new FrameworkName("target1", Version.Parse("1.0.0.0")) },
                UnresolvedUserAssemblies = new List<string> { "assembly1", "assembly2", "assembly3" },
            };

            var v1 = new AnalyzeResponseV1(response);

            var newtonsoft = v1.Serialize().Deserialize<AnalyzeResponseV1>();
            var dcjs = DeserializeObjectDcjs<AnalyzeResponseV1>(SerializeDcjs(v1));

            CompareAnalyzeResponseV1(v1, newtonsoft);
            CompareAnalyzeResponseV1(v1, dcjs);

        }

        [TestMethod]
        public void SerializeProjectSubmission()
        {
            var submission1 = new ProjectSubmission { Name = "test1", Length = 10, SubmittedDate = new DateTime(10, 1, 4) };
            var submission2 = new ProjectSubmission { Name = "test2", Length = 11, SubmittedDate = new DateTime(10, 2, 4) };
            var submission3 = new ProjectSubmission { Name = "test3", Length = 12, SubmittedDate = new DateTime(10, 3, 6) };

            var list = new List<ProjectSubmission> { submission1, submission2, submission3 };

            var serialized = list.Serialize();
            var deserialized = serialized.Deserialize<IEnumerable<ProjectSubmission>>();

            CollectionAssertAreEquivalent(list, deserialized);
        }

        [TestMethod]
        public void TestFrameworkNames()
        {
            VerifySerialization(new FrameworkName("name", new Version("1.2.3.4")));
            VerifySerialization(new FrameworkName("name", new Version("1.2.3.0")));
            VerifySerialization(new FrameworkName("name", new Version("1.2.0.0")));
            VerifySerialization(new FrameworkName("name", new Version("1.2.0.4")));
            VerifySerialization(new FrameworkName("name", new Version("1.2.1")));
            VerifySerialization(new FrameworkName("name", new Version("1.0")));
        }

        [TestMethod]
        public void TestVersion()
        {
            VerifySerialization(new Version("1.2.3.4"));
            VerifySerialization(new Version("1.2.3.0"));
            VerifySerialization(new Version("1.2.0.0"));
            VerifySerialization(new Version("1.2.0.4"));
            VerifySerialization(new Version("1.2.1"));
            VerifySerialization(new Version("1.0"));
        }

        [TestMethod]
        public void TestEmptyValues()
        {
            VerifyEmptySerialized<AnalyzeRequest>();
            VerifyEmptySerialized<FrameworkName>();
            VerifyEmptySerialized<Version>();
        }

        private static IDictionary<MemberInfo, ICollection<AssemblyInfo>> GetDependencies()
        {
            var dict = new Dictionary<MemberInfo, ICollection<AssemblyInfo>>();

            dict.Add(new MemberInfo { MemberDocId = "item1" }, new HashSet<AssemblyInfo> { new AssemblyInfo { AssemblyIdentity = "string1" }, new AssemblyInfo { AssemblyIdentity = "string2" } });
            dict.Add(new MemberInfo { MemberDocId = "item2" }, new HashSet<AssemblyInfo> { new AssemblyInfo { AssemblyIdentity = "string3" }, new AssemblyInfo { AssemblyIdentity = "string4" } });
            dict.Add(new MemberInfo { MemberDocId = "item3" }, new HashSet<AssemblyInfo> { new AssemblyInfo { AssemblyIdentity = "string5" }, new AssemblyInfo { AssemblyIdentity = "string6" } });

            return dict;
        }

        private static void CollectionAssertAreEquivalent<T>(IEnumerable<T> expected, IEnumerable<T> actual)
        {
            CollectionAssert.AreEquivalent(expected.ToList(), actual.ToList());
        }

        private static byte[] SerializeDcjs<T>(T obj)
        {
            using (var ms = new MemoryStream())
            {
                var dcjs = new DataContractJsonSerializer(typeof(T));
                dcjs.WriteObject(ms, obj);
                return ms.ToArray();
            }
        }

        private static T DeserializeObjectDcjs<T>(byte[] data)
        {
            using (var ms = new MemoryStream(data))
            {
                var dcjs = new DataContractJsonSerializer(typeof(T));
                return (T)dcjs.ReadObject(ms);
            }
        }

        private static void VerifyEmptySerialized<T>()
        {
            var deserialized = "".Serialize().Deserialize<T>();

            Assert.IsNull(deserialized, "Failed to deserialize empty: {0}", typeof(T).FullName);
        }

        private static void VerifySerialization<T>(T o)
        {
            var deserialized = o.Serialize().Deserialize<T>();

            Assert.AreEqual(o, deserialized);
        }

        private static void CompareAnalyzeRequest(AnalyzeRequest request, AnalyzeRequest deserialized)
        {
            Assert.AreEqual(request.ApplicationName, deserialized.ApplicationName);

            // Verify dependencies
            CollectionAssertAreEquivalent(request.Dependencies.Keys, deserialized.Dependencies.Keys);

            foreach (var item in request.Dependencies.Keys)
            {
                CollectionAssertAreEquivalent(request.Dependencies[item], deserialized.Dependencies[item]);
            }

            CollectionAssert.AreEquivalent(request.Targets.ToList(), deserialized.Targets.ToList());
            CollectionAssert.AreEquivalent(request.UnresolvedAssemblies.ToList(), deserialized.UnresolvedAssemblies.ToList());
            CollectionAssert.AreEquivalent(request.UserAssemblies.ToList(), deserialized.UserAssemblies.ToList());
            Assert.AreEqual(request.Version, deserialized.Version);
        }

        private static void CompareAnalyzeResponse(AnalyzeResponse response, AnalyzeResponse deserialized)
        {
            CollectionAssertAreEquivalent(response.MissingDependencies, deserialized.MissingDependencies);
            Assert.AreEqual(response.SubmissionId, deserialized.SubmissionId);
            CollectionAssertAreEquivalent(response.Targets, deserialized.Targets);
            CollectionAssertAreEquivalent(response.UnresolvedUserAssemblies, deserialized.UnresolvedUserAssemblies);
        }

        private static void CompareAnalyzeResponseV1(AnalyzeResponseV1 response, AnalyzeResponseV1 deserialized)
        {
            CollectionAssertAreEquivalent(response.MissingDependencies, deserialized.MissingDependencies);
            Assert.AreEqual(response.SubmissionId, deserialized.SubmissionId);
            CollectionAssertAreEquivalent(response.Targets, deserialized.Targets);
            CollectionAssertAreEquivalent(response.UnresolvedUserAssemblies, deserialized.UnresolvedUserAssemblies);
        }
    }
}
