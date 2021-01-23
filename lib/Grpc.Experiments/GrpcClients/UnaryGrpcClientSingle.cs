using System.Linq;
using System.Threading.Tasks;
using Grpc.Experiments.Factory;
using Grpc.Core;
using Microsoft.Extensions.Logging;

namespace Grpc.Experiments.GrpcClients
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
            var channels = await _channelFactory.GetChannels(client.ServiceName);
            
            var execution = channels.Select(channel =>
                {
                    var call = (AsyncUnaryCall<TRes>) CallGrpc<TReq, TRes>(client.ServiceType, request, channel, client.ServiceName);
                    return call.ResponseAsync;
                }
            );

            return MergeAll(await Task.WhenAll(execution));
        }

    }
}