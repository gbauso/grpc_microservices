using System.Collections.Generic;
using dotnet_etcd;
using MassTransit;
using Microsoft.Extensions.Logging;
using Moq;

namespace DiscoveryService.Test
{
    public class Utils
    {
        public static Mock<EtcdClientWrap> GetEtcdClientMock(IDictionary<string, string> values = null) {
            var mock = new Mock<EtcdClientWrap>();
            var etcdStub = values ?? new Dictionary<string, string>();

            mock.Setup(i => i.GetValue(It.IsAny<string>()))
                                .Returns((string key) => 
                                        etcdStub.ContainsKey(key) ? etcdStub[key] : string.Empty );
            
            mock.Setup(i => i.PutValueAsync(It.IsAny<KeyValuePair<string, string>>()))
                .Callback((KeyValuePair<string, string> kv) => { 
                    if(etcdStub.ContainsKey(kv.Key)) etcdStub[kv.Key] = kv.Value;
                    else etcdStub.Add(kv);
                });

            return mock;
        }

        public static ConsumeContext<T> GetContext<T>(T value) where T:class {
            var mock = new Mock<ConsumeContext<T>>();

            mock.Setup(i => i.Message).Returns(value);

            return mock.Object;
        }

        public static ILogger<T> GetLogger<T>()
        {
            return new Mock<ILogger<T>>().Object;
        }
    }
}