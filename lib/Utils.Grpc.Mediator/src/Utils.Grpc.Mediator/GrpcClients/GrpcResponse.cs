using System;

namespace Utils.Grpc.Mediator.GrpcClients
{
    public class GrpcResponse<Res>
    {
        public bool PartialContent { get; private set; }
        public Res Content { get; private set; }

        public static GrpcResponse<Res> CreateResponse(int channelCount, int responseCount, Res response) =>
            new GrpcResponse<Res>
            {
                Content = response,
                PartialContent = channelCount != responseCount
            };
    }
}
