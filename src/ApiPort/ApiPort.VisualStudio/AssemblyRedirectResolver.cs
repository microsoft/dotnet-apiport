// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;

namespace ApiPortVS
{
    /// <summary>
    /// Because the VSIX runs inside devenv.exe, the only configuration file that is used for redirects is
    /// devenv.exe.config.We don't want to modify this machine wide configuration so we are manually setting
    /// the redirects.
    /// More info: http://stackoverflow.com/a/31248093/4220757
    /// </summary>
    internal class AssemblyRedirectResolver
    {
        private readonly IDictionary<string, AssemblyRedirect> _redirectsDictionary;

        public AssemblyRedirectResolver(string configFile)
        {
            XName GetFullName(string name)
            {
                return XName.Get(name, "urn:schemas-microsoft-com:asm.v1");
            }

            var xml = XDocument.Load(configFile);
            var redirects = from element in xml.Descendants(GetFullName("dependentAssembly"))
                            let identity = element.Element(GetFullName("assemblyIdentity"))
                            let redirect = element.Element(GetFullName("bindingRedirect"))
                            let name = identity.Attribute("name").Value
                            let publicKey = identity.Attribute("publicKeyToken").Value
                            let newVersion = redirect.Attribute("newVersion").Value
                            select new AssemblyRedirect(name, newVersion, publicKey);

            _redirectsDictionary = redirects.ToDictionary(x => x.Name);
        }

        public AssemblyRedirectResolver(DirectoryInfo assemblyFolder)
        {
            var redirects = assemblyFolder.GetFiles("*.dll")
                .Select(dll =>
                {
                    var name = AssemblyName.GetAssemblyName(dll.FullName);
                    var publicKeyToken = name.GetPublicKeyToken().Aggregate(string.Empty, (s, b) => s += b.ToString("x2", CultureInfo.InvariantCulture));
                    return new AssemblyRedirect(name.Name, name.Version.ToString(), publicKeyToken);
                });

            _redirectsDictionary = redirects.ToDictionary(x => x.Name);
        }

        public Assembly ResolveAssembly(string assemblyName, Assembly requestingAssembly)
        {
            // Use latest strong name & version when trying to load SDK assemblies
            var requestedAssembly = new AssemblyName(assemblyName);

            if (!_redirectsDictionary.TryGetValue(requestedAssembly.Name, out var redirectInformation))
            {
                return null;
            }

            Trace.WriteLine("Redirecting assembly load of " + assemblyName
                          + ",\tloaded by " + (requestingAssembly == null ? "(unknown)" : requestingAssembly.FullName));

            var alreadyLoadedAssembly = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(a =>
            {
                var assm = a.GetName();
                return string.Equals(assm.Name, requestedAssembly.Name, StringComparison.Ordinal)
                    && redirectInformation.TargetVersion.Equals(assm.Version);
            });

            if (alreadyLoadedAssembly != default)
            {
                return alreadyLoadedAssembly;
            }

            requestedAssembly.Version = redirectInformation.TargetVersion;
            requestedAssembly.SetPublicKeyToken(new AssemblyName("x, PublicKeyToken=" + redirectInformation.PublicKeyToken).GetPublicKeyToken());
            requestedAssembly.CultureInfo = CultureInfo.InvariantCulture;

            return Assembly.Load(requestedAssembly);
        }
    }

    internal readonly struct AssemblyRedirect
    {
        public string Name { get; }

        public string PublicKeyToken { get; }

        public Version TargetVersion { get; }

        public AssemblyRedirect(string name, string version, string publicKeyToken)
        {
            Name = name;
            TargetVersion = new Version(version);
            PublicKeyToken = publicKeyToken;
        }
    }
}
