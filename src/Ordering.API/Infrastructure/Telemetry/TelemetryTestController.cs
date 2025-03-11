using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace eShop.Ordering.API.Controllers
{
    [ApiController]
    [Route("api/telemetry-test")]
    public class TelemetryTestController : ControllerBase
    {
        private readonly ILogger<TelemetryTestController> _logger;

        public TelemetryTestController(ILogger<TelemetryTestController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        public IActionResult TestSensitiveData()
        {
            _logger.LogInformation("Processing user: test@example.com with card: 1234-5678-9012-3456");

            var activity = Activity.Current;
            activity?.SetTag("email", "test@example.com");
            activity?.SetTag("payment_method", "Visa ending in 3456");

            return Ok("Check logs and traces in Jaeger/Grafana");
        }
    }
}
