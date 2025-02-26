using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

public class LinePayService
{
    private readonly HttpClient _httpClient;
    private const string LINE_PAY_URL = "https://sandbox-api-pay.line.me/v2/payments/request"; // 測試環境
    private const string CONFIRM_URL = "https://sandbox-api-pay.line.me/v2/payments/{0}/confirm";
    private const string CHANNEL_ID = "2006949561";
    private const string CHANNEL_SECRET = "1724fe3b7e82ea6bd7cf8cfcd91f0d4a";
    private const string FRONTEND_BASE_URL = "http://localhost:4200";

    public LinePayService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<string> CreatePaymentRequestAsync(decimal amount, string orderId, string productName)
    {
        var requestBody = new
        {
            amount = amount,
            currency = "TWD",
            orderId = orderId,
            packages = new[]
            {
                new {
                    id = "package-001",
                    amount = amount,
                    name = productName,
                    products = new[]
                    {
                        new {
                            id = "product-001",
                            name = productName,
                            quantity = 1,
                            price = amount
                        }
                    }
                }
            },
            redirectUrls = new
            {
                confirmUrl = $"{FRONTEND_BASE_URL}/home",  // 付款成功後跳轉首頁
                cancelUrl = $"{FRONTEND_BASE_URL}/payment/cancel?redirect=previous"  // 付款取消由前端處理返回
            }
        };

        var json = JsonSerializer.Serialize(requestBody);
        var requestMessage = new HttpRequestMessage(HttpMethod.Post, LINE_PAY_URL)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        };

        // **正確加入 Header**
        requestMessage.Headers.Add("X-LINE-ChannelId", CHANNEL_ID);
        requestMessage.Headers.Add("X-LINE-ChannelSecret", CHANNEL_SECRET);

        var response = await _httpClient.SendAsync(requestMessage);
        var responseString = await response.Content.ReadAsStringAsync();

        // **先檢查回應是否包含 "info" 屬性**
        using var doc = JsonDocument.Parse(responseString);
        if (!doc.RootElement.TryGetProperty("info", out JsonElement info))
        {
            throw new Exception($"LINE Pay 回應錯誤: {responseString}");
        }

        return info.GetProperty("paymentUrl").GetProperty("web").GetString();
    }

    public async Task<bool> ConfirmPaymentAsync(string transactionId, decimal amount)
    {
        var url = string.Format(CONFIRM_URL, transactionId);
        var requestBody = new { amount = amount, currency = "TWD" };
        var json = JsonSerializer.Serialize(requestBody);

        var requestMessage = new HttpRequestMessage(HttpMethod.Post, url)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        };

        // **確保 Header 正確**
        requestMessage.Headers.Add("X-LINE-ChannelId", CHANNEL_ID);
        requestMessage.Headers.Add("X-LINE-ChannelSecret", CHANNEL_SECRET);

        var response = await _httpClient.SendAsync(requestMessage);
        return response.IsSuccessStatusCode;
    }
}