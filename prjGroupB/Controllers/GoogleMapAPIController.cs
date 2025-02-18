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

        // 取得地圖資料
        [HttpGet("getMapData")]
        public async Task<IActionResult> GetMapData() {
            string apiKey = _config["GoogleMaps:ApiKey"];
            if (apiKey == null || apiKey=="") {
                return BadRequest("API Key 未設定");
            }
            // callback=initMap 告訴 Google Maps API 載入完成後要執行 window.initMap()。
            string requestUrl = $"https://maps.googleapis.com/maps/api/js?key={apiKey}&callback=initMap&language=zh-TW";

            // _httpClient 是 HttpClient 物件，用來發送 HTTP 請求。
            // GetStringAsync(requestUrl)：發送 GET 請求至 Google Maps API，並取得回應內容（JavaScript 代碼）。
            var response = await _httpClient.GetStringAsync(requestUrl);
            return Content(response, "application/javascript");
        }
    }
}
