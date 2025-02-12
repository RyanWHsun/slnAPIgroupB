using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Net.Http;
using System.Threading.Tasks;

namespace prjGroupB.Controllers {
    [Route("api/maps")]
    [ApiController]
    public class MapsController : ControllerBase {
        private readonly IConfiguration _config;
        private readonly HttpClient _httpClient;

        public MapsController(IConfiguration config, HttpClient httpClient) {
            _config = config;
            _httpClient = httpClient;
        }

        [HttpGet("geocode")]
        public async Task<IActionResult> Geocode([FromQuery] string address) {
            var apiKey = _config["GoogleMaps:ApiKey"];
            if (string.IsNullOrEmpty(apiKey)) {
                return BadRequest("API Key 未設定");
            }

            var googleApiUrl = $"https://maps.googleapis.com/maps/api/geocode/json?address={address}&key={apiKey}";
            var response = await _httpClient.GetStringAsync(googleApiUrl);

            return Content(response, "application/json");
        }
    }
}
