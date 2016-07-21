using System;
using System.Collections.Generic;
using Microsoft.Cci.MetadataReader.ObjectModelImplementation;

namespace Microsoft.Cci.MetadataReader.Extensions
{
    public static class ModuleExtensions
    {
        public static IEnumerable<ITypeMemberReference> GetConstructedTypeInstanceMembers(this IModule module)
        {
            if (!(module is Module))
            {
                throw new NotSupportedException("An IModule created using PEReader is required.");
            }

            return (module as Module).GetConstructedTypeInstanceMembers();
        }
    }
}
