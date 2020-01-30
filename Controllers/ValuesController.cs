using System;
using System.Net;
using circuit_breaker_pattern_polly.Models;
using Microsoft.AspNetCore.Mvc;
using Nest;
using Polly.CircuitBreaker;

namespace Ddd_UoW_Sample.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ValuesController : ControllerBase
    {
        ElasticClient client;
        private CircuitBreakerPolicy<ISearchResponse<InventoryItem>> _circuitBreakerPolicy;
        public ValuesController(CircuitBreakerPolicy<ISearchResponse<InventoryItem>> policy)
        {
            var configuration = new ConnectionSettings(new Uri("http://localhost:9200")).DefaultIndex("product");
            client = new ElasticClient(configuration);
            _circuitBreakerPolicy = policy;
        }

        // GET api/values
        [HttpGet]
        public IActionResult Get()
        {
            // if (_circuitBreakerPolicy.CircuitState == CircuitState.Open)
            // {
            //     //todo:
            // }

            var inventoryResponse = Search();
            return StatusCode((inventoryResponse.IsValid ? (int)HttpStatusCode.OK : (int)HttpStatusCode.InternalServerError), inventoryResponse.IsValid ? "sıkıntı yok" : "hata var");
        }

        public ISearchResponse<InventoryItem> Search()
        {
            var response = _circuitBreakerPolicy.Execute(() => client.Search<InventoryItem>(s => s
                  .Index("index_name")
                  .Query(q => q
                      .Match(m => m
                          .Field(field => field.Code.Suffix("keyword"))
                          .Query("test")))));
            System.Console.WriteLine($"Elastic status: {_circuitBreakerPolicy.CircuitState}");
            return response;
        }
    }
}
