using System;
using System.Collections.Generic;
using System.Text;
using DiscoveryService.Extensions;
using System.Linq;
using DiscoveryService.Factory;
using Healthcheck;
using Grpc.Net.Client;
using System.Threading.Tasks;

namespace DiscoveryService.HealthCheck
{
    public class HealthChecker
    {
        private readonly EtcdClientWrap _etcdClient;
        private readonly ChannelFactory _channelFactory;
        private const string SERVICES_KEY = "Services";

        public HealthChecker(EtcdClientWrap etcdClient, ChannelFactory channelFactory)
        {
            _etcdClient = etcdClient;
            _channelFactory = channelFactory;
        }

        public void Handle(object state)
        {
            // Get all services registered on ETCD
            var servicesRegistered = _etcdClient
                                        .GetValue(SERVICES_KEY)
                                        .SplitIfNotEmpty()
                                        .ToList();

            // Check if the services are UP
            var serviceCheckTasks = servicesRegistered.Select(service => Task.Run(() =>
            {
                return IsServiceUp(_channelFactory.GetChannel(service)) ? string.Empty : service;
            }));

            // Get services to remove
            var servicesToRemove = Task.WhenAll(serviceCheckTasks).Result.Where(i => !string.IsNullOrEmpty(i));

            // Get new existent services (excluding ones aren't up)
            var existentServices = servicesRegistered.Except(servicesToRemove);
            List<Task> etcdOperations = new List<Task>();
            etcdOperations.Add(Task.Run(() => _etcdClient.PutValueAsync(GetKeyValuePair(SERVICES_KEY, existentServices))));

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
                                                    .GetValue(serviceToRemove)
                                                    .SplitIfNotEmpty()
                                                    .Select(handler => Task.Run(() =>
                                                    {
                                                        var services = _etcdClient.GetValue(handler).SplitIfNotEmpty();
                                                        services.Remove(serviceToRemove);
                                                        _etcdClient.PutValueAsync(GetKeyValuePair(handler, services));
                                                    }));

                var removeServiceReference = Task.Run(() =>
                {
                    _etcdClient.Delete(serviceToRemove);
                });

                etcdOperations.Add(removeServiceReference);
                etcdOperations.AddRange(removeServiceFromHandlers);

            }

            Task.WhenAll(etcdOperations).Wait();
        }

        private bool IsServiceUp(GrpcChannel channel)
        {
            var client = new Healthcheck.HealthCheckService.HealthCheckServiceClient(channel);

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

        private KeyValuePair<string, string> GetKeyValuePair(string key, IEnumerable<string> value)
        {
            return new KeyValuePair<string, string>(key, string.Join(";", value));
        }
    }
}
