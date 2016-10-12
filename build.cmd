@ECHO OFF

:: Libraries are currently pre-release
SET VersionSuffix=alpha

:: Default to a release build
IF "%Configuration%"=="" SET Configuration=Release

powershell -noprofile -executionpolicy bypass -file build.ps1 %Configuration% "AnyCPU" -RunTests -CreateNugetPackages -VersionSuffix %VersionSuffix%
powershell -noprofile -executionpolicy bypass -file build.ps1 %Configuration% "x64" -VersionSuffix %VersionSuffix%
powershell -noprofile -executionpolicy bypass -file build.ps1 %Configuration% "x86" -VersionSuffix %VersionSuffix%