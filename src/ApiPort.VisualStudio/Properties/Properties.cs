using Microsoft.VisualStudio.Shell;

[assembly: ProvideBindingRedirection(AssemblyName = "Microsoft.Win32.Primitives")]

/* Need to provide a code base attribute for each assembly. To regenerate this, run the following powershell from ApiPort.Vsix output:

 gci *.dll `
   | where Name -notmatch Microsoft.VisualStudio `
   | where Name -notmatch stdole `
   | where Name -notmatch vslang `
   | where Name -notmatch EnvDTE `
   | where Name -notmatch Microsoft.Build `
   | % { Write - Host "[assembly: ProvideCodeBase(CodeBase = @`"`$PackageFolder`$\$($_.Name)`")]" }

 */
[assembly: ProvideCodeBase(CodeBase = @"$PackageFolder$\ApiPort.VisualStudio.2015.dll")]
[assembly: ProvideCodeBase(CodeBase = @"$PackageFolder$\ApiPort.VisualStudio.2017.dll")]
[assembly: ProvideCodeBase(CodeBase = @"$PackageFolder$\ApiPort.VisualStudio.Common.dll")]
[assembly: ProvideCodeBase(CodeBase = @"$PackageFolder$\ApiPort.VisualStudio.dll")]
[assembly: ProvideCodeBase(CodeBase = @"$PackageFolder$\Autofac.dll")]
[assembly: ProvideCodeBase(CodeBase = @"$PackageFolder$\Microsoft.AspNetCore.WebUtilities.dll")]
[assembly: ProvideCodeBase(CodeBase = @"$PackageFolder$\Microsoft.Cci.dll")]
[assembly: ProvideCodeBase(CodeBase = @"$PackageFolder$\Microsoft.Extensions.Primitives.dll")]
[assembly: ProvideCodeBase(CodeBase = @"$PackageFolder$\Microsoft.Fx.Portability.Cci.dll")]
[assembly: ProvideCodeBase(CodeBase = @"$PackageFolder$\Microsoft.Fx.Portability.dll")]
[assembly: ProvideCodeBase(CodeBase = @"$PackageFolder$\Microsoft.Net.Http.Headers.dll")]
[assembly: ProvideCodeBase(CodeBase = @"$PackageFolder$\Microsoft.Win32.Primitives.dll")]
[assembly: ProvideCodeBase(CodeBase = @"$PackageFolder$\Newtonsoft.Json.dll")]
[assembly: ProvideCodeBase(CodeBase = @"$PackageFolder$\System.AppContext.dll")]
[assembly: ProvideCodeBase(CodeBase = @"$PackageFolder$\System.Buffers.dll")]
[assembly: ProvideCodeBase(CodeBase = @"$PackageFolder$\System.Collections.Immutable.dll")]
[assembly: ProvideCodeBase(CodeBase = @"$PackageFolder$\System.Collections.NonGeneric.dll")]
[assembly: ProvideCodeBase(CodeBase = @"$PackageFolder$\System.Composition.AttributedModel.dll")]
[assembly: ProvideCodeBase(CodeBase = @"$PackageFolder$\System.Composition.Convention.dll")]
[assembly: ProvideCodeBase(CodeBase = @"$PackageFolder$\System.Composition.Hosting.dll")]
[assembly: ProvideCodeBase(CodeBase = @"$PackageFolder$\System.Composition.Runtime.dll")]
[assembly: ProvideCodeBase(CodeBase = @"$PackageFolder$\System.Composition.TypedParts.dll")]
[assembly: ProvideCodeBase(CodeBase = @"$PackageFolder$\System.Console.dll")]
[assembly: ProvideCodeBase(CodeBase = @"$PackageFolder$\System.Diagnostics.FileVersionInfo.dll")]
[assembly: ProvideCodeBase(CodeBase = @"$PackageFolder$\System.Diagnostics.TraceSource.dll")]
[assembly: ProvideCodeBase(CodeBase = @"$PackageFolder$\System.Globalization.Calendars.dll")]
[assembly: ProvideCodeBase(CodeBase = @"$PackageFolder$\System.IO.Compression.dll")]
[assembly: ProvideCodeBase(CodeBase = @"$PackageFolder$\System.IO.Compression.ZipFile.dll")]
[assembly: ProvideCodeBase(CodeBase = @"$PackageFolder$\System.IO.FileSystem.dll")]
[assembly: ProvideCodeBase(CodeBase = @"$PackageFolder$\System.IO.FileSystem.Primitives.dll")]
[assembly: ProvideCodeBase(CodeBase = @"$PackageFolder$\System.Net.Http.dll")]
[assembly: ProvideCodeBase(CodeBase = @"$PackageFolder$\System.Net.Sockets.dll")]
[assembly: ProvideCodeBase(CodeBase = @"$PackageFolder$\System.Runtime.CompilerServices.Unsafe.dll")]
[assembly: ProvideCodeBase(CodeBase = @"$PackageFolder$\System.Runtime.InteropServices.RuntimeInformation.dll")]
[assembly: ProvideCodeBase(CodeBase = @"$PackageFolder$\System.Security.Cryptography.Algorithms.dll")]
[assembly: ProvideCodeBase(CodeBase = @"$PackageFolder$\System.Security.Cryptography.Encoding.dll")]
[assembly: ProvideCodeBase(CodeBase = @"$PackageFolder$\System.Security.Cryptography.Primitives.dll")]
[assembly: ProvideCodeBase(CodeBase = @"$PackageFolder$\System.Security.Cryptography.X509Certificates.dll")]
[assembly: ProvideCodeBase(CodeBase = @"$PackageFolder$\System.Text.Encodings.Web.dll")]
[assembly: ProvideCodeBase(CodeBase = @"$PackageFolder$\System.Threading.Tasks.Dataflow.dll")]
[assembly: ProvideCodeBase(CodeBase = @"$PackageFolder$\System.Xml.ReaderWriter.dll")]
