using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nest;
using Polly;
using Polly.CircuitBreaker;
using Polly.Retry;

namespace Ddd_UoW_Sample.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ValuesController : ControllerBase
    {
        ElasticClient client;
        private CircuitBreakerPolicy<PingResponse> _circuitBreakerPolicy;
        public ValuesController(CircuitBreakerPolicy<PingResponse> policy)
        {
            var configuration = new ConnectionSettings(new Uri("http://localhost:9200")).DefaultIndex("product");
            client = new ElasticClient(configuration);
            _circuitBreakerPolicy = policy;
        }


        // GET api/values
        [HttpGet]
        public IActionResult Get()
        {
            EnsureAvailability();
            var inventoryResponse = client.Ping();


            return StatusCode((inventoryResponse.IsValid ? (int)HttpStatusCode.OK : (int)HttpStatusCode.InternalServerError), inventoryResponse.IsValid ? "sıkıntı yok" : "hata var");
        }

        public void EnsureAvailability()
        {
            var response = _circuitBreakerPolicy.Execute(() => client.Ping());
            System.Console.WriteLine($"Elastic status: {_circuitBreakerPolicy.CircuitState}");
        }
    }
}
