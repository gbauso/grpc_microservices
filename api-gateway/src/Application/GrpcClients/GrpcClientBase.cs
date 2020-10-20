using System;
using System.Linq;
using System.Threading.Tasks;
using Application.Factory;
using Google.Protobuf;
using Grpc.Core;
using Microsoft.Extensions.Logging;

namespace Application.GrpcClients
{
    public abstract class GrpcClientBase : IGrpcClient
    {
        protected readonly ClientFactory _clientFactory;
        private readonly Operation _operation;
        protected ILogger<GrpcClientBase> Logger;

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
            var client = _clientFactory.GetInstance(clientType, channel);
            var methods = client.GetType()
                .GetMethods()
                .Where(i => i.Name.EndsWith("Async"));

            var method = methods.FirstOrDefault(i =>
                i.GetParameters().Length == 2 &&
                (i.ReturnType.FullName?.Contains(typeof(Res).Name) ?? false));

            Logger.LogInformation("Calling Channel {Channel} for {Service}", channel.ResolvedTarget, service);

            return method?.Invoke(client, new object[] {request, GetCallContext(service, method.Name)});
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

        private CallOptions GetCallContext(string service, string methodName)
        {
            var context = new Metadata
            {
                {"service", service},
                {"rpc", methodName},
                {"operation_id", _operation.OperationId.ToString()}
            };

            return new CallOptions(context);
        }


        public abstract Task<Res> ExecuteAndMerge<Req, Res>(Req request) where Res : IMessage<Res>;
    }
}