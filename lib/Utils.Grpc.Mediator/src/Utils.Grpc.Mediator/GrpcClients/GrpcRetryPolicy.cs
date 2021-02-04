using Microsoft.Extensions.Logging;
using Polly;
using Polly.Retry;
using System;

namespace Utils.Grpc.Mediator.GrpcClients
{
    internal class GrpcRetryPolicy
    {
        private readonly ILogger<GrpcRetryPolicy> _logger;
        private int RetryAttempts = 3;
        // Time in seconds
        private int CummulativeRetryTime = 1;


        public GrpcRetryPolicy(ILogger<GrpcRetryPolicy> logger)
        {
            _logger = logger;
        }

        public RetryPolicy GetRetryPolicy() =>
            Policy.Handle<Exception>()
                .WaitAndRetry(
                    RetryAttempts,
                    retryAttempt =>
                    {
                        _logger.LogInformation("Retrying call attempt {0} of {1}",
                                               retryAttempt,
                                               RetryAttempts);
                        return TimeSpan.FromSeconds((retryAttempt * CummulativeRetryTime));
                    }
            );
    }
}