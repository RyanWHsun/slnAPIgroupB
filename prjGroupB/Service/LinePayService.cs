using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

public class LinePayService
{
    private readonly HttpClient _httpClient;
    private const string LINE_PAY_BASE_URL = "https://sandbox-api-pay.line.me"; // 測試環境
    private const string LINE_PAY_REQUEST_URL = "/v3/payments/request";
    private const string LINE_PAY_CONFIRM_URL = "/v3/payments/{0}/confirm";

    private const string CHANNEL_ID = "2006949561";
    private const string CHANNEL_SECRET = "1724fe3b7e82ea6bd7cf8cfcd91f0d4a";

    private const string FRONTEND_BASE_URL = "https://28e9-1-160-19-244.ngrok-free.app/event/detail/2007"; // 你的前端網址

    public LinePayService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    /// **🔹 產生 HMAC-SHA256 簽名**
    private string GenerateHmacSHA256(string message, string key)
    {
        using (var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(key)))
        {
            byte[] hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(message));
            return Convert.ToBase64String(hash);
        }
    }
    [Authorize]
    [HttpPost]
    /// **🔹 建立 LINE Pay 付款請求**
    public async Task<string> CreatePaymentRequestAsync(LinePayRequestDto requestDto)
    {
        string nonce = Guid.NewGuid().ToString("N");
        string requestUrl = LINE_PAY_REQUEST_URL;

        var requestBody = new
        {
            amount = requestDto.amount,
            currency = "TWD",
            orderId = requestDto.orderId,
            packages = requestDto.packages,
            redirectUrls = new
            {
                confirmUrl = $"{FRONTEND_BASE_URL}",
                cancelUrl = "https://28e9-1-160-19-244.ngrok-free.app/products/cart"
            }
        };

        string json = JsonSerializer.Serialize(requestBody, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
        string signature = GenerateHmacSHA256(CHANNEL_SECRET + requestUrl + json + nonce, CHANNEL_SECRET);

        var requestMessage = new HttpRequestMessage(HttpMethod.Post, $"{LINE_PAY_BASE_URL}{requestUrl}")
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        };

        // 設定 Header
        requestMessage.Headers.Add("X-LINE-Authorization", signature);
        requestMessage.Headers.Add("X-LINE-Authorization-Nonce", nonce);
        requestMessage.Headers.Add("X-LINE-ChannelId", CHANNEL_ID);

        var response = await _httpClient.SendAsync(requestMessage);
        var responseString = await response.Content.ReadAsStringAsync();

        Console.WriteLine($"📌 LINE Pay API Response: {responseString}");

        using var doc = JsonDocument.Parse(responseString);
        if (!doc.RootElement.TryGetProperty("info", out JsonElement info))
        {
            throw new Exception($"LINE Pay 回應錯誤: {responseString}");
        }

        return info.GetProperty("paymentUrl").GetProperty("web").GetString();
    }

    /// **🔹 確認 LINE Pay 付款**
    public async Task<bool> ConfirmPaymentAsync(string transactionId, decimal amount)
    {
        string nonce = Guid.NewGuid().ToString("N");
        string requestUrl = string.Format(LINE_PAY_CONFIRM_URL, transactionId);

        var requestBody = new
        {
            amount = amount,
            currency = "TWD"
        };

        string json = JsonSerializer.Serialize(requestBody, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
        string signature = GenerateHmacSHA256(CHANNEL_SECRET + requestUrl + json + nonce, CHANNEL_SECRET);

        var requestMessage = new HttpRequestMessage(HttpMethod.Post, $"{LINE_PAY_BASE_URL}{requestUrl}")
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        };

        // 設定 Header
        requestMessage.Headers.Add("X-LINE-Authorization", signature);
        requestMessage.Headers.Add("X-LINE-Authorization-Nonce", nonce);
        requestMessage.Headers.Add("X-LINE-ChannelId", CHANNEL_ID);

        var response = await _httpClient.SendAsync(requestMessage);
        var responseString = await response.Content.ReadAsStringAsync();

        Console.WriteLine($"📌 LINE Pay Confirm Response: {responseString}");

        return response.IsSuccessStatusCode;
    }
}




