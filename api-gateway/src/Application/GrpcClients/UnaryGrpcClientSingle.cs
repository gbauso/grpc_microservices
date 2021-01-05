using System;
using System.Linq;
using System.Threading.Tasks;
using Application.Factory;
using Grpc.Core;
using Grpc.Core.Interceptors;
using Grpc.Core.Logging;
using Microsoft.Extensions.Logging;

namespace Application.GrpcClients
{
    public class UnaryGrpcClientSingle : GrpcClientBase
    {
        private readonly ChannelFactory _channelFactory;

        public UnaryGrpcClientSingle(ClientFactory clientFactory,
            ChannelFactory channelFactory,
            Operation operation,
            ILogger<UnaryGrpcClientSingle> logger) :
            base(clientFactory, operation, logger)
        {
            _channelFactory = channelFactory;
        }

        public override async Task<TRes> ExecuteAndMerge<TReq, TRes>(TReq request)
        {
            var client = _clientFactory.GetClientInfo(typeof(TRes));
            var channels = await _channelFactory.GetChannels(client.service);
            
            var execution = channels.Select(ch =>
                {
                    var call = (AsyncUnaryCall<TRes>) CallGrpc<TReq, TRes>(client.client, request, ch, client.service);
                    return call.ResponseAsync;
                }
            );

            return MergeAll(await Task.WhenAll(execution));
        }

    }
}