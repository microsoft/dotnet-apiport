using ApiPortVS.Resources;
using System;
using System.ComponentModel;

namespace ApiPortVS.Attributes
{
    /// <summary>
    /// Localizes a category string given its name within <see cref="LocalizedStrings"/>.
    /// </summary>
    [AttributeUsage(AttributeTargets.All)]
    public class LocalizedCategoryAttribute : CategoryAttribute
    {
        /// <summary>
        /// Creates a category for the decorated member
        /// </summary>
        /// <param name="resourceName">Name of the resource in <see cref="LocalizedStrings"/></param>
        public LocalizedCategoryAttribute(string resourceName)
            : base(LocalizedStrings.ResourceManager.GetString(resourceName, LocalizedStrings.Culture))
        { }
    }
}
