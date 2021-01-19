using System;
using System.Collections.Generic;
using System.Text;
using DiscoveryService.Extensions;
using System.Linq;
using DiscoveryService.Factory;
using Healthcheck;
using Grpc.Net.Client;
using System.Threading.Tasks;
using Grpc.Core;
using DiscoveryService.Infra.Operations;
using DiscoveryService.Infra.UnitOfWork;

namespace DiscoveryService.HealthCheck
{
    public class HealthChecker
    {
        private readonly IServiceRegisterOperations _serviceRegisterRepository;
        private readonly IServiceRegisterUnitOfWork _serviceRegisterUnitOfWork;
        private readonly ChannelFactory _channelFactory;

        public HealthChecker(
            IServiceRegisterOperations serviceRegisterRepository,
            IServiceRegisterUnitOfWork serviceRegisterUnitOfWork,
            ChannelFactory channelFactory)
        {
            _serviceRegisterRepository = serviceRegisterRepository;
            _serviceRegisterUnitOfWork = serviceRegisterUnitOfWork;
            _channelFactory = channelFactory;
        }

        public void Handle(object state)
        {
            // Get all services registered 
            var servicesRegistered = _serviceRegisterRepository.GetAll();

            _serviceRegisterUnitOfWork.BeginTransaction();

            foreach (var service in servicesRegistered.Select(i => i.Service).Distinct())
            {
                var isAlive = IsServiceUp(_channelFactory.GetChannel(service.Name));
                _serviceRegisterRepository.SetServiceState(service.Id, isAlive);
            }

            _serviceRegisterUnitOfWork.CommitTransaction();
            
        }

        private bool IsServiceUp(Channel channel)
        {
            var client = new HealthCheckService.HealthCheckServiceClient(channel);

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

        //private KeyValuePair<string, string> GetKeyValuePair(string key, IEnumerable<string> value)
        //{
        //    return new KeyValuePair<string, string>(key, string.Join(";", value));
        //}
    }
}
