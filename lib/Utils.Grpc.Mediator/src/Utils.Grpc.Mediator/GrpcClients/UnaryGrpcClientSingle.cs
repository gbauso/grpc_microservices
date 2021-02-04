using System.Linq;
using System.Threading.Tasks;
using Utils.Grpc.Mediator.Factory;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using System;
using Polly.Retry;
using System.Threading;

namespace Utils.Grpc.Mediator.GrpcClients
{
    internal class UnaryGrpcClientSingle : GrpcClientBase
    {
        private readonly ChannelFactory _channelFactory;

        public UnaryGrpcClientSingle() : base()
        {
            // For Testing Purposes
        }

        public UnaryGrpcClientSingle(ClientFactory clientFactory,
            ChannelFactory channelFactory,
            Operation operation,
            GrpcRetryPolicy grpcRetryPolicy,
            ILogger<UnaryGrpcClientSingle> logger) :
            base(clientFactory, operation, grpcRetryPolicy, logger)
        {
            _channelFactory = channelFactory;
        }

        public override async Task<GrpcResponse<TRes>> ExecuteAndMerge<TReq, TRes>(TReq request)
        {
            var client = _clientFactory.GetClientInfo(typeof(TRes));
            var channels = await _channelFactory.GetChannels(client.ServiceName);
            
            var execution = channels.Select(channel =>
                {
                    try
                    {
                        var call = (AsyncUnaryCall<TRes>) CallGrpc<TReq, TRes>(client.ServiceType, request, channel, client.ServiceName);
                        return call.ResponseAsync;
                    }
                    catch
                    {
                        return Task.FromResult(default(TRes));
                    }
                }
            );

            var responses = await Task.WhenAll(execution)
                                .ContinueWith(cw => cw.Result.Where((res) => res != null ));

            return GrpcResponse<TRes>.CreateResponse(MergeAll(responses));
        }

    }
}