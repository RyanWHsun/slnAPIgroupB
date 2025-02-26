using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Threading.Tasks;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text.Json;
using prjGroupB.Models;

[AllowAnonymous]
[ApiController]
[Route("api/payment")]
public class LinePayController : ControllerBase
{
    private readonly LinePayService _linePayService;
    private readonly dbGroupBContext _dbContext;

    public LinePayController(LinePayService linePayService, dbGroupBContext dbContext)
    {
        _linePayService = linePayService;
        _dbContext = dbContext;
    }

    [HttpPost("request")]
    public async Task<IActionResult> RequestPayment([FromBody] LinePayRequestDto linePayRequestDto)
    {
        try
        {
            if (linePayRequestDto == null)
            {
                return BadRequest("Request body is missing or incorrect.");
            }

            var userId = 4; // 假設已登入
            if (userId == 0) return Unauthorized("請先登入");

            // **查詢購物車**
            var shoppingCart = await _dbContext.TShoppingCarts
                .Include(c => c.FUser)
                .Include(c => c.TShoppingCartItems)
                .FirstOrDefaultAsync(c => c.FUserId == userId);

            if (shoppingCart == null || !shoppingCart.TShoppingCartItems.Any())
                return BadRequest("購物車是空的");

            // **發送 LINE Pay 請求**
            var paymentUrl = await _linePayService.CreatePaymentRequestAsync(linePayRequestDto);

            Console.WriteLine($"✅ 付款連結：{paymentUrl}");

            return Ok(new { paymentUrl });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ 付款 API 失敗: {ex.Message}");
            return StatusCode(500, $"❌ 付款請求發生錯誤: {ex.Message}");
        }
    }

    [HttpPost("confirm")]
    public async Task<IActionResult> ConfirmPayment([FromBody] ConfirmPaymentDto request)
    {
        try
        {
            var success = await _linePayService.ConfirmPaymentAsync(request.transactionId, request.amount);
            if (!success)
                return BadRequest("付款失敗");

            return Ok("付款成功");
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"❌ 確認付款發生錯誤: {ex.Message}");
        }
    }
}
