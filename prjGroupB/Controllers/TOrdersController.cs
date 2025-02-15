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
using static prjGroupB.DTO.TOrderCreateDTO;

namespace prjGroupB.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TOrdersController : ControllerBase
    {
        private readonly dbGroupBContext _context;

        public TOrdersController(dbGroupBContext context)
        {
            _context = context;
        }

        //取得會員資料&錢包金額
        //GET: api/TOrders/getUserInfo
        [HttpGet("getUserInfo")]
        [Authorize]
        public async Task<ActionResult<TUserInfoForOrderDTO>> GetUserInfo()
        {
            try
            {
                var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

                //會員資料
                var user = await _context.TUsers
                    .Where(u => u.FUserId == userId)
                    .Select(u => new
                    {
                        u.FUserName,
                        u.FUserPhone,
                        u.FUserAddress,
                        u.FUserEmail
                    })
                    .FirstOrDefaultAsync();

                if (user == null)
                {
                    return NotFound(new { message = "找不到會員" });
                }

                //計算會員錢包金額
                int totalBalance = await _context.TWallets
                    .Where(w => w.FUserId == userId)
                    .SumAsync(w => w.FAmountChange ?? 0);

                //回傳
                return Ok(new TUserInfoForOrderDTO
                {
                    FUserId = userId,
                    FUserName = user.FUserName,
                    FUserPhone = user.FUserPhone,
                    FUserEmail = user.FUserEmail,
                    FUserAddress = user.FUserAddress,
                    TotalBalance = totalBalance
                });
        
            }
            catch (Exception ex) 
            {
                return StatusCode(500, new { message = "無法獲取會員資訊與錢包餘額", error = ex.Message });
            }
        }

        //建立訂單
        // POST: api/TOrders
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Checkout([FromBody] CheckoutRequestDTO checkoutRequest)
        {
            await using var transaction = await _context.Database.BeginTransactionAsync(); //加入交易機制
            try
            {
                var buyerId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
                //var buyerId = 3004;

                if (checkoutRequest == null || checkoutRequest.SelectedItems == null || !checkoutRequest.SelectedItems.Any())
                {
                    return BadRequest(new { message = "訂單內容無效" });
                }

                // 檢查所有商品是否可購買
                foreach (var item in checkoutRequest.SelectedItems)
                {
                    if (item.FItemType == "product")
                    {
                        var product = await _context.TProducts.FindAsync(item.FItemId);
                        //檢查上架狀態
                        if (product == null || !product.FIsOnSales.GetValueOrDefault())
                        {
                            await transaction.RollbackAsync();
                            return BadRequest(new { message = $"商品【{product?.FProductName ?? "未知商品"}】已下架，請稍後再試或洽詢賣方。" });
                        }
                        //檢查庫存
                        if (product.FStock < item.FQuantity)
                        {
                            await transaction.RollbackAsync();
                            return BadRequest(new { message = $"【{product.FProductName}】庫存只剩{product.FStock}，請修改數量後再購買" });
                        }
                        product.FStock -= item.FQuantity;
                    }
                }
                var createdOrders = new List<TOrder>();

                //分組處理商品(依sellerId分組)
                var productOrders = checkoutRequest.SelectedItems
                    .Where(item => item.FItemType == "product")
                    .GroupBy(item => item.FSellerId)
                    .ToList();

                //活動&票券依類別分組
                var otherOrders= checkoutRequest.SelectedItems
                    .Where(item=>item.FItemType == "attractionTicket" || item.FItemType == "eventFee")
                    .GroupBy(item => item.FItemType)
                    .ToList();

                if (productOrders.Any()) //有商品就處理商品訂單
                {                    
                    foreach (var group in productOrders)
                    {
                        var sellerId = group.Key;

                        var order = new TOrder
                        {
                            FBuyerId = buyerId,
                            FOrderStatusId = 1, //待出貨
                            FOrderDate = DateTime.Now,
                            FShipAddress = checkoutRequest.UserInfo.FUserAddress,
                            FPaymentMethod = checkoutRequest.FPaymentMethod,
                            TOrdersDetails = new List<TOrdersDetail>()
                        };

                        //先存檔產生FOrderId
                        _context.TOrders.Add(order);
                        await _context.SaveChangesAsync();

                        var orderTotal = 0m; //計算金額用

                        foreach (var item in group)
                        {
                            var unitPrice = GetItemPrice(item.FItemType, item.FItemId);
                            orderTotal += unitPrice * item.FQuantity;

                            var orderDetail = new TOrdersDetail
                            {
                                FOrderId = order.FOrderId,
                                FItemId = item.FItemId,
                                FItemType = item.FItemType,
                                FOrderQty = item.FQuantity,
                                FUnitPrice = unitPrice,
                                FExtraInfo = null  //備註
                            };
                            _context.TOrdersDetails.Add(orderDetail);

                            //刪除購物車內該商品
                            var cartItemToRemove = await _context.TShoppingCartItems.FindAsync(item.FCartItemId);
                            if (cartItemToRemove != null)
                            {
                                _context.TShoppingCartItems.Remove(cartItemToRemove);
                            }
                        }

                        //新增訂單歷史紀錄
                        var orderHistory = new TOrderStatusHistory
                        {
                            FOrderId = order.FOrderId,
                            FOrderStatusId = 1, //待出貨
                            FStatusName = _context.TOrderStatuses.FirstOrDefault(s => s.FOrderStatusId == 1).FStatusName,
                            FTimestamp = DateTime.Now,
                        };
                        _context.TOrderStatusHistories.Add(orderHistory);

                        //錢包扣款
                        if (checkoutRequest.FPaymentMethod == "Wallet")
                        {
                            var walletTransaction = new TWallet
                            {
                                FUserId = buyerId,
                                FAmountChange=(int)(-orderTotal),// 扣款，確保轉換為 int
                                FChangeLog=$"付款：訂單編號{order.FOrderId}",
                                FChangeTime=DateTime.Now,
                            };
                            _context.TWallets.Add(walletTransaction);
                        }

                        createdOrders.Add(order);
                    }
                }

                if (otherOrders.Any()) //處理票券&活動
                {
                    foreach (var group in otherOrders)
                    {
                        var order = new TOrder
                        {
                            FBuyerId = buyerId,
                            FOrderStatusId = 3, //訂單完成
                            FOrderDate = DateTime.Now,
                            FShipAddress = null,
                            FPaymentMethod = checkoutRequest.FPaymentMethod,
                            TOrdersDetails = new List<TOrdersDetail>()
                        };
                        _context.TOrders.Add(order);
                        await _context.SaveChangesAsync();

                        var orderTotal = 0m;

                        foreach (var item in group)
                        {
                            var unitPrice = GetItemPrice(item.FItemType, item.FItemId);
                            orderTotal += unitPrice * item.FQuantity;

                            var orderDetail = new TOrdersDetail
                            {
                                FOrderId = order.FOrderId,
                                FItemId = item.FItemId,
                                FItemType = item.FItemType,
                                FOrderQty = item.FQuantity,
                                FUnitPrice = unitPrice,
                                FExtraInfo = null  //備註
                            };
                            _context.TOrdersDetails.Add(orderDetail);
                            //刪除購物車內該商品
                            var cartItem = await _context.TShoppingCartItems.FindAsync(item.FCartItemId);
                            if (cartItem != null)
                            {
                                _context.TShoppingCartItems.Remove(cartItem);
                            }
                        }
                        if (checkoutRequest.FPaymentMethod == "Wallet")
                        {
                            var walletTransaction = new TWallet
                            {
                                FUserId = buyerId,
                                FAmountChange = (int)(-orderTotal), // 扣款，確保轉換為 int
                                FChangeLog = $"付款：訂單編號{order.FOrderId}",
                                FChangeTime = DateTime.Now
                            };
                            _context.TWallets.Add(walletTransaction);
                        }
                        //新增訂單歷史紀錄
                        var orderHistory = new TOrderStatusHistory
                        {
                            FOrderId = order.FOrderId,
                            FOrderStatusId = 3, //訂單完成
                            FStatusName = _context.TOrderStatuses.FirstOrDefault(s => s.FOrderStatusId == 3).FStatusName,
                            FTimestamp = DateTime.Now,
                        };
                        _context.TOrderStatusHistories.Add(orderHistory);
                        createdOrders.Add(order);
                    }
                }
                int affectedRows = await _context.SaveChangesAsync(); // 這裡檢查是否成功寫入
                await transaction.CommitAsync(); //提交交易
                if (affectedRows == 0)
                {
                    return StatusCode(500, new { message = "訂單建立失敗，未能寫入資料庫" });
                }
                return Ok(new { message = "訂單建立成功"});
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, new { message = "產生訂單出錯", error = ex.InnerException?.Message ?? ex.Message });
            }
        }

        private decimal GetItemPrice(string itemType, int itemId)
        {
            switch (itemType)
            {
                case "product":
                    return _context.TProducts
                               .Where(p => p.FProductId == itemId)
                               .Select(p => (decimal?)p.FProductPrice) // 使用 decimal? 避免 null
                               .FirstOrDefault() ?? 0; // 如果 null，則預設回傳 0

                case "attractionTicket":
                    return _context.TAttractionTickets
                               .Where(t => t.FAttractionTicketId == itemId)
                               .Select(t => (decimal?)t.FPrice)
                               .FirstOrDefault() ?? 0;

                case "eventFee":
                    return 100;
                    //return _context.TEvents
                    //           .Where(e => e.FEventId == itemId)
                    //           .Select(e => (decimal?)e.FPrice)
                    //           .FirstOrDefault() ?? 0;

                default:
                    return 0; // 預設回傳 0 避免錯誤
            }
        }

        private bool TOrderExists(int id)
        {
            return _context.TOrders.Any(e => e.FOrderId == id);
        }

        //原始

        // GET: api/TOrders
        //[HttpGet]
        //public async Task<ActionResult<IEnumerable<TOrder>>> GetTOrders()
        //{
        //    return await _context.TOrders.ToListAsync();
        //}

        // GET: api/TOrders/5
        //[HttpGet("{id}")]
        //public async Task<ActionResult<TOrder>> GetTOrder(int id)
        //{
        //    var tOrder = await _context.TOrders.FindAsync(id);

        //    if (tOrder == null)
        //    {
        //        return NotFound();
        //    }

        //    return tOrder;
        //}


        // DELETE: api/TOrders/5
        //[HttpDelete("{id}")]
        //public async Task<IActionResult> DeleteTOrder(int id)
        //{
        //    var tOrder = await _context.TOrders.FindAsync(id);
        //    if (tOrder == null)
        //    {
        //        return NotFound();
        //    }

        //    _context.TOrders.Remove(tOrder);
        //    await _context.SaveChangesAsync();

        //    return NoContent();
        //}

    }
}
