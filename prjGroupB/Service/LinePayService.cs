using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using RestSharp;
using Newtonsoft.Json;
using Dapper;
using Microsoft.Data.SqlClient;

public class LinePayService
{
    private readonly IConfiguration _config;
    private readonly string _channelId;
    private readonly string _channelSecret;
    private readonly string _baseUrl;
    private readonly string _connectionString;

    public LinePayService(IConfiguration config)
    {
        _config = config;
        _channelId = _config["LinePay:ChannelId"];
        _channelSecret = _config["LinePay:ChannelSecret"];
        _baseUrl = _config["LinePay:BaseUrl"];
        _connectionString = _config.GetConnectionString("dbGroupB"); // ✅ 確保用正確的 Key
    }

    // ✅ 取得訂單商品資訊
    public async Task<List<PaymentPackage>> GetOrderPackagesAsync(string orderId)
    {
        Console.WriteLine($"🔍 查詢訂單 {orderId} 是否存在...");

        using (var connection = new SqlConnection(_connectionString))
        {
            var query = @"
        SELECT
            OD.fItemId AS ProductId,
            P.fProductName AS ProductName,
            P.fProductPrice AS ProductPrice
        FROM tOrdersDetails OD
        JOIN tProduct P ON OD.fItemId = P.fProductId
        WHERE OD.fOrderId = @OrderId;";

            var orderItems = (await connection.QueryAsync(query, new { OrderId = orderId })).ToList();

            if (orderItems == null || orderItems.Count == 0)
            {
                Console.WriteLine($"❌ 查無此訂單 {orderId}，回傳空資料");
                return new List<PaymentPackage>(); // 讓前端顯示錯誤訊息
            }

            Console.WriteLine($"✅ 訂單 {orderId} 存在，商品數量: {orderItems.Count}");

            var packages = new List<PaymentPackage>();
            var package = new PaymentPackage
            {
                id = "PKG001",
                amount = 0,
                name = "訂單結帳",
                products = new List<PaymentProduct>()
            };

            foreach (var item in orderItems)
            {
                var product = new PaymentProduct
                {
                    id = item.ProductId.ToString(),
                    name = string.IsNullOrWhiteSpace(item.ProductName) ? "預設商品名稱" : item.ProductName,
                    imageUrl = "https://example.com/default-product.jpg",
                    quantity = 1, // 假設數量為 1
                    price = item.ProductPrice
                };
                package.products.Add(product);
                package.amount += item.ProductPrice;
            }

            packages.Add(package);
            return packages;
        }
    }

    // ✅ 取得訂單資訊
    public async Task<OrderInfoDto> GetOrderByTransactionIdAsync(string transactionId)
    {
        using (var connection = new SqlConnection(_connectionString))
        {
            var query = @"
            SELECT
                O.fOrderId AS OrderId,
                O.fBuyerId AS BuyerId,
                O.fPaymentMethod AS PaymentMethod,
                SUM(OD.fUnitPrice * OD.fOrderQty) AS TotalAmount
            FROM tOrders O
            JOIN tOrdersDetails OD ON O.fOrderId = OD.fOrderId
            WHERE O.fOrderId = @TransactionId
            GROUP BY O.fOrderId, O.fBuyerId, O.fPaymentMethod;";

            return await connection.QueryFirstOrDefaultAsync<OrderInfoDto>(query, new { TransactionId = transactionId });
        }
    }

    // ✅ 發送付款請求
    public async Task<string> RequestPaymentAsync(decimal totalAmount, string currency, string orderId, List<PaymentPackage> packages, string confirmUrl, string cancelUrl)
    {
        var client = new RestClient($"{_baseUrl}/request");
        var request = new RestRequest();
        request.Method = Method.Post;

        request.AddHeader("Content-Type", "application/json");
        request.AddHeader("X-LINE-ChannelId", _channelId);
        request.AddHeader("X-LINE-ChannelSecret", _channelSecret);

        var body = new
        {
            amount = totalAmount,
            currency = currency,
            orderId = orderId,
            packages = packages,
            redirectUrls = new
            {
                confirmUrl = confirmUrl,
                cancelUrl = cancelUrl
            }
        };

        request.AddJsonBody(body);
        var response = await client.ExecuteAsync(request);

        Console.WriteLine("發送的請求：" + JsonConvert.SerializeObject(body, Formatting.Indented));
        Console.WriteLine("LINE Pay API 回應：" + response.Content);

        return response.Content ?? "{}";
    }

    // ✅ 確認付款狀態
    public async Task<string> ConfirmPaymentAsync(string transactionId, decimal amount, string currency)
    {
        var client = new RestClient($"{_baseUrl}/{transactionId}/confirm");
        var request = new RestRequest();
        request.Method = Method.Post;

        request.AddHeader("Content-Type", "application/json");
        request.AddHeader("X-LINE-ChannelId", _channelId);
        request.AddHeader("X-LINE-ChannelSecret", _channelSecret);

        var body = new
        {
            amount = amount,
            currency = currency
        };

        request.AddJsonBody(body);
        var response = await client.ExecuteAsync(request);

        Console.WriteLine("確認付款請求：" + JsonConvert.SerializeObject(body, Formatting.Indented));
        Console.WriteLine("LINE Pay 確認付款 API 回應：" + response.Content);

        if (!response.IsSuccessful)
        {
            throw new Exception($"LINE Pay 確認付款失敗: {response.StatusCode} - {response.ErrorMessage}");
        }

        return response.Content ?? "{}";
    }
}

// ✅ 訂單資訊 DTO
public class OrderInfoDto
{
    public string OrderId { get; set; }
    public int BuyerId { get; set; }
    public string PaymentMethod { get; set; }
    public decimal TotalAmount { get; set; }
}

// ✅ 付款請求所需的商品包裝
public class PaymentPackage
{
    public string id { get; set; }
    public decimal amount { get; set; }
    public string name { get; set; }
    public List<PaymentProduct> products { get; set; }
}

// ✅ 單個商品資訊
public class PaymentProduct
{
    public string id { get; set; }
    public string name { get; set; }
    public string imageUrl { get; set; }
    public int quantity { get; set; }
    public decimal price { get; set; }
}