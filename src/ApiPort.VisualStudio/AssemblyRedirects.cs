// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;

namespace ApiPortVS
{
    /// <summary>
    /// Because the VSIX runs inside devenv.exe, the only configuration file
    /// that is used for redirects is devenv.exe.config. We don't want to modify
    /// the machine wide configuration so we are manually setting the redirects.
    /// More info: http://stackoverflow.com/a/31248093/4220757
    /// </summary>
    internal class AssemblyRedirects
    {
        private readonly IEnumerable<AssemblyRedirect> _redirects = new List<AssemblyRedirect>();
        private readonly IDictionary<string, AssemblyRedirect> _redirectsDictionary = new Dictionary<string, AssemblyRedirect>();

        public AssemblyRedirects(string configFile)
        {
            XDocument xml;

            try
            {
                xml = XDocument.Load(configFile);
            }
            catch (Exception e)
            {
                Trace.TraceError($"Unable to load {configFile}. Exception: {e}");

                return;
            }

            Func<string, XName> getFullName = (name) => { return XName.Get(name, "urn:schemas-microsoft-com:asm.v1"); };

            var redirects = from element in xml.Descendants(getFullName("dependentAssembly"))
                            let identity = element.Element(getFullName("assemblyIdentity"))
                            let redirect = element.Element(getFullName("bindingRedirect"))
                            let name = identity.Attribute("name").Value
                            let publicKey = identity.Attribute("publicKeyToken").Value
                            let newVersion = redirect.Attribute("newVersion").Value
                            select new AssemblyRedirect(name, newVersion, publicKey);

            _redirects = redirects;
            _redirectsDictionary = redirects.ToDictionary(x => x.Name);
        }

        public Assembly ResolveAssembly(string assemblyName, Assembly requestingAssembly)
        {
            // Use latest strong name & version when trying to load SDK assemblies
            var requestedAssembly = new AssemblyName(assemblyName);

            AssemblyRedirect redirectInformation;

            if (!_redirectsDictionary.TryGetValue(requestedAssembly.Name, out redirectInformation))
            {
                return null;
            }

            Trace.WriteLine("Redirecting assembly load of " + assemblyName
                          + ",\tloaded by " + (requestingAssembly == null ? "(unknown)" : requestingAssembly.FullName));

            var alreadyLoadedAssembly = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(a => a.GetName().Name == requestedAssembly.Name);

            if (alreadyLoadedAssembly != null)
            {
                return alreadyLoadedAssembly;
            }

            requestedAssembly.Version = redirectInformation.TargetVersion;
            requestedAssembly.SetPublicKeyToken(new AssemblyName("x, PublicKeyToken=" + redirectInformation.PublicKeyToken).GetPublicKeyToken());
            requestedAssembly.CultureInfo = CultureInfo.InvariantCulture;

            return Assembly.Load(requestedAssembly);
        }
    }

    internal class AssemblyRedirect
    {
        public readonly string Name;

        public readonly string PublicKeyToken;

        public readonly Version TargetVersion;

        public AssemblyRedirect(string name, string version, string publicKeyToken)
        {
            Name = name;
            TargetVersion = new Version(version);
            PublicKeyToken = publicKeyToken;
        }
    }
}
