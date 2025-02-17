using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using prjGroupB.Models;

namespace prjGroupB.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // ⬅ 確保用戶已登入
    public class EventRegistrationToCartController : ControllerBase
    {
        private readonly dbGroupBContext _context;

        public EventRegistrationToCartController(dbGroupBContext context)
        {
            _context = context;
        }

        // ✅ 【活動專用】將活動報名加入購物車
        [HttpPost("addEvent")]
        public async Task<IActionResult> AddEventToCart([FromBody] int eventId)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            // 🔹 檢查活動是否存在
            var eventItem = await _context.TEvents.FindAsync(eventId);
            if (eventItem == null)
            {
                return NotFound(new { message = "活動不存在" });
            }

            // 🔹 檢查是否已報名此活動（避免重複加入）
            var existingItem = await _context.TShoppingCartItems
                .FirstOrDefaultAsync(i => _context.TShoppingCarts.Any(c => c.FCartId == i.FCartId && c.FUserId == userId) &&
                                          i.FItemType == "eventFee" &&
                                          i.FItemId == eventId);

            if (existingItem != null)
            {
                return Conflict(new { message = "您已報名過此活動" });
            }

            // 🔹 檢查是否已存在購物車
            var existingCart = await _context.TShoppingCarts
                .Include(c => c.TShoppingCartItems)
                .FirstOrDefaultAsync(c => c.FUserId == userId);

            // 🔹 如果購物車不存在就建新的
            if (existingCart == null)
            {
                existingCart = new TShoppingCart
                {
                    FUserId = userId,
                    FCreatedDate = DateTime.Now,
                    TShoppingCartItems = new List<TShoppingCartItem>()
                };
                _context.TShoppingCarts.Add(existingCart);
                await _context.SaveChangesAsync();
            }

            // 🔹 設定活動報名價格
            //decimal eventPrice = eventItem.FEventPrice ?? 0;

            // 🔹 新增活動至購物車
            var newItem = new TShoppingCartItem
            {
                FCartId = existingCart.FCartId,
                FItemType = "eventFee",  // ✅ 確保是活動報名
                FItemId = eventId,
                FQuantity = 1,  // ✅ 活動報名固定數量 1
                //FPrice = eventPrice
            };
            _context.TShoppingCartItems.Add(newItem);
            await _context.SaveChangesAsync();

            return Ok(new { message = "活動已成功加入購物車" });
        }
    }
}