// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Threading.Tasks;
using EnvDTE;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;
using NSubstitute;
using Xunit;
using System.Collections.Generic;
using ApiPortVS.Contracts;

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
            uint pdwCookie;

            var result = projectBuilder.BuildAsync(new List<Project> { project }).Result;

            Assert.False(result);

            // Checking that we are not listening to build events 
            // if starting a build was not successful
            buildManager.DidNotReceiveWithAnyArgs()
                .AdviseUpdateSolutionEvents(null, out pdwCookie);
        }

        [Fact]
        public static void Build_BuildCompletedSuccessfully_TaskResultSetTrue()
        {
            var buildManager = BuildManagerWhichReturns(VSConstants.S_OK);
            var project = Substitute.For<Project>();
            var mapper = Substitute.For<IProjectMapper>();
            var threading = Substitute.For<IVSThreadingService>();

            var projectBuilder = new DefaultProjectBuilder(buildManager, threading, mapper);
            uint pdwCookie;

            var buildTask = projectBuilder.BuildAsync(new List<Project> { project });

            // Checking that we are subscribed to build events
            buildManager.ReceivedWithAnyArgs(1)
                .AdviseUpdateSolutionEvents(Arg.Any<IVsUpdateSolutionEvents>(), out pdwCookie);
        }

        private static IVsSolutionBuildManager2 BuildManagerWhichReturns(int returnForUpdate)
        {
            var buildManager = Substitute.For<IVsSolutionBuildManager2>();
            buildManager.StartUpdateSpecificProjectConfigurations(default(uint), null, null, null, null, null, default(uint), default(int))
                        .ReturnsForAnyArgs(returnForUpdate);

            uint cookie;

            buildManager.AdviseUpdateSolutionEvents(null, out cookie)
                .ReturnsForAnyArgs(4);

            return buildManager;
        }

        private static DefaultProjectBuilder ProjectBuilderAfterBuildHasBegun(IVsSolutionBuildManager2 buildManager)
        {
            var project = Substitute.For<Project>();
            var projectMapper = Substitute.For<IProjectMapper>();
            var threading = Substitute.For<IVSThreadingService>();

            var projectBuilder = new DefaultProjectBuilder(buildManager, threading, projectMapper);
            projectBuilder.BuildAsync(new List<Project> { project });

            return projectBuilder;
        }
    }
}
