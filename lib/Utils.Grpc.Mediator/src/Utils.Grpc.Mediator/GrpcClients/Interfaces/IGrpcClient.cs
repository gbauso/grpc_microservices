using System.Threading.Tasks;
using Google.Protobuf;

namespace Utils.Grpc.Mediator.GrpcClients
{
    internal interface IGrpcClient 
    {
        Task<Res> ExecuteAndMerge<Req, Res>(Req request) where Res : IMessage<Res>;
    }
}