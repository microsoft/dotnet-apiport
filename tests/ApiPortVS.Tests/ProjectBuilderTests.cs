// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Threading.Tasks;
using EnvDTE;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;
using NSubstitute;
using Xunit;

namespace ApiPortVS.Tests
{
    public class ProjectBuilderTests
    {
        [Fact]
        public void Build_VsFailsToStartBuild_TaskResultSetFalse()
        {
            var buildManager = BuildManagerWhichReturns(VSConstants.S_FALSE);
            var project = Substitute.For<Project>();
            var projectBuilder = new ProjectBuilder(buildManager);
            uint pdwCookie;

            var result = projectBuilder.BuildAsync(project).Result;

            Assert.False(result);

            // Checking that we are not listening to build events 
            // if starting a build was not successful
            buildManager.DidNotReceiveWithAnyArgs()
                .AdviseUpdateSolutionEvents(null, out pdwCookie);
        }

        [Fact]
        public void Build_BuildCompletedSuccessfully_TaskResultSetTrue()
        {
            var buildManager = BuildManagerWhichReturns(VSConstants.S_OK);
            var project = Substitute.For<Project>();
            var projectBuilder = new ProjectBuilder(buildManager);
            uint pdwCookie;

            var buildTask = projectBuilder.BuildAsync(project);

            // Checking that we are subscribed to build events
            buildManager.ReceivedWithAnyArgs(1)
                .AdviseUpdateSolutionEvents(Arg.Any<IVsUpdateSolutionEvents>(), out pdwCookie);
        }

        private IVsSolutionBuildManager BuildManagerWhichReturns(int returnForUpdate)
        {
            var buildManager = Substitute.For<IVsSolutionBuildManager>();
            buildManager.StartSimpleUpdateProjectConfiguration(null, null, null, 0, 0, 0)
                        .ReturnsForAnyArgs(returnForUpdate);

            uint cookie;

            buildManager.AdviseUpdateSolutionEvents(null, out cookie)
                .ReturnsForAnyArgs(4);

            return buildManager;
        }

        private ProjectBuilder ProjectBuilderAfterBuildHasBegun(IVsSolutionBuildManager buildManager)
        {
            var project = Substitute.For<Project>();

            var projectBuilder = new ProjectBuilder(buildManager);
            projectBuilder.BuildAsync(project);

            return projectBuilder;
        }
    }
}
