using Microsoft.Fx.Portability.Resources;
using System;

namespace Microsoft.Fx.Portability
{
    public class InternalServerErrorException : PortabilityAnalyzerException
    {
        private InternalServerErrorException(string message, string exception)
            : base(string.Format(LocalizedStrings.InternalServerErrorMessage, message, exception))
        { }

        /// <summary>
        /// This returns a properly formatted exception received from the server.
        /// </summary>
        public static InternalServerErrorException Create(string message, string exception)
        {
            var split = exception.Split(new[] { "\\r\\n", "\\r", "\\n" }, StringSplitOptions.RemoveEmptyEntries);

            return new InternalServerErrorException(message, string.Join(Environment.NewLine, split));
        }
    }
}
