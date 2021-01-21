using MassTransit;
using Microsoft.Extensions.Logging;
using Moq;

namespace DiscoveryService.Test
{
    public class Utils
    {
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