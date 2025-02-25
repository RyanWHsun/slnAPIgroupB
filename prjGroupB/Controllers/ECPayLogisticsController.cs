using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using prjGroupB.DTO;
using System.Text.RegularExpressions;
using static System.Net.WebRequestMethods;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace prjGroupB.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ECPayLogisticsController : ControllerBase
    {
        private readonly HttpClient _httpClient;

        public ECPayLogisticsController(HttpClient httpClient) 
        { 
            _httpClient = httpClient;
        }


        [HttpPost("openStoreMap")]
        public async Task<IActionResult> OpenStoreMap([FromServices] IOptions<ECPaySettings> ecPaySettings)
        {
            var settings = ecPaySettings.Value;
            var url = "https://logistics-stage.ecpay.com.tw/Express/map";

            if (string.IsNullOrEmpty(settings.HashKey) || string.IsNullOrEmpty(settings.HashIV))
            {
                return BadRequest("ECPay 金鑰 (HashKey / HashIV) 未設定");
            }

            using var httpClient = new HttpClient();
            var parameters = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("MerchantID",settings.MerchantID),
                new KeyValuePair<string, string>("MerchantTradeNo", Guid.NewGuid().ToString("N")),
                new KeyValuePair<string, string>("LogisticsType", "CVS"),
                new KeyValuePair<string, string>("LogisticsSubType", "FAMIC2C"),
                new KeyValuePair<string, string>("IsCollection", "N"),
                new KeyValuePair<string, string>("ServerReplyURL", settings.ServerReplyURL),
                new KeyValuePair<string, string>("ExtraData", ""),
                new KeyValuePair<string, string>("Device", "0")
            });
            Console.WriteLine(parameters);
            var request = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = parameters
            };
            var response = await _httpClient.SendAsync(request);
            if (!response.IsSuccessStatusCode) 
            { 
                return StatusCode((int)response.StatusCode,"API連線失敗");
            }
            var responseContent = await response.Content.ReadAsStringAsync();
            // 解析 HTML 取得 form 的 action URL
            var match = Regex.Match(responseContent, @"<form.*?action=[""'](.*?)[""']", RegexOptions.IgnoreCase);
            if (match.Success)
            {
                var formUrl = match.Groups[1].Value;

                // 如果是相對路徑，補上完整網址
                if (!formUrl.StartsWith("http"))
                {
                    formUrl = "https://logistics.ecpay.com.tw" + formUrl;
                }

                return Ok(new { mapUrl = formUrl });
            }
            else
            {
                return BadRequest("ECPay 回傳的 HTML 無法解析地圖 URL");
            }
        }


        [HttpPost("StoreSelection")]
        public IActionResult StoreSelection()
        {
            var testData = new
            {
                StoreName = "全家台北測試門市",
                StoreID = "001234",
            };

            return Ok(testData);
        }


        [HttpGet("StoreSelection")]
        public IActionResult TestECPayGet()
        {
            Console.WriteLine("收到 ECPay 測試 GET 請求，可能在驗證 URL");
            return Ok("ECPay 測試 GET 已收到");
        }

    }
}
