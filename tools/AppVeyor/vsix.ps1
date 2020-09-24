# This is subset of VSIX Module for AppVeyor by Mads Kristensen from https://github.com/madskristensen/ExtensionScripts/tree/master/AppVeyor
[cmdletbinding()]
param()

$vsixUploadEndpoint = "https://www.vsixgallery.com/api/upload"
#$vsixUploadEndpoint = "https://localhost:44372/api/upload"


function Vsix-GetRepoUrl{
    [cmdletbinding()]
    param ()
    if ($env:APPVEYOR_REPO_PROVIDER -contains "github"){
        $repoUrl = "https://github.com/" + $env:APPVEYOR_REPO_NAME + "/"
    } elseif ($env:APPVEYOR_REPO_PROVIDER -contains "bitbucket"){
        $repoUrl = "https://bitbucket.org/" + $env:APPVEYOR_REPO_NAME + "/"
    } else {
        $repoUrl = ""
    }
    return $repoUrl
}

function Vsix-PublishToGallery{
    [cmdletbinding()]
    param (
        [Parameter(Position=0, Mandatory=0,ValueFromPipeline=$true)]
        [string[]]$path = "./*.vsix"
    )
    foreach($filePath in $path){
        if ($env:APPVEYOR_PULL_REQUEST_NUMBER){
            return
        }

        $repo = ""
        $issueTracker = ""

        $repoUrl = Vsix-GetRepoUrl
        if ($baseRepoUrl -ne "") {
            [Reflection.Assembly]::LoadWithPartialName("System.Web") | Out-Null
            $repo = [System.Web.HttpUtility]::UrlEncode($repoUrl)
            $issueTracker = [System.Web.HttpUtility]::UrlEncode(($repoUrl + "issues/"))
        }

        'Publish to VSIX Gallery...' | Write-Host -ForegroundColor Cyan -NoNewline

        $fileNames = (Get-ChildItem $filePath -Recurse)

        foreach($vsixFile in $fileNames)
        {
            [string]$url = ($vsixUploadEndpoint + "?repo=" + $repo + "&issuetracker=" + $issueTracker)
            [byte[]]$bytes = [System.IO.File]::ReadAllBytes($vsixFile)
             
            try {
                $webclient = New-Object System.Net.WebClient
                $webclient.UploadFile($url, $vsixFile) | Out-Null
                'OK' | Write-Host -ForegroundColor Green
            }
            catch{
                'FAIL' | Write-Error
                $_.Exception.Response.Headers["x-error"] | Write-Error
            }
        }
    }
}

