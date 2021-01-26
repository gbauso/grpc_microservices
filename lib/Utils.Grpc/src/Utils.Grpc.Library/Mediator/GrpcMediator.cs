using Utils.Grpc.Factory;
using Utils.Grpc.Extensions;
using System;
using Grpc.Core.Utils;
using Grpc.Core;
using Microsoft.Extensions.DependencyInjection;
using Utils.Grpc.GrpcClients;
using Utils.Grpc.Exceptions;
using System.Threading.Tasks;
using Google.Protobuf;

namespace Utils.Grpc.Mediator
{
    internal class GrpcMediator : IGrpcMediator
    {
        private readonly IServiceProvider _serviceProvider;

        public GrpcMediator(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public Task<Res> Send<Req, Res>(Req request) where Res : IMessage<Res>
        {
            using(var scope = _serviceProvider.CreateScope())
            {
                var clientFactory = scope.ServiceProvider.GetRequiredService<ClientFactory>();

                var grpcClient = clientFactory.GetClientInfo(typeof(Res));

                var grpcMethodType = grpcClient.ServiceType.GetMethodType<Req, Res>();

                IGrpcClient client = grpcMethodType switch
                {
                    MethodType.Unary => scope.ServiceProvider.GetRequiredService<UnaryGrpcClientSingle>(),
                    _ => throw new ClientNotFoundException()
                };

                return client.ExecuteAndMerge<Req, Res>(request);
            }
        }
    }
}
