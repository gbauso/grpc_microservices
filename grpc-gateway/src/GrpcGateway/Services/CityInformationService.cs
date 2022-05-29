using Cityinformation;
using Grpc.Core;
using Utils.Grpc.Extensions;
using Utils.Grpc.Factory;

namespace GrpcGateway.Services
{
    public class CityInformationService : CityService.CityServiceBase
    {
        private readonly IChannelFactory _channelFactory;
        private readonly ILogger<CityInformationService> _logger;

        public CityInformationService(IChannelFactory channelFactory, ILogger<CityInformationService> logger)
        {
            _channelFactory = channelFactory;
            _logger = logger;
        }

        public override async Task<SearchResponse> GetCityInformation(SearchRequest request, ServerCallContext context)
        {
            _logger.LogInformation("Logging", request);
            var channels = await _channelFactory.GetChannels(CityService.Descriptor.FullName);

            var tasks = channels.Select(async channel =>
            {
                var client = new CityService.CityServiceClient(channel);

                return await client.GetCityInformationAsync(request).CallWithRetry();
            });

            return await tasks.MergeAllResults();
        }
    }
}
