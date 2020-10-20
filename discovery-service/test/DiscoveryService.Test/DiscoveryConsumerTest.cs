using System.Linq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using dotnet_etcd;
using FluentAssertions;
using MassTransit;
using MassTransit.Testing;
using Moq;
using Xunit;

namespace DiscoveryService.Test
{
    public class DiscoveryConsumerTest
    {
        private readonly Func<IDictionary<string, string>, EtcdClientWrap> _EtcdClient;

        public DiscoveryConsumerTest()
        {
            _EtcdClient = (IDictionary<string, string> data) => Utils.GetEtcdClientMock(data).Object;
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
            var etcd = _EtcdClient(null);

            var consumer = new DiscoveryConsumer(etcd, logger);
            await consumer.Consume(context);

            etcd.GetValue(service).Should().Be(handlers);
        }

        [Theory]
        [InlineData("population", "svc2", "svc2")]
        [InlineData("population", "svc1;svc2", "svc1;svc2")]
        [InlineData("population", "svc3;svc2", "svc3;svc2")]
        public async Task DiscoveryConsumer_Consume_RemoveAndAddService(string service,
                                                                        string handlers,
                                                                        string expected)
        {
            var existentKv = new Dictionary<string,string>();
            existentKv.Add("population", "svc1");
            existentKv.Add("svc1", "population");

            var discovery = new Discovery() {
                Service = service,
                Handlers = handlers.Split(";")
            };

            var context = Utils.GetContext(discovery);
            var logger = Utils.GetLogger<DiscoveryConsumer>();
            var etcd = _EtcdClient(existentKv);

            var consumer = new DiscoveryConsumer(etcd, logger);
            await consumer.Consume(context);

            etcd.GetValue(service).Should().Be(handlers);
        }
    }
}
