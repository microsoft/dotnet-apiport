// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using ApiPortVS.Contracts;
using EnvDTE;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;
using NSubstitute;
using System.Collections.Generic;
using Xunit;

namespace ApiPortVS.Tests
{
    public class ProjectBuilderTests
    {
        [Fact]
        public static void Build_VsFailsToStartBuild_TaskResultSetFalse()
        {
            var buildManager = BuildManagerWhichReturns(VSConstants.S_FALSE);
            var project = Substitute.For<Project>();
            var mapper = Substitute.For<IProjectMapper>();
            var threading = Substitute.For<IVSThreadingService>();
            var projectBuilder = new DefaultProjectBuilder(buildManager, threading, mapper);

            var result = projectBuilder.BuildAsync(new List<Project> { project }).Result;

            Assert.False(result);

            // Checking that we are not listening to build events
            // if starting a build was not successful
            buildManager.DidNotReceiveWithAnyArgs()
                .AdviseUpdateSolutionEvents(null, out var pdwCookie);
        }

        [Fact]
        public static void Build_BuildCompletedSuccessfully_TaskResultSetTrue()
        {
            var buildManager = BuildManagerWhichReturns(VSConstants.S_OK);
            var project = Substitute.For<Project>();
            var mapper = Substitute.For<IProjectMapper>();
            var threading = Substitute.For<IVSThreadingService>();

            var projectBuilder = new DefaultProjectBuilder(buildManager, threading, mapper);
            var buildTask = projectBuilder.BuildAsync(new List<Project> { project });

            // Checking that we are subscribed to build events
            buildManager.ReceivedWithAnyArgs(1)
                .AdviseUpdateSolutionEvents(Arg.Any<IVsUpdateSolutionEvents>(), out var pdwCookie);
        }

        private static IVsSolutionBuildManager2 BuildManagerWhichReturns(int returnForUpdate)
        {
            var buildManager = Substitute.For<IVsSolutionBuildManager2>();
            buildManager.StartUpdateSpecificProjectConfigurations(default, null, null, null, null, null, default, default)
                        .ReturnsForAnyArgs(returnForUpdate);

            buildManager.AdviseUpdateSolutionEvents(null, out var cookie)
                .ReturnsForAnyArgs(4);

            return buildManager;
        }
    }
}
