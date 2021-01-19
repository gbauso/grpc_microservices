using MassTransit;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using DiscoveryService.Infra.Operations;
using DiscoveryService.Infra.UnitOfWork;

namespace DiscoveryService
{
    public class DiscoveryConsumer : IConsumer<Discovery>
    {
        private readonly IServiceRegisterOperations _serviceRegisteroperations;
        private readonly IServiceRegisterUnitOfWork _serviceRegisterUnitOfWork;
        private readonly ILogger<DiscoveryConsumer> _logger;

        public DiscoveryConsumer(
            IServiceRegisterOperations serviceRegisteroperations,
            IServiceRegisterUnitOfWork serviceRegisterUnitOfWork,
            ILogger<DiscoveryConsumer> logger)
        {
            _serviceRegisteroperations = serviceRegisteroperations;
            _serviceRegisterUnitOfWork = serviceRegisterUnitOfWork;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<Discovery> context)
        {
            var message = context.Message;
            _logger.LogInformation("Message Handling STARTED {message}", message);

            // Try to find if there's some 
            var handlerKeyValue = message.Handlers
                                .Select(i => new { Key = i, Value = _serviceRegisteroperations.GetMethodHandlers(i).Result })
                                .ToList();

            var serviceKeyValue = await _serviceRegisteroperations.GetServiceMethods(message.Service);
            var handlers = serviceKeyValue.Select(i => i.Name);

            var handlersToAdd = message.Handlers.Except(handlers).ToList();
            var handlersToRemove = handlers.Except(message.Handlers).ToList();

            await _serviceRegisterUnitOfWork.BeginTransaction();

            if (handlersToAdd.Any())
            {
                foreach (var handler in handlersToAdd)
                    await _serviceRegisteroperations.AddHandler(handler, message.Service);
            }

            if (handlersToRemove.Any())
            {
                foreach (var handler in handlersToRemove)
                    await _serviceRegisteroperations.RemoveHandler(handler, message.Service);
            }

            await _serviceRegisterUnitOfWork.CommitTransaction();

            _logger.LogInformation("Message Handling FINISHED", message);
        }

    }
}
