using System;
using System.Linq;
using System.Threading.Tasks;
using Utils.Grpc.Factory;
using Google.Protobuf;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using Utils.Grpc.Extensions;

namespace Utils.Grpc.GrpcClients
{
    internal abstract class GrpcClientBase : IGrpcClient
    {
        protected readonly ClientFactory _clientFactory;
        private readonly Operation _operation;
        protected ILogger<GrpcClientBase> Logger;

        private const int TIMEOUT = 30;

        protected GrpcClientBase()
        {

        }
        protected GrpcClientBase(ClientFactory clientFactory,
                                 Operation operation,
                                 ILogger<GrpcClientBase> logger)
        {
            _clientFactory = clientFactory;
            _operation = operation;
            Logger = logger;
        }

        protected object CallGrpc<Req, Res>(Type clientType, Req request, Channel channel, string service)
        {
            var client = _clientFactory.GetInstance(TypeChannelPair.Create(channel, clientType));
            var method = client.GetType()
                .GetCallableMethods()
                .GetMethodByResponse(typeof(Res));

            Logger.LogInformation("Calling Channel {Channel} for {Service}", channel.ResolvedTarget, service);

            return method?.Invoke(client, new object[] {request, GetCallContext(service, method.Name, channel.ResolvedTarget) });
        }

        protected Res MergeAll<Res>(Res[] responseList) where Res : IMessage<Res>
        {
            var response = Activator.CreateInstance<Res>();

            foreach (var res in responseList.Where(x => x != null))
            {
                response.MergeFrom(res);
            }

            return response;
        }

        private CallOptions GetCallContext(string service, string methodName, string target)
        {
            var headers = new Metadata
            {
                {"service", service},
                {"rpc", methodName},
                {"operation_id", _operation.OperationId.ToString()},
                {"target", target}
            };

            return new CallOptions(headers, DateTime.UtcNow.AddSeconds(TIMEOUT));
        }


        public abstract Task<Res> ExecuteAndMerge<Req, Res>(Req request) where Res : IMessage<Res>;
    }
}