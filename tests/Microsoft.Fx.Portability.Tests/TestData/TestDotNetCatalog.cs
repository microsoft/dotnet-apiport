// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Fx.Portability.ObjectModel;
using System;
using System.Runtime.Versioning;

namespace Microsoft.Fx.Portability.Tests.TestData
{
    public class TestDotNetCatalog : DotNetCatalog
    {
        private static readonly FrameworkName s_windows80 = new FrameworkName("Windows,Version=v8.0");
        private static readonly FrameworkName s_windows81 = new FrameworkName("Windows,Version=v8.1");
        private static readonly FrameworkName s_netCore50 = new FrameworkName(".NET Core,Version=v5.0");
        private static readonly FrameworkName s_net11 = new FrameworkName(".NET Framework,Version=v1.1");
        private static readonly FrameworkName s_net40 = new FrameworkName(".NET Framework,Version=v4.0");

        private static readonly string[] s_frameworkIdentities = new[] {
            "System.Collections, PublicKeyToken=b03f5f7f11d50a3a",
            "System.Collections.Concurrent, PublicKeyToken=b03f5f7f11d50a3a",
            "System.Collections.NonGeneric, PublicKeyToken=b03f5f7f11d50a3a"
        };

        private static readonly TargetInfo[] s_testSupportedTargets = new[] {
            new TargetInfo { DisplayName = s_windows80, IsReleased = true },
            new TargetInfo { DisplayName = s_windows81, IsReleased = true },
            new TargetInfo { DisplayName = s_netCore50, IsReleased = true },
            new TargetInfo { DisplayName = s_net11, IsReleased = true },
            new TargetInfo { DisplayName = s_net40, IsReleased = true },
        };

        public TestDotNetCatalog()
        {
            LastModified = new DateTimeOffset(2015, 12, 2, 11, 23, 01, TimeSpan.FromHours(7));
            BuiltBy = "Test Machine";
            Apis = GetApis();
            FrameworkAssemblyIdenties = s_frameworkIdentities;
            SupportedTargets = s_testSupportedTargets;
        }

        private ApiInfoStorage[] GetApis()
        {
            var targets11 = new[] { s_windows80, s_netCore50, s_net11 };
            var targets40 = new[] { s_windows80, s_netCore50, s_net40 };

            var apis = new[] {
                new ApiInfoStorage {
                    DocId = "N:System.Collections",
                    FullName = "System.Collections",
                    Name = "System.Collections",
                    Type = "",
                    Parent = null,
                    Targets = targets11
                },
                new ApiInfoStorage {
                    DocId = "N:System.Collections.Concurrent",
                    FullName = "System.Collections.Concurrent",
                    Name = "System.Collections.Concurrent",
                    Type = "",
                    Parent = null,
                    Targets = targets40
                },
                new ApiInfoStorage {
                    DocId = "T:System.Collections.Concurrent.ConcurrentBag`1",
                    FullName = "System.Collections.Concurrent.ConcurrentBag<T>",
                    Name = "ConcurrentBag<T>",
                    Type = "",
                    Parent = "N:System.Collections.Concurrent",
                    Targets = targets40
                },
                new ApiInfoStorage {
                    DocId = "P:System.Collections.Concurrent.ConcurrentBag`1.Count",
                    FullName = "System.Collections.Concurrent.ConcurrentBag<T>.Count",
                    Name = "Count",
                    Type = "Int32",
                    Parent = "T:System.Collections.Concurrent.ConcurrentBag`1",
                    Targets = targets40
                },
                new ApiInfoStorage {
                    DocId = "M:System.Collections.Concurrent.ConcurrentBag`1.CopyTo(`0[],System.Int32)",
                    FullName = "System.Collections.Concurrent.ConcurrentBag<T>.CopyTo(T[], Int32)",
                    Name = "CopyTo(T[], Int32)",
                    Type = "Void",
                    Parent = "T:System.Collections.Concurrent.ConcurrentBag`1",
                    Targets = targets40
                },
                new ApiInfoStorage {
                    DocId = "M:System.Collections.Concurrent.ConcurrentBag`1.get_Count",
                    FullName = "System.Collections.Concurrent.ConcurrentBag<T>.Count.get_Count()",
                    Name = "get_Count()",
                    Type = "Int32",
                    Parent = "P:System.Collections.Concurrent.ConcurrentBag`1.Count",
                    Targets = targets40
                }
            };

            return apis;
        }
    }
}
