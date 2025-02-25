using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using prjGroupB.DTO;  // ✅ 確保引用 DTO 命名空間

[Route("api/[controller]")]
[ApiController]
public class LinePayController : ControllerBase
{
    private readonly LinePayService _linePayService;

    public LinePayController(LinePayService linePayService)
    {
        _linePayService = linePayService;
    }

    /// <summary>
    /// 發送 LinePay 付款請求
    /// </summary>
    //[HttpPost("request-payment")]
    //public async Task<IActionResult> RequestPayment([FromBody] PaymentRequestDTO request)
    //{
    //    try
    //    {
    //        Console.WriteLine("🚀 進入 RequestPayment API");
    //        Console.WriteLine($"📥 接收到的 orderId: {request.OrderId}");

    //        if (request == null || request.OrderId <= 0)
    //        {
    //            Console.WriteLine("❌ 無效的訂單 ID，回傳錯誤");
    //            return BadRequest(new { message = "無效的訂單 ID" });
    //        }

    //        var packages = await _linePayService.GetOrderPackagesAsync(request.OrderId.ToString());
    //        if (packages == null || !packages.Any())
    //        {
    //            Console.WriteLine($"❌ 訂單 {request.OrderId} 沒有對應的商品");
    //            return BadRequest(new { message = "找不到對應的訂單商品" });
    //        }

    //        // ✅ 轉換 List<Package> 為 List<PaymentPackage>
    //        var paymentPackages = packages.Select(p => new PaymentPackage
    //        {
    //            Id = p.Id.ToString(),
    //            Amount = p.Amount,
    //            Name = p.Name,
    //            Products = p.Products?.Select(pr => new PaymentProduct
    //            {
    //                Id = pr.Id.ToString(),
    //                Name = pr.Name,
    //                ImageUrl = pr.ImageUrl,
    //                Quantity = pr.Quantity,
    //                Price = pr.Price
    //            }).ToList() ?? new List<PaymentProduct>()  // 避免 null 例外
    //        }).ToList();

    //        var response = await _linePayService.RequestPaymentAsync(
    //            request.TotalAmount,
    //            "TWD",
    //            request.OrderId.ToString(),
    //            paymentPackages,
    //            request.ConfirmUrl,
    //            request.CancelUrl
    //        );

    //        Console.WriteLine($"✅ LINE Pay 回應：{response}");
    //        var jsonResponse = Newtonsoft.Json.JsonConvert.DeserializeObject<object>(response);
    //        return Ok(jsonResponse);
    //    }
    //    catch (Exception ex)
    //    {
    //        Console.WriteLine($"❌ 發生錯誤：{ex.Message}");
    //        return BadRequest(new { message = "付款請求失敗", error = ex.Message });
    //    }
    //}

    /// <summary>
    /// 確認 LinePay 付款
    /// </summary>
    [HttpPost("confirm-payment")]
    public async Task<IActionResult> ConfirmPayment([FromBody] ConfirmPaymentDto request)
    {
        try
        {
            if (request == null || string.IsNullOrEmpty(request.TransactionId) || request.Amount <= 0)
            {
                return BadRequest(new { message = "無效的交易資訊" });
            }

            var order = await _linePayService.GetOrderByTransactionIdAsync(request.TransactionId);
            if (order == null)
            {
                return BadRequest(new { message = "無法找到對應的訂單" });
            }

            if (order.TotalAmount != request.Amount)
            {
                return BadRequest(new { message = "付款金額不匹配" });
            }

            var result = await _linePayService.ConfirmPaymentAsync(request.TransactionId, request.Amount, "TWD");
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = "確認付款失敗", error = ex.Message });
        }
    }
}