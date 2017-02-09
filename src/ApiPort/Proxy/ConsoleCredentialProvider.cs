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
    /// Taken from: https://github.com/NuGet/NuGet.Client/blob/dev/src/NuGet.Clients/NuGet.CommandLine/Common/ConsoleCredentialProvider.cs
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
                _progressReporter.SuspendProgressTasks();

                Console.WriteLine();
                Console.WriteLine(message, uri.OriginalString);
                Console.Write(LocalizedStrings.Credentials_UserName);
                cancellationToken.ThrowIfCancellationRequested();

                string username = Console.ReadLine();
                Console.Write(LocalizedStrings.Credentials_Password);

#if FEATURE_NETWORK_CREDENTIAL
                using (SecureString password = new SecureString())
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
#else
                cancellationToken.ThrowIfCancellationRequested();

                var credentials = new NetworkCredential
                {
                    UserName = username,
                    Password = ReadUnsecureStringFromConsole()
                };

                return Task.FromResult(credentials);
#endif
            }
            finally
            {
                _progressReporter.ResumeProgressTasks();
            }
        }

        /// <summary>
        /// Using Console input, retrieves a SecureString
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

#if !FEATURE_NETWORK_CREDENTIAL
        /// <summary>
        /// NOTE: Remove this when NetworkCredential.SecurePassword is
        /// available in .NET Standard 2.0.
        /// </summary>
        private static string ReadUnsecureStringFromConsole()
        {
            var builder = new System.Text.StringBuilder();

            ConsoleKeyInfo keyInfo;
            while ((keyInfo = Console.ReadKey(intercept: true)).Key != ConsoleKey.Enter)
            {
                if (keyInfo.Key == ConsoleKey.Backspace)
                {
                    if (builder.Length < 1)
                    {
                        continue;
                    }
                    Console.SetCursorPosition(Console.CursorLeft - 1, Console.CursorTop);
                    Console.Write(' ');
                    Console.SetCursorPosition(Console.CursorLeft - 1, Console.CursorTop);
                    builder.Remove(builder.Length - 1, 1);
                }
                else
                {
                    builder.Append(keyInfo.KeyChar);
                    Console.Write('*');
                }
            }
            Console.WriteLine();

            return builder.ToString();
        }
#endif
    }
}
