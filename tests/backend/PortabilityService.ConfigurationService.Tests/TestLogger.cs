// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;

namespace PortabilityService.ConfigurationService.Tests
{
    /// <summary>
    /// Logging: This class creates a custom logger that is used in the unit tests to verify that
    ///          logging is working as expected in the methods being tested.
    /// </summary>
    public class TestLogger<TCategory> : ILogger<TCategory>
    {
        private const string LogFormat = "[{0}] -- {1}";
        private const string LogFormatWithPrefix = "[{0}] -- {1}: {2}";

        public ConcurrentBag<string> LoggedMessages { get; private set; } = new ConcurrentBag<string>();

        public IDisposable BeginScope<TState>(TState state)
        {
            throw new NotImplementedException();
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return true;
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            LoggedMessages.Add(string.Format(CultureInfo.InvariantCulture, LogFormat, logLevel, state));
        }

        /// <summary>
        /// Removes all items from the LoggedMessages
        /// </summary>
        public void ClearMessages()
        {
            LoggedMessages.Clear();
        }

        /// <summary>
        /// Builds a string representing the log message based on the passed in LogLevel and Message
        /// </summary>
        public string GetLogString(LogLevel logLevel, string message) => string.Format(CultureInfo.InvariantCulture, LogFormat, logLevel, message);

        /// <summary>
        /// Builds a string representing the log message based on the passed in LogLevel, methodName and Message
        /// </summary>
        public string GetLogString(LogLevel logLevel, string methodName, string message) => string.Format(CultureInfo.InvariantCulture, LogFormatWithPrefix, logLevel, methodName, message);
    }
}
