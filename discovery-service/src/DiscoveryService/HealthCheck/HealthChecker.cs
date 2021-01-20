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

            using(var transaction = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                foreach (var service in servicesRegistered.Distinct())
                {
                    var isAlive = IsServiceUp(_channelFactory.GetChannel(service.Name));
                    _serviceRegisteroperations.SetServiceState(service.Id, isAlive);
                }

                transaction.Complete();
            }
        }

        private bool IsServiceUp(Channel channel)
        {
            var client = _serviceClientFactory(channel);

            try
            {
                var result = client.GetStatus(new Empty(), deadline: DateTime.UtcNow.AddSeconds(10));
                return result.Response == "Pong";
            }
            catch
            {
                return false;
            }
        }

    }
}
