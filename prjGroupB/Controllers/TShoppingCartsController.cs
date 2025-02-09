using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.Elfie.Model.Tree;
using Microsoft.EntityFrameworkCore;
using prjGroupB.DTO;
using prjGroupB.Models;

namespace prjGroupB.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class TShoppingCartsController : ControllerBase
    {
        private readonly dbGroupBContext _context;

        public TShoppingCartsController(dbGroupBContext context)
        {
            _context = context;
        }

        // GET: api/TShoppingCarts
        [HttpGet]       
        public async Task<IActionResult> GetShoppingCart()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            //var userId = 5;
            var cartItems = await _context.TShoppingCartItems
                .Where(i=>_context.TShoppingCarts.Any(c=>c.FCartId==i.FCartId&&c.FUserId==userId))
                .ToListAsync(); // 先從資料庫獲取清單

            var result = cartItems.Select(i=> new TShoppingCartDTO
                {
                    FUserId=userId,
                    FCartItemId=i.FCartItemId,
                    FItemType=i.FItemType,
                    FItemId=i.FItemId,
                    FPrice=i.FPrice,
                    FQuantity=i.FQuantity,
                    FItemName=GetItemName(i.FItemType,i.FItemId),
                    FSingleImage=GetItemImage(i.FItemType, i.FItemId),
                    FSellerName = i.FItemType == "product"? GetSellerName(i.FItemType,i.FItemId):null,
                    FProductStock=i.FItemType =="product"? _context.TProducts.FirstOrDefault(p => p.FProductId == i.FItemId)?.FStock : null,
                FSpecification = null, //若有規格要填

                }).ToList();
            if (!result.Any())
            {
                return NotFound(new { messange = "購物車無項目。2" });
            }
            return Ok(result);
        }

        private static string GetItemName(string fItemType, int? fItemId)
        {
            if(fItemId == null)
            {
                return "未知商品";
            }
            using (var context = new dbGroupBContext())
            {
                return fItemType switch
                {
                    "product" => context.TProducts.FirstOrDefault(p => p.FProductId == fItemId)?.FProductName ?? "商品",
                    "attractionTicket" => context.TAttractions.FirstOrDefault(a => a.FAttractionId == (context.TAttractionTickets.FirstOrDefault(t => t.FAttractionTicketId == fItemId).FAttractionId)).FAttractionName ?? "景點名稱",
                    "eventFee" => context.TEvents.FirstOrDefault(p => p.FEventId == fItemId)?.FEventName ?? "活動"
                };

            } 
                
        }

        private static string GetSellerName(string fItemType, int? fItemId)
        {
            if (fItemId == null || fItemType != "product") 
            {
                return null;
            }
            using (var context = new dbGroupBContext())
            {
                var sellerUserId = context.TProducts.FirstOrDefault(p => p.FProductId == fItemId)?.FUserId;
                return context.TUsers.FirstOrDefault(u => u.FUserId == sellerUserId).FUserNickName ;
            }
        }
        private static string? GetItemImage(string fItemType, int? fItemId)
        {
            if (fItemId == null)
            {
                return null;
            }
            using (var context = new dbGroupBContext())
            {
                byte[] imageBytes = fItemType switch
                {
                    "product" => context.TProductImages.FirstOrDefault(i => i.FProductId == fItemId).FImage,
                    "attractionTicket" => context.TAttractionImages.FirstOrDefault(a => a.FAttractionId == (context.TAttractionTickets.FirstOrDefault(t => t.FAttractionTicketId == fItemId).FAttractionId)).FImage,
                    "eventFee" => context.TEventImages.FirstOrDefault(p => p.FEventId == fItemId).FEventImage,

                };
                return imageBytes != null ? ConvertToThumbnailBase64(imageBytes, 100, 100) : null;
            }

        }
        private static string ConvertToThumbnailBase64(byte[] fUserImage, int width, int height)
        {
            using (var ms = new MemoryStream(fUserImage))
            {
                // 使用 System.Drawing 讀取圖片
                using (var image = Image.FromStream(ms))
                {
                    // 建立縮圖
                    using (var thumbnail = image.GetThumbnailImage(width, height, () => false, IntPtr.Zero))
                    {
                        using (var thumbnailStream = new MemoryStream())
                        {
                            // 儲存縮圖到記憶體流
                            thumbnail.Save(thumbnailStream, ImageFormat.Png);
                            // 將縮圖轉換為 Base64
                            return Convert.ToBase64String(thumbnailStream.ToArray());
                        }
                    }
                }
            }
        }


        // GET: api/TShoppingCarts/5
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

        // PUT: api/TShoppingCarts/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutTShoppingCart(int id, TShoppingCart tShoppingCart)
        {
            if (id != tShoppingCart.FCartId)
            {
                return BadRequest();
            }

            _context.Entry(tShoppingCart).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!TShoppingCartExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }


        //[HttpPost]
        //public async Task<ActionResult<TShoppingCart>> PostTShoppingCart(TShoppingCart tShoppingCart)
        //{
        //    _context.TShoppingCarts.Add(tShoppingCart);
        //    await _context.SaveChangesAsync();

        //    return CreatedAtAction("GetTShoppingCart", new { id = tShoppingCart.FCartId }, tShoppingCart);
        //}


        //POST: api/TShoppingCarts
        //To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        //增加商品
        [HttpPost("addProductToCart")]
        public async Task<IActionResult> addProductToCart([FromBody] addProductToCartDTO cartDTO)
        {
            if (cartDTO == null || cartDTO.FQuantity<=0) 
            {
                return BadRequest("無效的項目");
            }
            //抓用戶ID
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            
            //檢查是否已存在購物車
            var existingCart = await _context.TShoppingCarts
                .Include(c=>c.TShoppingCartItems)
                .FirstOrDefaultAsync(c=>c.FUserId == userId);
            
            //如果購物車不存在就建新的
            if (existingCart == null) 
            {
                existingCart = new TShoppingCart
                {
                    FUserId = userId,
                    FCreatedDate = DateTime.Now,
                    TShoppingCartItems = new List<TShoppingCartItem>()
                };
                _context.TShoppingCarts.Add(existingCart);
                await _context.SaveChangesAsync(); //先儲存取得id
            }

            //檢查購物車是否已有該項目
            var existingItem = await _context.TShoppingCartItems
                .FirstOrDefaultAsync(i=>i.FCartId==existingCart.FCartId && 
                                        i.FItemType== cartDTO.FItemType && 
                                        i.FItemId==cartDTO.FItemId );

            //取得該商品的庫存
            int stock = await _context.TProducts
                .Where(p => p.FProductId == cartDTO.FItemId)
                .Select(p => p.FStock??0)
                .FirstOrDefaultAsync();

            if (existingItem != null)
            {
                if(existingItem.FQuantity + cartDTO.FQuantity > stock)
                {
                    return BadRequest(new { message = "購物車數量已達庫存上限，無法再加入。" });
                }
                existingItem.FQuantity += cartDTO.FQuantity;
            }
            else
            {
                if (cartDTO.FQuantity > stock) 
                {
                    return BadRequest(new { message = "庫存不足，無法加入購物車。" });
                }
                var newItem = new TShoppingCartItem
                {
                    FCartId = existingCart.FCartId,
                    FItemType = cartDTO.FItemType,
                    FItemId = cartDTO.FItemId,
                    FQuantity = cartDTO.FQuantity,
                    FPrice = cartDTO.FPrice,
                };
                _context.TShoppingCartItems.Add(newItem);
            }
            await _context.SaveChangesAsync();
            return Ok(new { message = "商品已成功加入購物車" });      
        }




        // DELETE: api/TShoppingCarts/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTShoppingCart(int id)
        {
            var tShoppingCart = await _context.TShoppingCarts.FindAsync(id);
            if (tShoppingCart == null)
            {
                return NotFound();
            }

            _context.TShoppingCarts.Remove(tShoppingCart);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool TShoppingCartExists(int id)
        {
            return _context.TShoppingCarts.Any(e => e.FCartId == id);
        }
    }
}
