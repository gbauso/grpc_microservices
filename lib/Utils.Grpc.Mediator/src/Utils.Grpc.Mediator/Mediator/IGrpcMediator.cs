using Google.Protobuf;
using System.Threading.Tasks;

namespace Utils.Grpc.Mediator
{
    public interface IGrpcMediator
    {
        Task<Res> Send<Req, Res>(Req request) where Res : IMessage<Res>;
    }
}
