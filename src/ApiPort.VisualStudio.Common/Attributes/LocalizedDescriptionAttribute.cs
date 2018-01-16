using ApiPortVS.Resources;
using System;
using System.ComponentModel;

namespace ApiPortVS.Attributes
{
    /// <summary>
    /// Localizes a description string given its name within <see cref="LocalizedStrings"/>.
    /// </summary>
    [AttributeUsage(AttributeTargets.All)]
    public class LocalizedDescriptionAttribute : DescriptionAttribute
    {
        /// <summary>
        /// Creates a description for the decorated member
        /// </summary>
        /// <param name="resourceName">Name of the resource in <see cref="LocalizedStrings"/></param>
        public LocalizedDescriptionAttribute(string resourceName)
            : base(LocalizedStrings.ResourceManager.GetString(resourceName, LocalizedStrings.Culture))
        {
        }
    }
}
