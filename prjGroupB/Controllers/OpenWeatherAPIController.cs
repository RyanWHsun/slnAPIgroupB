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

        // 取得天氣圖示
        // GET: api/WeatherIcon
        // https://localhost:7112/api/OpenWeatherAPI/WeatherIcon?lat=25.070843504702268&lon=121.49929878012719
        [HttpGet("WeatherIcon")]
        public async Task<ActionResult<string>> GetWeatherIcon(float lat, float lon) {
            string apiKey = _config["OpenWeather:ApiKey"];
            
            if(apiKey==null || apiKey == "") {
                return BadRequest("API Key 未設定");
            }
            // https://api.openweathermap.org/data/2.5/weather?lat=25.070843504702268&lon=121.49929878012719&appid=11005260cd604454569d8cd051212258
            string requestUrl = $"https://api.openweathermap.org/data/2.5/weather?lat={lat}&lon={lon}&appid={apiKey}";
            HttpResponseMessage response = await _client.GetAsync(requestUrl);

            // 解析 JSON
            string jsonResponse = await response.Content.ReadAsStringAsync();
            using var jsonDoc = JsonDocument.Parse(jsonResponse);
            var root = jsonDoc.RootElement;

            if (!root.TryGetProperty("weather", out var weatherArray) || weatherArray.GetArrayLength() == 0) {
                return BadRequest("找不到天氣資訊");
            }

            string iconCode = weatherArray[0].GetProperty("icon").GetString();
            string iconUrl = $"https://openweathermap.org/img/wn/{iconCode}@2x.png";

            return Ok(iconUrl);
        }
    }
}
