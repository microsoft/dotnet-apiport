namespace Microsoft.Fx.Portability
{
    /// <summary>
    /// Thrown when <see cref="IApiPortOptions"/> is invalid.
    /// </summary>
    public class InvalidApiPortOptionsException : PortabilityAnalyzerException
    {
        public InvalidApiPortOptionsException(string message) 
            : base(message)
        { }
    }
}
