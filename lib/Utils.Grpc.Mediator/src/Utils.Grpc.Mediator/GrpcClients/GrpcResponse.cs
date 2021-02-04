using Google.Protobuf;
using System;
using System.Linq;
using System.Reflection;
using Utils.Grpc.Mediator.Extensions;

namespace Utils.Grpc.Mediator.GrpcClients
{
    public class GrpcResponse<Res> where Res : IMessage<Res>
    {
        public bool PartialContent { get; private set; }
        public Res Content { get; private set; }

        public static GrpcResponse<Res> CreateResponse(Res response, bool? isPartial = null) =>
            new GrpcResponse<Res>
            {
                Content = response,
                PartialContent = isPartial ?? response.HasFullContent()
            };
    }
}
