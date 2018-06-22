// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.WindowsAzure.Storage;
using System;

namespace PortabilityService.AnalysisService
{
    /// <summary>
    /// Portability service settings
    /// </summary>
    public interface IServiceSettings
    {
        /// <summary>
        /// How often to check for updates for data stored in Azure Storage.
        /// Used to check for updates for catalog.bin and fxmember.json.gz
        /// </summary>
        TimeSpan UpdateFrequency { get; }

        /// <summary>
        /// The name of the default ResultFormatInformation
        /// </summary>
        string DefaultResultFormat { get; }

        /// <summary>
        /// Semi-colon delimited list of default .NET Platform Targets to
        /// perform portability analysis against.
        /// </summary>
        string DefaultTargets { get; }

        /// <summary>
        /// Semi-colon delimited list of default .NET Platform Targets and
        /// their version to perform portability analysis against.
        /// </summary>
        /// <example>.NETStandard,Version=1.6;.NET Framework,Version=4.5</example>
        string DefaultVersions { get; }

        /// <summary>
        /// Storage account for catalog.bin
        /// </summary>
        CloudStorageAccount StorageAccount { get; }

        /// <summary>
        /// TODO: Figure out whether this is in use still or whether to remove it.
        /// </summary>
        string TargetGroups { get; }

        /// <summary>
        /// Absolute path to a directory containing breaking change docs as .md files
        /// (i.e. the contents of https://github.com/Microsoft/dotnet/tree/master/Documentation/compatibility)
        /// </summary>
        string BreakingChangesPath { get; }

        /// <summary>
        /// Absolute path to a directory tree containing recommended changes as .md files
        /// (i.e. the contents of https://github.com/Microsoft/dotnet-apiport/tree/master/docs/RecommendedChanges)
        /// </summary>
        string RecommendedChangesPath { get; }

        /// <summary>
        /// True to unify APIs from ASP.NET Core and .NET Core into the same
        /// platform target; false otherwise.
        /// </summary>
        bool UnionAspNetWithNetCore { get; }
    }
}
