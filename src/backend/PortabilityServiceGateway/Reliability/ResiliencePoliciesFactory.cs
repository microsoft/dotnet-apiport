// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Polly;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace PortabilityServiceGateway.Reliability
{
    public class ResiliencePoliciesFactory
    {
        private readonly ILogger<ResiliencePoliciesFactory> _logger;
        private readonly int _circuitBreakerDuration;
        private readonly int _retryCount;
        private readonly double _retryBaseDelay;
        private readonly int _exceptionsAllowedBeforeCircuitBreak;
        private readonly int _timeoutSeconds;

        public ResiliencePoliciesFactory(ILogger<ResiliencePoliciesFactory> logger, IConfiguration configuration)
        {
            _logger = logger;

            if (!int.TryParse(configuration["ResilientHttp:CircuitBreakerDurationSeconds"], out _circuitBreakerDuration))
            {
                _circuitBreakerDuration = 60;
            }

            if (!double.TryParse(configuration["ResilientHttp:RetryBaseDelaySeconds"], out _retryBaseDelay))
            {
                _retryBaseDelay = 0.5;
            }

            if (!int.TryParse(configuration["ResilientHttp:RetryCount"], out _retryCount))
            {
                _retryCount = 3;
            }

            if (!int.TryParse(configuration["ResilientHttp:ExceptionsBeforeCircuitBreak"], out _exceptionsAllowedBeforeCircuitBreak))
            {
                _exceptionsAllowedBeforeCircuitBreak = 3;
            }

            if (!int.TryParse(configuration["ResilientHttp:TimeoutSeconds"], out _timeoutSeconds))
            {
                _timeoutSeconds = 3;
            }
        }

        /// <summary>
        /// Instantiates Polly resilience policies
        /// </summary>
        /// <returns>A collection of reslience policies</returns>
        internal Policy[] CreatePolicies()
        {
            // Retry failed requests
            var standardHttpRetry = Policy.Handle<HttpRequestException>()

                // Number of times to retry and backoff function
                .WaitAndRetryAsync(_retryCount, i => TimeSpan.FromSeconds(Math.Pow(2, i) *_retryBaseDelay),
                (exception, waitDuration, retryCount, context) =>
                {
                    // Log warning if retries don't work
                    _logger.LogWarning("Retrying ({retryCount}) after {waitDuration} seconds due to: [{exceptionType}] {exception}",
                        retryCount,
                        waitDuration.TotalSeconds,
                        exception.GetType().Name,
                        exception.Message);
                    _logger.LogTrace("Full exception: {exception}", exception.ToString());
                });

            // Stop trying requests that repeatedly fail
            var standardHttpCircuitBreaker = Policy.Handle<HttpRequestException>()
                .CircuitBreakerAsync(_exceptionsAllowedBeforeCircuitBreak, TimeSpan.FromSeconds(_circuitBreakerDuration),
                (exception, duration) =>
                {
                    // Log warning when circuit break is opened
                    _logger.LogWarning("Circuit breaker opened for {circuitBreakerDuration} seconds due to: [{exceptionType}] {exception}",
                        duration.TotalSeconds,
                        exception.GetType().Name,
                        exception.Message);
                    _logger.LogTrace("Full exception: {exception}", exception.ToString());
                },
                () =>
                {
                    // Log informational message when the circuit breaker resets
                    _logger.LogInformation("Circuit breaker closed");
                },
                () =>
                {
                    // Log informational message when the circuit breaker is half-opem
                    _logger.LogInformation("Circuit half-open");
                });

            // Stop waiting on requests after a period of time
            // Ocelot defaults to perssimistic timeout
            var standardHttpTimeout = Policy.TimeoutAsync(TimeSpan.FromSeconds(_timeoutSeconds), Polly.Timeout.TimeoutStrategy.Pessimistic,
                (context, timespan, timedOutAction) =>
                {
                    _logger.LogWarning("Downstream call timed out after {timeout} seconds", timespan.TotalSeconds);
                    return Task.CompletedTask;
                });

            // The order of policies matters. When wrapped, the later policies apply first (they are the 'inner' policies)
            // So, in this example, circuit breaker checks are done before retries.
            return new Policy[] { standardHttpRetry, standardHttpCircuitBreaker, standardHttpTimeout };
        }
    }
}
