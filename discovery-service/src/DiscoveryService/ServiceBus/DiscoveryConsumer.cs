using dotnet_etcd;
using MassTransit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DiscoveryService.Extensions;
using Microsoft.Extensions.Logging;

namespace DiscoveryService
{
    public class DiscoveryConsumer : IConsumer<Discovery>
    {
        private readonly EtcdClientWrap _EtcdClient;
        private readonly ILogger<DiscoveryConsumer> _logger;

        public DiscoveryConsumer(EtcdClientWrap etcdClient, ILogger<DiscoveryConsumer> logger)
        {
            _EtcdClient = etcdClient;
            _logger = logger;
        }



        public async Task Consume(ConsumeContext<Discovery> context)
        {
            var message = context.Message;
            _logger.LogInformation("Message Handling STARTED {message}", message);
            
            // Try to find if there's some 
            var handlerKeyValue = message.Handlers
                                .Select(i => new { Key = i, Value = _EtcdClient.GetValue(i).SplitIfNotEmpty() })
                                .ToList();

            var serviceKeyValue = _EtcdClient.GetValue(message.Service).SplitIfNotEmpty().ToList();

            var handlersToAdd = message.Handlers.Except(serviceKeyValue).ToList();
            var handlersToRemove = serviceKeyValue.Except(message.Handlers).ToList();

            if (handlersToAdd.Any() || handlersToRemove.Any())
            {
                serviceKeyValue.AddRange(handlersToAdd);
                serviceKeyValue.RemoveAll(i => handlersToRemove.Contains(i));

                var tasks = new List<Task>
                {
                    Task.Run(() => _EtcdClient.PutValueAsync(GetKeyValuePair(message.Service, serviceKeyValue)))
                };

                tasks.AddRange(handlerKeyValue
                                    .FindAll(i => handlersToAdd.Contains(i.Key))
                                        .Select(i => Task.Run(() =>
                                            {
                                                i.Value.Add(message.Service);
                                                _EtcdClient.PutValueAsync(GetKeyValuePair(i.Key, i.Value));
                                            }
                                        )
                                    )
                               );

                tasks.AddRange(handlerKeyValue
                                    .FindAll(i => handlersToRemove.Contains(i.Key))
                                        .Select(i => Task.Run(() =>
                                            {
                                                i.Value.Remove(message.Service);
                                                _EtcdClient.PutValueAsync(GetKeyValuePair(i.Key, i.Value));
                                            }
                                        )
                                    )
                               );

                

                await Task.WhenAll(tasks);
                
                _logger.LogInformation("Message Handling FINISHED", message);
            }

            var servicesKeyValue = _EtcdClient.GetValue("Services").SplitIfNotEmpty().ToList();

            if (!servicesKeyValue.Contains(message.Service))
            {
                servicesKeyValue.Add(message.Service);
                await _EtcdClient.PutValueAsync(GetKeyValuePair("Services", servicesKeyValue));
            }

        }

        private KeyValuePair<string, string> GetKeyValuePair(string key, ICollection<string> value)
        {
            return new KeyValuePair<string, string>(key, string.Join(";", value));
        }
    }
}
