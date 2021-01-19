using System;
using System.Linq;
using System.Threading.Tasks;
using DiscoveryService.Infra.Database;
using DiscoveryService.Infra.Operations;
using DiscoveryService.Infra.UnitOfWork;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Xunit;

namespace DiscoveryService.Test
{
    public class DiscoveryConsumerTest
    {
        private readonly IServiceRegisterOperations _repository;
        private readonly IServiceRegisterUnitOfWork _uow;
        private readonly DiscoveryDbContext _discoveryDbContext;

        public DiscoveryConsumerTest()
        {
            _discoveryDbContext = new DiscoveryDbContext(new DbContextOptionsBuilder()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning))
                .Options);

            _repository = new ServiceRegisterOperations(_discoveryDbContext);
            _uow = new ServiceRegisterUnitOfWork(_discoveryDbContext);
        }

        [Theory]
        [InlineData("population", "svc1;svc2")]
        public async Task DiscoveryConsumer_Consume_AddNewService(string service, string handlers)
        {
            var discovery = new Discovery() {
                Service = service,
                Handlers = handlers.Split(";")
            };

            var context = Utils.GetContext(discovery);
            var logger = Utils.GetLogger<DiscoveryConsumer>();

            var consumer = new DiscoveryConsumer(_repository, _uow, logger);
            await consumer.Consume(context);

            var methods = await _repository.GetServiceMethods(service);
            string.Join(';', methods.Select(i => i.Name)).Should().Be(handlers);
        }

        [Theory]
        [InlineData("population", "svc2", "svc2")]
        [InlineData("population", "svc1;svc2", "svc1;svc2")]
        [InlineData("population", "svc3;svc2", "svc3;svc2")]
        public async Task DiscoveryConsumer_Consume_RemoveAndAddService(string service,
                                                                        string handlers,
                                                                        string expected)
        {
            await _repository.AddHandler("svc1", service);

            var discovery = new Discovery() {
                Service = service,
                Handlers = handlers.Split(";")
            };

            var context = Utils.GetContext(discovery);
            var logger = Utils.GetLogger<DiscoveryConsumer>();

            var consumer = new DiscoveryConsumer(_repository, _uow, logger);
            await consumer.Consume(context);

            var methods = await _repository.GetServiceMethods(service);
            string.Join(';', methods.Select(i => i.Name)).Should().Be(expected);
        }
    }
}
