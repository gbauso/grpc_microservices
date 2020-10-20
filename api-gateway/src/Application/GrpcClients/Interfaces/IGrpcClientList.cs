using System.Collections.Generic;
using System.Threading.Tasks;
using Google.Protobuf;

namespace Application.GrpcClients
{
    public interface IGrpcClientList
    {
        Task<ICollection<Res>> Execute<Req, Res>(string service, Req request) where Res : IMessage<Res>;
    }
}