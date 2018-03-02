// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Fx.Portability.ObjectModel;
using Xunit;

namespace Microsoft.Fx.Portability.Tests.ObjectModel
{
    public class DiagnosticAnalyzerInfoTests
    {
        [Fact]
        public static void NullIdReturnsNegativeOne()
        {
            var diagnosticAnalyzerInfo = new DiagnosticAnalyzerInfo
            {
                AnalyzerName = "Test Analyzer",
                Id = null
            };

            Assert.Equal(-1, diagnosticAnalyzerInfo.GetIdNumber());
        }

        [Fact]
        public static void EmptyIdReturnsNegativeOne()
        {
            var diagnosticAnalyzerInfo = new DiagnosticAnalyzerInfo
            {
                AnalyzerName = "Test Analyzer",
                Id = string.Empty
            };

            Assert.Equal(-1, diagnosticAnalyzerInfo.GetIdNumber());
        }

        [Fact]
        public static void ExpectedIdFormatWithTrailingCharacterReturnsIntId()
        {
            var diagnosticAnalyzerInfo = new DiagnosticAnalyzerInfo
            {
                AnalyzerName = "Test Analyzer",
                Id = "CD0001A"
            };

            Assert.Equal(1, diagnosticAnalyzerInfo.GetIdNumber());
        }

        [Fact]
        public static void ExpectedIdFormatWithoutTrailingCharacterReturnsIntId()
        {
            var diagnosticAnalyzerInfo = new DiagnosticAnalyzerInfo
            {
                AnalyzerName = "Test Analyzer",
                Id = "CD0003"
            };

            Assert.Equal(3, diagnosticAnalyzerInfo.GetIdNumber());
        }

        [Fact]
        public static void NoNumberIdReturnsNegativeOne()
        {
            var diagnosticAnalyzerInfo = new DiagnosticAnalyzerInfo
            {
                AnalyzerName = "Test Analyzer",
                Id = "No Number"
            };

            Assert.Equal(-1, diagnosticAnalyzerInfo.GetIdNumber());
        }
    }
}
