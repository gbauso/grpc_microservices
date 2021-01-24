﻿using System.Threading.Tasks;
using Cityinformation;
using Microsoft.AspNetCore.Mvc;
using Utils.Grpc.GrpcClients;

namespace Api.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    public class CityController : ControllerBase
    {
        private readonly IGrpcClient _client;

        public CityController(IGrpcClient client)
        {
            _client = client;
        }

        // GET: api/City
        [HttpGet]
        public async Task<IActionResult> Get([FromQuery] SearchRequest request)
        {
            var response = _client.ExecuteAndMerge<SearchRequest, SearchResponse>(request);

            return Ok(await response);
        }

    }
}
