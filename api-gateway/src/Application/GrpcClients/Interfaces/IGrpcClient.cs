using System.Threading.Tasks;
using Google.Protobuf;

namespace Application.GrpcClients
{
    public interface IGrpcClient 
    {
        Task<Res> ExecuteAndMerge<Req, Res>(Req request) where Res : IMessage<Res>;
    }
}