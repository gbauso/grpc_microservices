using Google.Protobuf;
using System.Threading.Tasks;
using Utils.Grpc.Mediator.GrpcClients;

namespace Utils.Grpc.Mediator
{
    public interface IGrpcMediator
    {
        Task<GrpcResponse<Res>> Send<Req, Res>(Req request) where Res : IMessage<Res>;
    }
}
