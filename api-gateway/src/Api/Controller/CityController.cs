using System.Threading.Tasks;
using Cityinformation;
using Microsoft.AspNetCore.Mvc;
using Utils.Grpc.Mediator;

namespace Api.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    public class CityController : ControllerBase
    {
        private readonly IGrpcMediator _mediator;

        public CityController(IGrpcMediator mediator)
        {
            _mediator = mediator;
        }

        // GET: api/City
        [HttpGet]
        public async Task<IActionResult> Get([FromQuery] SearchRequest request)
        {
            var response = _mediator.Send<SearchRequest, SearchResponse>(request);

            return Ok(await response);
        }

    }
}
