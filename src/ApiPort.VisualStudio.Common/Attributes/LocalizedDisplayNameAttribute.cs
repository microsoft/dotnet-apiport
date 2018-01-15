using ApiPortVS.Resources;
using System;
using System.ComponentModel;

namespace ApiPortVS.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Event)]
    public class LocalizedDisplayNameAttribute : DisplayNameAttribute
    {
        /// <summary>
        /// Creates a display name for the decorated member
        /// </summary>
        /// <param name="resourceName">Name of the resource in <see cref="LocalizedStrings"/></param>
        public LocalizedDisplayNameAttribute(string resourceName)
            : base(LocalizedStrings.ResourceManager.GetString(resourceName, LocalizedStrings.Culture))
        {
        }
    }
}
