using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using System.Buffers.Text;
using System.Net.Http;
using System.Text.Json;
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
            var apiKey = EnvironmentConfig.GetValue("GoogleMaps", "ApiKey");
            // var apiKey = _config["GoogleMaps:ApiKey"]; // 從 User-Scrtets 中讀取 API Key
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
            var apiKey = EnvironmentConfig.GetValue("GoogleMaps", "ApiKey");
            //string apiKey = _config["GoogleMaps:ApiKey"];
            if (apiKey == null || apiKey == "") {
                return BadRequest("API Key 未設定");
            }
            // callback=initMap 告訴 Google Maps API 載入完成後要執行 window.initMap()。
            string requestUrl = $"https://maps.googleapis.com/maps/api/js?key={apiKey}&callback=initMap&language=zh-TW";

            // _httpClient 是 HttpClient 物件，用來發送 HTTP 請求。
            // GetStringAsync(requestUrl)：發送 GET 請求至 Google Maps API，並取得回應內容（JavaScript 代碼）。
            var response = await _httpClient.GetStringAsync(requestUrl);
            return Content(response, "application/javascript");
        }

        // 根據地點名稱取得照片
        [HttpGet("getPlacePhoto")]
        public async Task<IActionResult> GetPlacePhotoAsync(string query) {
            var apiKey = EnvironmentConfig.GetValue("GoogleMaps", "ApiKey");
            //string apiKey = _config["GoogleMaps:ApiKey"];
            if (string.IsNullOrEmpty(apiKey)) {
                return BadRequest(new { error = "API Key 未設定" });
            }

            string baseUrl = "https://maps.googleapis.com/maps/api/place";

            try {
                // 1. 搜尋景點
                var searchUrl = $"{baseUrl}/textsearch/json?query={query}&key={apiKey}";
                var searchResponse = await _httpClient.GetStringAsync(searchUrl);

                var json = JObject.Parse(searchResponse);
                if (json["status"]?.ToString() != "OK") {
                    return BadRequest(new { error = "Google Places API 回應錯誤", status = json["status"]?.ToString() });
                }

                var firstResult = json["results"]?.First;
                if (firstResult == null) {
                    return NotFound(new { error = "找不到景點" });
                }

                var photoReference = firstResult["photos"]?.First?["photo_reference"]?.ToString();
                if (string.IsNullOrEmpty(photoReference)) {
                    return NotFound(new { error = "此景點沒有照片" });
                }

                // 2. 取得照片 URL
                var photoUrl = $"{baseUrl}/photo?maxwidth=400&photo_reference={photoReference}&key={apiKey}";

                return Ok(new { photoUrl = photoUrl });
            }
            catch (HttpRequestException ex) {
                return StatusCode(500, new { error = "無法連接到 Google Places API", details = ex.Message });
            }
        }

        [HttpPost("locations/geocode")]
        public async Task<IActionResult> GetLocations([FromBody] List<string> places) {
            var apiKey = EnvironmentConfig.GetValue("GoogleMaps", "ApiKey");
            //string apiKey = _config["GoogleMaps:ApiKey"];
            if (string.IsNullOrEmpty(apiKey)) {
                return BadRequest(new { error = "API Key 未設定" });
            }

            if (places == null || places.Count == 0) {
                return BadRequest(new { error = "請提供至少一個景點" });
            }

            var locations = new List<object>();

            foreach (var place in places) {
                string url = $"https://maps.googleapis.com/maps/api/geocode/json?address={Uri.EscapeDataString(place)}&key={apiKey}";

                var response = await _httpClient.GetAsync(url);
                if (!response.IsSuccessStatusCode) {
                    return StatusCode(500, $"Google API 錯誤: {response.ReasonPhrase}");
                }

                var jsonResponse = await response.Content.ReadAsStringAsync();
                var geocodeData = JsonDocument.Parse(jsonResponse);

                var results = geocodeData.RootElement.GetProperty("results");
                if (results.GetArrayLength() == 0) {
                    continue; // 如果沒有找到對應的地點，跳過這個地點
                }

                var location = results[0].GetProperty("geometry").GetProperty("location");
                locations.Add(new {
                    name = place,
                    lat = location.GetProperty("lat").GetDouble(),
                    lng = location.GetProperty("lng").GetDouble()
                });
            }

            if (locations.Count == 0) {
                return NotFound("未找到任何有效的地點");
            }

            return Ok(locations);
        }
    }
}
