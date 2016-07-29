using System;
using System.Threading.Tasks;
using EnvDTE;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;

namespace ApiPortVS.Tests
{
    [TestClass]
    public class ProjectBuilderTests
    {
        [TestMethod]
        public void Build_VsFailsToStartBuild_TaskResultSetFalse()
        {
            var serviceProvider = ProviderWithBuildManagerWhichReturns(VSConstants.S_FALSE);

            var project = Substitute.For<Project>();

            var taskCompletionSource = new TaskCompletionSource<bool>();

            var projectBuilder = new ProjectBuilder(serviceProvider);
            projectBuilder.Build(project, taskCompletionSource);

            Assert.IsFalse(taskCompletionSource.Task.Result);
        }

        [TestMethod]
        public void Build_UpdateActionFailed_TaskResultSetFalse()
        {
            var taskCompletionSource = new TaskCompletionSource<bool>();
            var projectBuilder = ProjectBuilderAfterBuildHasBegun(taskCompletionSource);

            var updateActionFailed = 0;
            projectBuilder.UpdateSolution_Done(updateActionFailed, 0, 0);

            Assert.IsFalse(taskCompletionSource.Task.Result);
        }

        [TestMethod]
        public void Build_UpdateCanceled_TaskResultSetFalse()
        {
            var taskCompletionSource = new TaskCompletionSource<bool>();
            var projectBuilder = ProjectBuilderAfterBuildHasBegun(taskCompletionSource);

            projectBuilder.UpdateSolution_Cancel();

            Assert.IsFalse(taskCompletionSource.Task.Result);
        }

        [TestMethod]
        public void Build_BuildCompletedSuccessfully_TaskResultSetTrue()
        {
            var taskCompletionSource = new TaskCompletionSource<bool>();
            var projectBuilder = ProjectBuilderAfterBuildHasBegun(taskCompletionSource);

            var updateActionSucceeded = 1;
            projectBuilder.UpdateSolution_Done(updateActionSucceeded, 0, 0);

            Assert.IsTrue(taskCompletionSource.Task.Result);
        }

        [TestMethod]
        public void Build_BuildCompletedSuccessfully_UnregistersForBuildEvents()
        {
            var buildManager = Substitute.For<IVsSolutionBuildManager>();

            var taskCompletionSource = new TaskCompletionSource<bool>();

            var projectBuilder = ProjectBuilderAfterBuildHasBegun(taskCompletionSource, buildManager);

            var updateActionSucceeded = 1;
            projectBuilder.UpdateSolution_Done(updateActionSucceeded, 0, 0);

            buildManager.Received().UnadviseUpdateSolutionEvents(Arg.Any<uint>());
        }

        [TestMethod]
        public void Build_BuildCompletedUnsuccessfully_UnregistersForBuildEvents()
        {
            var buildManager = Substitute.For<IVsSolutionBuildManager>();

            var taskCompletionSource = new TaskCompletionSource<bool>();

            var projectBuilder = ProjectBuilderAfterBuildHasBegun(taskCompletionSource, buildManager);

            var updateActionFailed = 0;
            projectBuilder.UpdateSolution_Done(updateActionFailed, 0, 0);

            buildManager.Received().UnadviseUpdateSolutionEvents(Arg.Any<uint>());
        }

        [TestMethod]
        public void Build_BuildCanceled_UnregistersForBuildEvents()
        {
            var buildManager = Substitute.For<IVsSolutionBuildManager>();

            var taskCompletionSource = new TaskCompletionSource<bool>();

            var projectBuilder = ProjectBuilderAfterBuildHasBegun(taskCompletionSource, buildManager);

            projectBuilder.UpdateSolution_Cancel();

            buildManager.Received().UnadviseUpdateSolutionEvents(Arg.Any<uint>());
        }

        private IServiceProvider ProviderWithBuildManagerWhichReturns(int returnForUpdate)
        {
            var buildManager = Substitute.For<IVsSolutionBuildManager>();
            buildManager.StartSimpleUpdateProjectConfiguration(null, null, null, 0, 0, 0)
                        .ReturnsForAnyArgs(returnForUpdate);

            return ProviderReturningBuildManager(buildManager);
        }

        private IServiceProvider ProviderReturningBuildManager(IVsSolutionBuildManager buildManager)
        {
            var serviceProvider = Substitute.For<IServiceProvider>();
            serviceProvider.GetService(typeof(SVsSolutionBuildManager)).Returns(buildManager);

            return serviceProvider;
        }

        private ProjectBuilder ProjectBuilderAfterBuildHasBegun(TaskCompletionSource<bool> taskCompletionSource, IVsSolutionBuildManager buildManager = null)
        {
            var serviceProvider = buildManager == null ? ProviderWithBuildManagerWhichReturns(VSConstants.S_OK)
                                                       : ProviderReturningBuildManager(buildManager);

            var project = Substitute.For<Project>();

            var projectBuilder = new ProjectBuilder(serviceProvider);
            projectBuilder.Build(project, taskCompletionSource);

            return projectBuilder;
        }
    }
}
