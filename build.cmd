@ECHO OFF

:: Default to a release build
IF "%Configuration%"=="" SET Configuration=Release

powershell -noprofile -executionpolicy bypass -file .\build.ps1 %Configuration% "AnyCPU" -RunTests