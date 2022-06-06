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
            var serviceName = CityService.Descriptor.FullName;
            var channels = await _channelFactory.GetChannels(serviceName);
            var correlationId = Guid.NewGuid().ToString();

            _logger.LogInformation("Request to {method} STARTED with payload: {request}. CorrelationId: {correlationId}", 
                context.Method, request, correlationId);

            var tasks = channels.Select(async channel =>
            {
                try
                {
                    var client = new CityService.CityServiceClient(channel);

                    var callOptions = client.GetCallOptions(correlationId, context.Method, channel.ResolvedTarget);

                    _logger.LogInformation("Call to Channel {channel} for {method} STARTED. CorrelationId: {correlationId}", 
                        channel.ResolvedTarget, context.Method, correlationId);
                    
                    return await client.GetCityInformationAsync(request, callOptions).CallWithRetry();

                }
                catch(Exception ex)
                {
                    _logger.LogError(ex, "Call to Channel {channel} for {method} FAILED. CorrelationId: {correlationId}",
                        channel.ResolvedTarget, context.Method, correlationId);
                    return new SearchResponse();
                }

            });

            var mergedResult = await tasks.MergeAllResults();

            _logger.LogInformation("Request to {Method} FINISHED with payload: {mergedResult}. CorrelationId: {correlationId}",
                context.Method, mergedResult, correlationId);

            return mergedResult;
        }
    }
}
