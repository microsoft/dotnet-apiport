// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Ocelot.Configuration;
using Ocelot.Requester.QoS;
using Polly;
using Polly.Timeout;

namespace PortabilityService.Gateway.Reliability
{
    /// <summary>
    /// Custom quality-of-service provider to create Polly policies tailored
    /// to PortabilityService scenarios.
    /// 
    /// Note that these policies are currently very similar to Ocelot defaults 
    /// because there's not a lot of flexibility in the types of policies Ocelot accepts. 
    /// There is a work item (https://github.com/ThreeMammals/Ocelot/issues/264) open to fix 
    /// that and this type should be updated according to the comments below once that's done.
    /// </summary>
    public class PortabilityServiceQoSProvider : IQoSProvider
    {
        private readonly ILogger _logger;
        private readonly CircuitBreaker _circuitBreaker;

        public PortabilityServiceQoSProvider(DownstreamReRoute reRoute, ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<PortabilityServiceQoSProvider>();
            _circuitBreaker = CreateOcelotCircuitBreaker(reRoute.QosOptionsOptions);
        }

        private CircuitBreaker CreateOcelotCircuitBreaker(QoSOptions options)
        {
            // Retry failed requests
            // TODO : Ocelot does not currently allow more policies to be added.
            //        Update Ocelot to allow this and then add this policy.
            var retryCount = 3; // options.RetryCount;
            var retryBaseDelay = 0.5; // options.RetryBaseDelay
            var standardRetryPolicy = Policy
                .Handle<HttpRequestException>()
                .Or<TimeoutException>()
                .Or<TimeoutRejectedException>()
                .OrResult<HttpResponseMessage>(response => ReportsServerError(response))
                // Number of times to retry and backoff function
                .WaitAndRetryAsync(retryCount, i => TimeSpan.FromSeconds(Math.Pow(2, i) * retryBaseDelay),
                    (failureResult, waitDuration, count, context) =>
                    {
                        // Log warning if retries don't work
                        _logger.LogWarning("Retrying ({retryCount}) after {waitDuration} due to: {error}",
                            count,
                            waitDuration,
                            GetError(failureResult));
                    });

            // Stop trying requests that repeatedly fail
            var circuitBreakerPolicy = Policy
                .Handle<HttpRequestException>()
                .Or<TimeoutException>()
                .Or<TimeoutRejectedException>()
                // TODO : Ocelot forces policies to be one CircuitBreakPolicy (*not* CircuitBreakerPolicy<T> which is what we get
                //        if we handle a non-exceptional response type and one TimeoutPolicy. I should look into updating it 
                //        to use an arbitrary PolicyWrap or at least IEnumerable<IPolicy>.
                // .OrResult<HttpResponseMessage>(response => ReportsServerError(response))
                .CircuitBreakerAsync(options.ExceptionsAllowedBeforeBreaking, TimeSpan.FromMilliseconds(options.DurationOfBreak),
                    (exception, duration) =>
                    {
                        // Log warning when circuit break is opened
                        _logger.LogWarning("Circuit breaker opened for {circuitBreakerDuration} due to: {error}",
                            duration,
                            exception);
                    },
                    () =>
                    {
                        // Log informational message when the circuit breaker resets
                        _logger.LogInformation("Circuit breaker closed");
                    },
                    () =>
                    {
                        // Log informational message when the circuit breaker is half-open
                        _logger.LogInformation("Circuit half-open");
                    });

            // Stop waiting on requests after a period of time
            var timeoutPolicy = Policy.TimeoutAsync(TimeSpan.FromMilliseconds(options.TimeoutValue), options.TimeoutStrategy,
                (context, timespan, timedOutAction) =>
                {
                    _logger.LogWarning("Downstream call timed out after {timeout}", timespan);
                    return Task.CompletedTask;
                });

            return new CircuitBreaker(/*standardRetryPolicy,*/ circuitBreakerPolicy, timeoutPolicy);
        }

        private static object GetError(DelegateResult<HttpResponseMessage> failureResult) =>
            (failureResult.Exception != null) ?
                failureResult.Exception :
                (object)failureResult.Result;

        private static bool ReportsServerError(HttpResponseMessage response) => ((int)response.StatusCode) / 100 == 5;

        public CircuitBreaker CircuitBreaker => _circuitBreaker;
    }
}
