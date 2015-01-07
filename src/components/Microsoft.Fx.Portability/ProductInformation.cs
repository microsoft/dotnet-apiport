using Microsoft.Fx.Portability.Resources;
using System;
using System.Reflection;

namespace Microsoft.Fx.Portability
{
    public class ProductInformation
    {
        private readonly string _name;
        private readonly string _version;

        public ProductInformation(string name)
        {
            var version = GetVersionString();

            if (!IsValid(name))
            {
                throw new ArgumentOutOfRangeException("name", LocalizedStrings.ProductInformationInvalidArgument);
            }

            if (!IsValid(version))
            {
                throw new ArgumentOutOfRangeException("version", LocalizedStrings.ProductInformationInvalidArgument);
            }

            _name = name;
            _version = version;
        }

        private static string GetVersionString()
        {
            var entryAssembly = Assembly.GetEntryAssembly() ?? Assembly.GetCallingAssembly();
            var assemblyName = entryAssembly.GetName();
            var assemblyVersion = assemblyName.Version;

            return assemblyVersion == null ? "unknown" : assemblyVersion.ToString();
        }

        /// <summary>
        /// Verify strings/versions only contain letters, digits, '.', or '_'.  Otherwise, the user agent string may be created incorrectly
        /// </summary>
        private static bool IsValid(string str)
        {
            foreach (var s in str.ToCharArray())
            {
                if (!char.IsLetterOrDigit(s) && s != '.' && s != '_')
                {
                    return false;
                }
            }

            return true;
        }

        public string Name { get { return _name; } }
        public string Version { get { return _version; } }
    }
}
