using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net.Http;
using System.Text.Json;
using System.Text;
using System.Threading.Tasks;
using System.Security.Claims;
using prjGroupB.Models; // ✅ 確保引入

[AllowAnonymous] // 允許未登入的用戶請求
[ApiController]
[Route("api/payment")]
public class PaymentController : ControllerBase
{
    private readonly LinePayService _linePayService;
    private readonly dbGroupBContext _dbContext;
    private readonly HttpClient _httpClient;

    public PaymentController(LinePayService linePayService, dbGroupBContext dbContext, HttpClient httpClient)
    {
        _linePayService = linePayService;
        _dbContext = dbContext;
        _httpClient = httpClient;
    }

    private string GetItemName(string fItemType, int? fItemId)
    {
        if (fItemId == null) return "未命名商品";

        try
        {
            return fItemType switch
            {
                "product" => _dbContext.TProducts
                    .Where(p => p.FProductId == fItemId)
                    .Select(p => p.FProductName)
                    .FirstOrDefault() ?? "未命名商品",

                "attractionTicket" => (from ticket in _dbContext.TAttractionTickets
                                       join attraction in _dbContext.TAttractions
                                       on ticket.FAttractionId equals attraction.FAttractionId
                                       where ticket.FAttractionTicketId == fItemId
                                       select attraction.FAttractionName)
                                       .FirstOrDefault() ?? "未命名票券",

                "eventFee" => _dbContext.TEvents
                    .Where(e => e.FEventId == fItemId)
                    .Select(e => e.FEventName)
                    .FirstOrDefault() ?? "未命名活動",

                _ => "未知類型"
            };
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ GetItemName 發生錯誤: {ex.Message}");
            return "錯誤商品";  // 避免回傳空字串
        }
    }

    [AllowAnonymous]
    [HttpPost("request")]
    public async Task<IActionResult> RequestPayment()
    {
        try
        {
            var userId = int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out int uid) ? uid : 0;
            var shoppingCart = await _dbContext.TShoppingCarts.FirstOrDefaultAsync(c => c.FUserId == userId);

            if (shoppingCart == null) return BadRequest("購物車不存在");

            var cartItems = await _dbContext.TShoppingCartItems
                .Where(i => i.FCartId == shoppingCart.FCartId)
                .ToListAsync();

            if (!cartItems.Any()) return BadRequest("購物車是空的");

            // 產生唯一 orderId
            string orderId = $"ORDER_{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}_{userId}";

            // 計算總金額
            decimal totalAmount = cartItems.Sum(i => (i.FPrice ?? 0) * (i.FQuantity ?? 1));
            if (totalAmount <= 0) totalAmount = 1; // 確保價格不為 0

            // 呼叫 LinePayService
            var paymentUrl = await _linePayService.CreatePaymentRequestAsync(totalAmount, orderId, "購物車結帳");

            return Ok(new { paymentUrl });
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"❌ 付款請求發生錯誤: {ex.Message}");
        }
    }

    [HttpPost("confirm")]
    public async Task<IActionResult> ConfirmPayment([FromBody] ConfirmPaymentDto request)
    {
        var success = await _linePayService.ConfirmPaymentAsync(request.TransactionId, request.Amount);
        if (!success)
            return BadRequest("付款失敗");

        return Ok("付款成功");
    }
}

public class ConfirmPaymentDto
{
    public string TransactionId { get; set; }
    public decimal Amount { get; set; }
}