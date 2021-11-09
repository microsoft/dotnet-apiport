// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Autofac;
using Microsoft.VisualStudio.Shell;

namespace ApiPortVS
{
    /// <summary>
    /// Registering a COM instance as a specific type with <see cref="Autofac.Builder.IRegistrationBuilder{TLimit, TActivatorData, TRegistrationStyle}.As{TService}"/>
    /// fails. This is probably due to it not showing via reflection that it is of the specified type. An easy work around is to register it as a lambda instead.
    /// </summary>
    internal static class AutofacComRegisterExtensions
    {
        public static void RegisterComInstance<TImplementation, TService>(this ContainerBuilder builder)
        {
            var instance = Package.GetGlobalService(typeof(TImplementation));

            builder.Register(_ => (TService)instance)
                .SingleInstance()
                .As<TService>();
        }

        public static void RegisterCom<T>(this ContainerBuilder builder, object item)
        {
            builder.Register(_ => (T)item)
                .SingleInstance()
                .As<T>();
        }
    }
}
