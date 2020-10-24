using System;
using System.Collections.Generic;
using System.Text;
using DiscoveryService.Extensions;
using System.Linq;
using DiscoveryService.Factory;
using Healthcheck;
using Grpc.Net.Client;
using System.Threading.Tasks;

namespace DiscoveryService.HealthChecker
{
    public class HealthChecker
    {
        private readonly EtcdClientWrap _etcdClient;
        private readonly ChannelFactory _channelFactory;
        private const string SERVICE_KEYS = "Services";

        public HealthChecker(EtcdClientWrap etcdClient, ChannelFactory channelFactory)
        {
            _etcdClient = etcdClient;
            _channelFactory = channelFactory;
        }

        public async Task Handle()
        {
            // Get all services registered on ETCD
            var servicesRegistered = _etcdClient
                                        .GetValue(SERVICE_KEYS)
                                        .SplitIfNotEmpty()
                                        .ToList();

            // Check if the services are UP
            var serviceCheckTasks = servicesRegistered.Select(service => Task.Run(() =>
            {
                return IsServiceUp(_channelFactory.GetChannel(service)) ? string.Empty : service;
            }));

            // Get services to remove
            var servicesToRemove = (await Task.WhenAll(serviceCheckTasks)).Where(i => !string.IsNullOrEmpty(i));

            // Get new existent services (excluding ones aren't up)
            var existentServices = servicesRegistered.Except(servicesToRemove);
            var etcdOperations = new List<Task>(Task.Run(() => _etcdClient.Put(SERVICE_KEYS, existentServices)));

            /* 
               Remove from ETCD all references from the "down" services
               Current situation on ETCD:
               "service1": "method1;method2"
               "service2": "method1"
               "method1": "service1;service2"
               "method2": "service1"

                TODO: Check if it's necessary store services on ETCD
                TODO: Add a key called "Services" on DiscoveryConsumer(ETCD) and remove/add/do nothing depending on the situation
             */
            foreach (var serviceToRemove in servicesToRemove)
            {
                var removeServiceFromHandlers = _etcdClient
                                                    .Get(serviceToRemove)
                                                    .SplitIfNotEmpty()
                                                    .Select(handler => Task.Run(() =>
                                                    {
                                                        var services = _etcdClient.Get(handler).SplitIfNotEmpty();
                                                        services.Remove(serviceToRemove);
                                                        _etcdClient.Put(handler, services);
                                                    }));

                var removeServiceReference = Task.Run(() =>
                {
                    _etcdClient.Delete(serviceToRemove);
                });

                etcdOperations.AddRange(removeServiceFromHandlers);

            }

            await Task.WhenAll(servicesList);
        }

        private bool IsServiceUp(GrpcChannel channel)
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
    }
}
