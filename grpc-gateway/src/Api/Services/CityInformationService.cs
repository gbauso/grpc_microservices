using Cityinformation;
using Grpc.Core;
using System.Linq;
using System.Threading.Tasks;
using Utils.Grpc.Factory;

namespace GrpcComposition.Services
{
    public class CityInformationService : CityService.CityServiceBase
    {
        private readonly IChannelFactory _channelFactory;

        public CityInformationService(IChannelFactory channelFactory)
        {
            _channelFactory = channelFactory;
        }

        public override async Task<SearchResponse> GetCityInformation(SearchRequest request, ServerCallContext context)
        {
            var channels = await _channelFactory.GetChannels(CityService.Descriptor.FullName);

            var tasks = channels.Select(async channel =>
            {
                var client = new CityService.CityServiceClient(channel);

                return await client.GetCityInformationAsync(request);
            });

            var results = await Task.WhenAll(tasks);
            var result = new SearchResponse();

            foreach (var res in results)
            {
                result.MergeFrom(res);
            }

            return result;
        }
    }
}
