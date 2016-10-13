// Import the utility functionality.
import jobs.generation.Utilities;

def project = GithubProject
def branch = GithubBranchName

// Generate the builds for debug and release, commit and PRJob
[true, false].each { isPR -> // Defines a closure over true and false, value assigned to isPR
    ['Debug', 'Release'].each { configuration ->
        ['AnyCPU', 'x64', 'x86'].each { platform ->

            def newJobName = Utilities.getFullJobName(project, configuration, isPR)
            
            // Define build string
            def buildString = ".\\build.ps1 ${configuration} ${platform} -RunTests"

            // Create a new job with the specified name.  The brace opens a new closure
            // and calls made within that closure apply to the newly created job.
            def newJob = job(newJobName) {
                steps {
                    powerShell(buildString)
                }
            }
            
            Utilities.setMachineAffinity(newJob, 'Windows_NT', 'latest-or-auto')
            
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
                Utilities.addGithubPRTriggerForBranch(newJob, branch, "Windows ${configuration}")
            }
            else {
                Utilities.addGithubPushTrigger(newJob)
            }
        }
    }
}