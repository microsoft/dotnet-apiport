// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

//TODO (yumeng): uncommend next line
//using Microsoft.Fx.Portability.Github;
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
        /// URL to http://dotnetstatus.azurewebsites.net site
        /// 
        /// TODO: Remove this because site is replaced with apisof.net
        /// </summary>
        string DotNetStatusEndpoint { get; }

        /// <summary>
        /// Storage account for catalog.bin
        /// </summary>
        CloudStorageAccount StorageAccount { get; }

        /// <summary>
        /// TODO: Figure out whether this is in use still or whether to remove it.
        /// </summary>
        string TargetGroups { get; }

        /* TODO (yumeng): add back the following after portability is done
        /// <summary>
        /// Git repository used to fetch <see cref="Microsoft.Fx.Portability.ObjectModel.ApiPortData"/>
        /// </summary>
        IGitSettings ApiPortGitData { get; }

        /// <summary>
        /// Git repository to fetch <see cref="BreakingChange"/>
        /// </summary>
        IGitSettings BreakingChangeData { get; }

        /// <summary>
        /// Settings used to download Compatibility Diagnostic information
        /// </summary>
        IDiagnosticDownloaderSettings DiagnosticDownloader { get; }

        /// <summary>
        /// OAuth information used to communicate with GitHub.
        /// </summary>
        /// <remarks>Used for the WebHooks for dotnet-apiport PR validation</remarks>
        IGitHubSettings GitHub { get; }
        end TODO */

        /// <summary>
        /// True to unify APIs from ASP.NET Core and .NET Core into the same
        /// platform target; false otherwise.
        /// </summary>
        bool UnionAspNetWithNetCore { get; }
    }
}
