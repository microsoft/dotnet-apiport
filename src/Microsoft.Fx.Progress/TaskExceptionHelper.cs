using System;

namespace Microsoft.Fx.Progress
{
    internal static class TaskExceptionHelper
    {
        public static void ThrowIfNotCancelled(Exception exception)
        {
            if (exception is OperationCanceledException)
                return;

            var agg = exception as AggregateException;
            if (agg != null)
            {
                foreach (var innerException in agg.InnerExceptions)
                    ThrowIfNotCancelled(innerException);

                return;
            }

            throw new Exception("The operation failed.", exception);
        }
    }
}