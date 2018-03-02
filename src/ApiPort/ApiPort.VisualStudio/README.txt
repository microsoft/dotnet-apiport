DEBUGGING
=========
Debugging in the experimental instance of Visual Studio requires additional configuration.

On the project Properties->Debug page:
    - In the Start Action section select "Start external program" and set the target to 
      devenv.exe for the version of VS you're running.
    - In the Start Options section add command line arguments:  /rootsuffix Exp

On the project Properties->VSIX page:
    - Check "Create VSIX container during build" and "Deploy VSIX content to the
      experimental instance for debugging".

NEW RELEASES
============
Update the version number:
    - In ApiPortVSPackage.cs -> InstalledProductRegistration attribute
    - In AssemblyInfo.cs (AssemblyFileVersion and AssemblyVersion)
    - In source.extension.vsixmanifest
    - In LocalizedStrings.resx -> ApplicationVersionUserAgent