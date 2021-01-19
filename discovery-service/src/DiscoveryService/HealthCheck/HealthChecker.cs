using System;
using System.Collections.Generic;
using System.Linq;
using DiscoveryService.Factory;
using Healthcheck;
using Grpc.Core;
using DiscoveryService.Infra.Operations;
using DiscoveryService.Infra.UnitOfWork;
using static Healthcheck.HealthCheckService;

namespace DiscoveryService.HealthCheck
{
    public class HealthChecker
    {
        private readonly IServiceRegisterOperations _serviceRegisteroperations;
        private readonly IServiceRegisterUnitOfWork _serviceRegisterUnitOfWork;
        private readonly ChannelFactory _channelFactory;
        private readonly Func<Channel, HealthCheckServiceClient> _serviceClientFactory;


        public HealthChecker(
            IServiceRegisterOperations serviceRegisteroperations,
            IServiceRegisterUnitOfWork serviceRegisterUnitOfWork,
            ChannelFactory channelFactory,
            Func<Channel, HealthCheckServiceClient> serviceClientFactory)
        {
            _serviceRegisteroperations = serviceRegisteroperations;
            _serviceRegisterUnitOfWork = serviceRegisterUnitOfWork;
            _channelFactory = channelFactory;
            _serviceClientFactory = serviceClientFactory;
        }

        public void Handle(object state)
        {
            // Get all services registered 
            var servicesRegistered = _serviceRegisteroperations.GetAllServices();

            _serviceRegisterUnitOfWork.BeginTransaction();

            foreach (var service in servicesRegistered.Distinct())
            {
                var isAlive = IsServiceUp(_channelFactory.GetChannel(service.Name));
                _serviceRegisteroperations.SetServiceState(service.Id, isAlive);
            }

            _serviceRegisterUnitOfWork.CommitTransaction();
            
        }

        private bool IsServiceUp(Channel channel)
        {
            var client = _serviceClientFactory(channel);

            try
            {
                var result = client.GetStatus(new Empty());
                return result.Response == "Pong";
            }
            catch
            {
                return false;
            }
        }

    }
}
