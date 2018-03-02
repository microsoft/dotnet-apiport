# Microsoft.Fx.Portability.Offline Usage
Microsoft.Fx.Portability.Offline enables ApiPort to work locally (without 
sending any data to a remote service) via dependency injection.

In order to use ApiPort in offline mode, copy the binary output of this 
project next to ApiPort.exe. Note that because ApiPort typically expects the 
server-side component to generate reports, any report generators used must 
also be deployed next to ApiPort.exe. The html and json report generators can 
be found in their own directories next to this project.

The unity.config file contained in this project controls which local 
components are used by ApiPort.exe. By default, it includes the local 
Microsoft.Fx.Portability.Offline analysis library, as well as local html and 
json report generators. If either report generator is not deployed next to 
ApiPort.exe, the associated registration should be removed from Unity.config.

### Common Setup Steps
For most local uses of ApiPort.exe, the necessary setup steps will be:

1. Build the PortabilityTools solution
2. Copy the binary output of Microsoft.Fx.Portability.Offline next to 
   ApiPort.exe
3. Copy the binary output of Microsoft.Fx.Portability.Reports.Html next to 
   ApiPort.exe
4. Copy the binary output of Microsoft.Fx.Portability.Reports.Json next to 
   ApiPort.exe