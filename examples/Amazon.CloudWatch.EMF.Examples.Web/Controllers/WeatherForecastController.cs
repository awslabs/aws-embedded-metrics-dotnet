using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Amazon.CloudWatch.EMF.Logger;
using Amazon.CloudWatch.EMF.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Amazon.CloudWatch.EMF.Web.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        private readonly ILogger<WeatherForecastController> _logger;
        private readonly IMetricsLogger _metrics;

        public WeatherForecastController(
            ILogger<WeatherForecastController> logger,
            IMetricsLogger metrics)
        {
            _logger = logger;
            _metrics = metrics;
        }

        [HttpGet]
        public IEnumerable<WeatherForecast> Get()
        {
            Thread.Sleep(100);
            var rng = new Random();
            var temperature = rng.Next(-20, 55);
            var WindSpeed = rng.Next(10, 100);

            _metrics.PutMetric("Temperature", temperature, Unit.NONE);
            _metrics.PutMetric("WindSpeed", WindSpeed, Unit.NONE, StorageResolution.HIGH);

            return Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateTime.Now.AddDays(index),
                TemperatureC = temperature,
                Summary = Summaries[rng.Next(Summaries.Length)]
            })
            .ToArray();
        }
    }
}
