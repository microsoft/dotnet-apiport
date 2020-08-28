// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using ApiPort.Resources;
using Microsoft.Fx.Portability;
using Microsoft.Fx.Portability.Proxy;
using System;
using System.Net;
using System.Security;
using System.Threading;
using System.Threading.Tasks;

namespace ApiPort
{
    /// <summary>
    /// Credential provider for console input.
    /// Taken from: https://github.com/NuGet/NuGet.Client/blob/dev/src/NuGet.Clients/NuGet.CommandLine/Common/ConsoleCredentialProvider.cs.
    /// </summary>
    public class ConsoleCredentialProvider : ICredentialProvider
    {
        private readonly IProgressReporter _progressReporter;

        public ConsoleCredentialProvider(IProgressReporter progressReporter)
        {
            _progressReporter = progressReporter;
        }

        /// <summary>
        /// Fetches credentials for the given uri and proxy.
        /// </summary>
        public Task<NetworkCredential> GetCredentialsAsync(Uri uri, IWebProxy proxy, CredentialRequestType type, CancellationToken cancellationToken)
        {
            string message;

            switch (type)
            {
                case CredentialRequestType.Proxy:
                    message = LocalizedStrings.Credentials_ProxyCredentials;
                    break;

                case CredentialRequestType.Forbidden:
                    message = LocalizedStrings.Credentials_ForbiddenCredentials;
                    break;

                default:
                    message = LocalizedStrings.Credentials_RequestCredentials;
                    break;
            }

            try
            {
                _progressReporter.Suspend();

                Console.WriteLine();
                Console.WriteLine(message, uri.OriginalString);
                Console.Write(LocalizedStrings.Credentials_UserName);
                cancellationToken.ThrowIfCancellationRequested();

                string username = Console.ReadLine();
                Console.Write(LocalizedStrings.Credentials_Password);

                using (var password = new SecureString())
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    ReadSecureStringFromConsole(password);

                    var credentials = new NetworkCredential
                    {
                        UserName = username,
                        SecurePassword = password
                    };

                    return Task.FromResult(credentials);
                }
            }
            finally
            {
                _progressReporter.Resume();
            }
        }

        /// <summary>
        /// Using Console input, retrieves a SecureString.
        /// </summary>
        private static void ReadSecureStringFromConsole(SecureString secureString)
        {
            ConsoleKeyInfo keyInfo;
            while ((keyInfo = Console.ReadKey(intercept: true)).Key != ConsoleKey.Enter)
            {
                if (keyInfo.Key == ConsoleKey.Backspace)
                {
                    if (secureString.Length < 1)
                    {
                        continue;
                    }

                    Console.SetCursorPosition(Console.CursorLeft - 1, Console.CursorTop);
                    Console.Write(' ');
                    Console.SetCursorPosition(Console.CursorLeft - 1, Console.CursorTop);
                    secureString.RemoveAt(secureString.Length - 1);
                }
                else
                {
                    secureString.AppendChar(keyInfo.KeyChar);
                    Console.Write('*');
                }
            }

            Console.WriteLine();
        }
    }
}
