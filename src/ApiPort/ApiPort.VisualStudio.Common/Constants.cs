// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiPortVS.Common
{
    public static class Constants
    {
        /// <summary>
        /// A set of well-known output group names.
        /// </summary>
        public static class OutputGroups
        {
            /// <summary>
            /// MSBuild .targets file string for BuiltProjectOutputGroup.
            /// </summary>
            public const string BuiltProject = "Built";
        }

        /// <summary>
        /// A set of well-known output group item metadata names.
        /// </summary>
        public static class MetadataNames
        {
            /// <summary>
            /// MSBuild .targets file string for FinalOutputPath.
            /// </summary>
            public const string FinalOutputPath = nameof(FinalOutputPath);

            /// <summary>
            /// Metadata name for file output location
            /// </summary>
            public const string OutputLocation = "OUTPUTLOC";
        }
    }
}
