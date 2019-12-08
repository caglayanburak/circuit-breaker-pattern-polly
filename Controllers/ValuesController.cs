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
        private readonly CircuitBreakerPolicy<PingResponse> _breakerPolicy;
        private readonly HttpClient _httpClient;
        ElasticClient client;
        public ValuesController(CircuitBreakerPolicy<PingResponse> breakerPolicy)
        {
            _breakerPolicy = breakerPolicy;
            var configuration = new ConnectionSettings(new Uri("http://localhost:9200")).DefaultIndex("product");
            client = new ElasticClient(configuration);
        }
        // GET api/values
        [HttpGet]
        public IActionResult Get()
        {
            var inventoryResponse = _breakerPolicy.Execute(() => client.Ping());
            System.Console.WriteLine($"Elastic status: {inventoryResponse.IsValid}");

            return StatusCode((inventoryResponse.IsValid ? (int)HttpStatusCode.OK : (int)HttpStatusCode.InternalServerError), "hata var");
            //return new string[] { "value1", "value2" };
        }

        // GET api/values/5
        [HttpGet("{id}")]
        public ActionResult<string> Get(int id)
        {
            return "value";
        }

        // POST api/values
        [HttpPost]
        public void Post([FromBody] string value)
        {
        }

        // PUT api/values/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/values/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
