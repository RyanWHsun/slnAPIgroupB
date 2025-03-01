using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using prjGroupB.DTO;
using prjGroupB.Models;

namespace prjGroupB.Controllers {
    [Route("api/[controller]")]
    [ApiController]
    public class TAttractionTicketShoppingCart : ControllerBase {
        private readonly dbGroupBContext _context;

        public TAttractionTicketShoppingCart(dbGroupBContext context) {
            _context = context;
        }

        // GET: api/TAttractionTicketShoppingCart
        //[HttpGet]
        //public async Task<ActionResult<IEnumerable<TShoppingCart>>> GetTShoppingCarts()
        //{
        //    return await _context.TShoppingCarts.ToListAsync();
        //}

        // GET: api/TAttractionTicketShoppingCart/5
        //[HttpGet("{id}")]
        //public async Task<ActionResult<TShoppingCart>> GetTShoppingCart(int id)
        //{
        //    var tShoppingCart = await _context.TShoppingCarts.FindAsync(id);

        //    if (tShoppingCart == null)
        //    {
        //        return NotFound();
        //    }

        //    return tShoppingCart;
        //}

        // PUT: api/TAttractionTicketShoppingCart/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        //[HttpPut("{id}")]
        //public async Task<IActionResult> PutTShoppingCart(int id, TShoppingCart tShoppingCart)
        //{
        //    if (id != tShoppingCart.FCartId)
        //    {
        //        return BadRequest();
        //    }

        //    _context.Entry(tShoppingCart).State = EntityState.Modified;

        //    try
        //    {
        //        await _context.SaveChangesAsync();
        //    }
        //    catch (DbUpdateConcurrencyException)
        //    {
        //        if (!TShoppingCartExists(id))
        //        {
        //            return NotFound();
        //        }
        //        else
        //        {
        //            throw;
        //        }
        //    }

        //    return NoContent();
        //}

        // POST: api/TAttractionTicketShoppingCart
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        [Authorize]
        public async Task<TAttractionTicketShoppingCartDTO> PostTShoppingCart(TAttractionTicketShoppingCartDTO ticket) {
            // FindFirstValue(): 從 User.Claims 查找 第一個符合 ClaimTypes.NameIdentifier 的 Claim，並回傳它的值。
            // ClaimTypes.NameIdentifier 是一個 標準的 Claim 類型，表示「使用者的唯一識別碼」（通常是 UserId）。
            var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
            int userId = int.TryParse(userIdValue, out var parsedId) ? parsedId : 0;

            // 檢查登入的使用者是否已經有購物車
            var existingCart = await _context.TShoppingCarts.Include(c => c.TShoppingCartItems).FirstOrDefaultAsync(c => c.FUserId == userId);

            // 如果使用者沒有購物車，則建立一個新的購物車
            if (existingCart == null) {
                existingCart = new TShoppingCart {
                    FCartId = 0,
                    FUserId = userId,
                    FCreatedDate = DateTime.Now,
                    TShoppingCartItems = new List<TShoppingCartItem>()
                };
                _context.TShoppingCarts.Add(existingCart);
                await _context.SaveChangesAsync();// SaveChangesAsync() 會將資料寫入資料庫，會產生新的 cart.FCartId
            }

            var existingItem = await _context.TShoppingCartItems.FirstOrDefaultAsync(i => i.FCartId == existingCart.FCartId && i.FItemId == ticket.FTicketId && i.FItemType == "attractionTicket");

            if (existingItem != null) {
                // 1. existingItem 是透過 FirstOrDefaultAsync() 查詢出來的，因此 Entity Framework 會追蹤它。
                // 2. 修改 existingItem.FQuantity 時，Entity Framework 內部的 變更追蹤機制（Change Tracking） 會記錄這個屬性的變更。
                // 3. 當 await _context.SaveChangesAsync(); 執行時，Entity Framework 會發送 UPDATE SQL 指令，更新 TShoppingCartItems 資料表的 FQuantity 欄位。
                existingItem.FQuantity += ticket.FQuantity;// 如果購物車內已經有該項目，直接增加數量就好
            }
            else {
                var cartItem = new TShoppingCartItem {
                    FCartItemId = 0,
                    FCartId = existingCart.FCartId,
                    FItemType = "attractionTicket",
                    FItemId = ticket.FTicketId,
                    FQuantity = ticket.FQuantity,
                    FPrice = ticket.FPrice,
                };
                _context.TShoppingCartItems.Add(cartItem);
            }

            try {
                await _context.SaveChangesAsync();// SaveChangesAsync() 會將資料寫入資料庫，會產生新的 cart.FCartId
            }
            catch (DbUpdateConcurrencyException ex) {
                throw new Exception("TShoppingCarts update Error");
            }

            return ticket;
        }

        // DELETE: api/TAttractionTicketShoppingCart/5
        //[HttpDelete("{id}")]
        //public async Task<IActionResult> DeleteTShoppingCart(int id)
        //{
        //    var tShoppingCart = await _context.TShoppingCarts.FindAsync(id);
        //    if (tShoppingCart == null)
        //    {
        //        return NotFound();
        //    }

        //    _context.TShoppingCarts.Remove(tShoppingCart);
        //    await _context.SaveChangesAsync();

        //    return NoContent();
        //}

        private bool TShoppingCartExists(int id) {
            return _context.TShoppingCarts.Any(e => e.FCartId == id);
        }
    }
}
