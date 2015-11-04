@ECHO OFF

:: Libraries are currently pre-release
SET VersionSuffix=alpha

:: Default to a release build
IF "%Configuration%"=="" SET Configuration=Release

if not exist bin\%Configuration% mkdir bin\%Configuration%

powershell -noprofile -executionpolicy bypass -file build\restore.ps1
"%ProgramFiles(x86)%\MSBuild\14.0\bin\MSBuild.exe" /nologo /m /v:m /nr:false /flp:logfile=bin\%Configuration%\msbuild.log;verbosity=normal %*
powershell -noprofile -executionpolicy bypass -file build\postbuild.ps1 -configuration %Configuration%