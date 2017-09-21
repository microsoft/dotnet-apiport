// Import the utility functionality.
import jobs.generation.Utilities;

def project = GithubProject
def branch = GithubBranchName
def configurationGroups = ['Debug', 'Release']
def outerloopPlatforms = ['Windows_NT', 'Ubuntu14.04', 'Ubuntu16.04', 'OSX10.12']

// Generate the builds for debug and release, commit and PRJob
[true, false].each { isPR -> // Defines a closure over true and false, value assigned to isPR
    configurationGroups.each { configuration ->
        outerloopPlatforms.each { os ->
            def name = "${os.toLowerCase()}_${configuration.toLowerCase()}"
            def newJobName = Utilities.getFullJobName(project, name, isPR)
            def newJob = job(newJobName)

            if (os == 'Windows_NT') {
                newJob.with {
                    steps {
                        powerShell(".\\build.ps1 -Configuration ${configuration} -Platform AnyCPU -RunTests")
                    }
                }

                Utilities.setMachineAffinity(newJob, os, 'latest-dev15-3')
            } else {
                newJob.with {
                    steps {
                        shell("./build.sh --configuration ${configuration}")
                    }
                }

                Utilities.setMachineAffinity(newJob, os, 'latest-or-auto')
            }

            // This call performs remaining common job setup on the newly created job.
            // It does the following:
            //   1. Sets up source control for the project.
            //   2. Adds standard options for build retention and timeouts
            //   3. Adds standard parameters for PR and push jobs.
            Utilities.standardJobSetup(newJob, project, isPR, "*/${branch}")
            Utilities.addMSTestResults(newJob, 'TestResults/*.trx')

            // The following two calls add triggers for push and PR jobs
            // In Github, the PR trigger will appear as "Windows Debug" and "Windows Release" and will be run
            // by default
            if (isPR) {
                Utilities.addGithubPRTriggerForBranch(newJob, branch, "${os} ${configuration}")
            }
            else {
                Utilities.addGithubPushTrigger(newJob)
            }
        }
    }
}
