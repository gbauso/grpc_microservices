using MassTransit;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using DiscoveryService.Infra.Operations;
using System;

namespace DiscoveryService
{
    public class DiscoveryConsumer : IConsumer<Discovery>
    {
        private readonly Func<IServiceRegisterOperations> _serviceRegisteroperations;
        private readonly ILogger<DiscoveryConsumer> _logger;

        public DiscoveryConsumer(
            Func<IServiceRegisterOperations> serviceRegisteroperations,
            ILogger<DiscoveryConsumer> logger)
        {
            _serviceRegisteroperations = serviceRegisteroperations;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<Discovery> context)
        {
            using(var operations = _serviceRegisteroperations())
            {
                var message = context.Message;
                _logger.LogInformation("Message Handling STARTED {message}", message);

                // Try to find if there's some 
                var handlerKeyValue = message.Handlers
                                    .Select(i => new { Key = i, Value = operations.GetMethodHandlers(i).Result })
                                    .ToList();

                var serviceKeyValue = await operations.GetServiceMethods(message.Service);
                var handlers = serviceKeyValue.Select(i => i.Name);

                var handlersToAdd = message.Handlers.Except(handlers).ToList();
                var handlersToRemove = handlers.Except(message.Handlers).ToList();


                if (handlersToAdd.Any())
                {
                    foreach (var handler in handlersToAdd)
                        await operations.AddHandler(handler, message.Service);
                }

                if (handlersToRemove.Any())
                {
                    foreach (var handler in handlersToRemove)
                        await operations.RemoveHandler(handler, message.Service);
                }


                _logger.LogInformation("Message Handling FINISHED", message);

            }
        }

    }
}
