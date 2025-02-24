using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Text.Json;
using static System.Net.WebRequestMethods;

namespace prjGroupB.Controllers {
    [Route("api/[controller]")]
    [ApiController]
    public class OpenWeatherAPIController : ControllerBase {
        private readonly IConfiguration _config;
        private readonly HttpClient _client;

        public OpenWeatherAPIController(IConfiguration config, HttpClient client) {
            _config = config;
            _client = client;
        }

        // Call current weather data
        [HttpGet("CurrentWeather")]
        public async Task<ActionResult<string>> GetCurrentWeather(float lat, float lon) {
            string apiKey = _config["OpenWeather:ApiKey"];

            if (apiKey == null || apiKey == "") {
                return BadRequest("API Key 未設定");
            }

            string requestUrl = $"https://api.openweathermap.org/data/2.5/weather?lat={lat}&lon={lon}&appid={apiKey}";

            // 使用 HttpClient 來發送 HTTP GET 請求。
            // GetAsync() 會回傳 HttpResponseMessage 物件，其中包含 HTTP 回應的 狀態碼、標頭和內容。
            HttpResponseMessage response = await _client.GetAsync(requestUrl);

            // 解析 JSON
            string jsonResponse = await response.Content.ReadAsStringAsync(); // response.Content.ReadAsStringAsync() 會將 HTTP 回應的 Body 內容 讀取為 字串。

            return Ok(jsonResponse);
        }

        // 取得天氣圖示
        // GET: api/WeatherIcon
        // https://localhost:7112/api/OpenWeatherAPI/WeatherIcon?lat=25.070843504702268&lon=121.49929878012719
        [HttpGet("WeatherIcon")]
        public async Task<ActionResult<string>> GetWeatherIcon(float lat, float lon) {
            string apiKey = _config["OpenWeather:ApiKey"];

            if (apiKey == null || apiKey == "") {
                return BadRequest("API Key 未設定");
            }
            // https://api.openweathermap.org/data/2.5/weather?lat=25.070843504702268&lon=121.49929878012719&appid=11005260cd604454569d8cd051212258
            string requestUrl = $"https://api.openweathermap.org/data/2.5/weather?lat={lat}&lon={lon}&appid={apiKey}";
            HttpResponseMessage response = await _client.GetAsync(requestUrl);

            // 解析 JSON
            string jsonResponse = await response.Content.ReadAsStringAsync();

            // 為什麼要用 using var？
            // JsonDocument 是 IDisposable，表示它使用 非託管資源（如記憶體緩衝區）。
            // 使用 using var 會自動釋放資源，避免記憶體洩漏。
            using var jsonDoc = JsonDocument.Parse(jsonResponse); // JsonDocument.Parse(jsonResponse) 會將 JSON 字串解析為 JSON 物件。
            var root = jsonDoc.RootElement; // 取得 JSON 的根元素

            // TryGetProperty() 會嘗試從 root 取出 "weather" 屬性。
            if (!root.TryGetProperty("weather", out var weatherArray) || weatherArray.GetArrayLength() == 0) {
                return BadRequest("找不到天氣資訊");
            }

            string iconCode = weatherArray[0].GetProperty("icon").GetString();
            string iconUrl = $"https://openweathermap.org/img/wn/{iconCode}@2x.png";

            return Ok(iconUrl);
        }
    }
}
