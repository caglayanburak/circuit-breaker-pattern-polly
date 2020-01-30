using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using circuit_breaker_pattern_polly.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Nest;
using Polly;
using Polly.CircuitBreaker;
using Polly.Retry;

namespace Ddd_UoW_Sample
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            CircuitBreakerPolicy<ISearchResponse<InventoryItem>> breakerPolicy = Polly.Policy
            .HandleResult<ISearchResponse<InventoryItem>>(r => !r.IsValid)
            .CircuitBreaker(2, TimeSpan.FromSeconds(5), OnBreak, OnReset, onHalfOpen: OnHalfOpen);

            services.AddSingleton<CircuitBreakerPolicy<ISearchResponse<InventoryItem>>>(breakerPolicy);

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                // app.UseHsts();
            }

            // app.UseHttpsRedirection();
            app.UseMvc();
        }

        private void OnHalfOpen()
        {
            Console.WriteLine("\t\t\t\t\tConnection half open");
        }

        private void OnReset(Polly.Context context)
        {
            Console.WriteLine("\t\t\t\t\tConnection reset");
        }

        private void OnBreak(DelegateResult<ISearchResponse<InventoryItem>> delegateResult, TimeSpan timeSpan, Polly.Context context)
        {
            Console.WriteLine($"\t\t\t\t\tConnection break: {delegateResult.Result.IsValid}");
        }
    }
}
