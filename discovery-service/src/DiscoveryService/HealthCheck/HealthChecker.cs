using System;
using System.Collections.Generic;
using System.Linq;
using DiscoveryService.Factory;
using Healthcheck;
using Grpc.Core;
using DiscoveryService.Infra.Operations;
using static Healthcheck.HealthCheckService;
using System.Transactions;

namespace DiscoveryService.HealthCheck
{
    public class HealthChecker
    {
        private readonly IServiceRegisterOperations _serviceRegisteroperations;
        private readonly ChannelFactory _channelFactory;
        private readonly Func<Channel, HealthCheckServiceClient> _serviceClientFactory;


        public HealthChecker(
            IServiceRegisterOperations serviceRegisteroperations,
            ChannelFactory channelFactory,
            Func<Channel, HealthCheckServiceClient> serviceClientFactory)
        {
            _serviceRegisteroperations = serviceRegisteroperations;
            _channelFactory = channelFactory;
            _serviceClientFactory = serviceClientFactory;
        }

        public void Handle(object state)
        {
            // Get all services registered 
            var servicesRegistered = _serviceRegisteroperations.GetAllServices();
            var operationId = Guid.NewGuid();

            foreach (var service in servicesRegistered.Distinct())
            {
                var isAlive = IsServiceUp(_channelFactory.GetChannel(service.Name), operationId);
                _serviceRegisteroperations.SetServiceState(service.Id, isAlive);
            }

        }

        private bool IsServiceUp(Channel channel, Guid operationId)
        {
            var client = _serviceClientFactory(channel);

            try
            {
                var callContext = GetCallContext(HealthCheckService.Descriptor.FullName,
                                                 nameof(client.GetStatus),
                                                 channel.ResolvedTarget,
                                                 operationId);

                var result = client.GetStatus(new Empty(), callContext);
                return result.Response.ToLower() == "pong";
            }
            catch
            {
                return false;
            }
        }

        private CallOptions GetCallContext(string service, string methodName, string target, Guid operationId)
        {
            var headers = new Metadata
            {
                {"service", service},
                {"rpc", methodName},
                {"operation_id", operationId.ToString()},
                {"target", target}
            };

            return new CallOptions(headers, DateTime.UtcNow.AddSeconds(10));
        }

    }
}
