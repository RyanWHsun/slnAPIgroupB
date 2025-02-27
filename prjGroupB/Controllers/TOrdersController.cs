using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using prjGroupB.DTO;
using prjGroupB.Models;
using QRCoder;
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
            Console.WriteLine("🚀 進入 Checkout API"); // ✅ 記錄 API 被呼叫
            Console.WriteLine($"Request Body: {System.Text.Json.JsonSerializer.Serialize(checkoutRequest)}"); // ✅ 記錄請求內容
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

                        // 如果庫存為 0，將商品設為下架
                        if (product.FStock <= 0)
                        {
                            product.FIsOnSales = false;
                        }
                        _context.TProducts.Update(product);
                    }
                }
                await _context.SaveChangesAsync(); //存檔一次以防有商品要修改狀態
                var createdOrders = new List<TOrder>();

                //分組處理商品(依sellerId分組)
                var productOrders = checkoutRequest.SelectedItems
                    .Where(item => item.FItemType == "product")
                    .GroupBy(item => item.FSellerId)
                    .ToList();

                //依活動類別分組
                var eventOrders = checkoutRequest.SelectedItems
                    .Where(item => item.FItemType == "eventFee")
                    .GroupBy(item => item.FSellerId)
                    .ToList();

                //票券依類別分組
                var attractionOrders = checkoutRequest.SelectedItems
                    .Where(item=>item.FItemType == "attractionTicket")
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
                            FTimestamp = DateTime.Now
                        };
                        _context.TOrderStatusHistories.Add(orderHistory);

                        //錢包扣款
                        if (checkoutRequest.FPaymentMethod == "Wallet")
                        {
                            var walletTransaction = new TWallet
                            {
                                FUserId = buyerId,
                                FAmountChange = (int)(-orderTotal),// 扣款，確保轉換為 int
                                FChangeLog = $"付款：訂單編號#{order.FOrderId}",
                                FChangeTime = DateTime.Now,
                            };
                            _context.TWallets.Add(walletTransaction);
                        }

                        createdOrders.Add(order);
                    }
                }

                //處理活動
                if (eventOrders.Any()) 
                {
                    foreach (var group in eventOrders)
                    {
                        var eventItem = group.FirstOrDefault();

                        if (eventItem != null) 
                        {
                            var newEvent = new TEventRegistrationForm
                            {
                                FUserId = buyerId,
                                FEventId = eventItem.FItemId,
                                FEregistrationDate = DateTime.Now,
                                FRegistrationStatus = "pending",
                            };
                            _context.TEventRegistrationForms.Add(newEvent);
                            await _context.SaveChangesAsync();
                            var orderTotal = 0m;

                            foreach (var item in group)
                            {
                                var unitPrice = GetItemPrice(item.FItemType, item.FItemId);
                                orderTotal += unitPrice * 1;

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
                                    FChangeLog = $"付款：報名編號{newEvent.FEventRegistrationFormId}",
                                    FChangeTime = DateTime.Now
                                };
                                _context.TWallets.Add(walletTransaction);
                            }
                        }                         
                    }
                }


                //處理活動
                if (attractionOrders.Any())
                {
                    foreach (var group in attractionOrders)
                    {
                        var attractionItem = group.FirstOrDefault();

                        if (attractionItem != null)
                        {
                            var unitPrice = GetItemPrice(attractionItem.FItemType, attractionItem.FItemId);
                            var newTicket = new TAttractionTicketOrder
                            {
                                FBuyerId=buyerId,
                                FAttractionTicketId=attractionItem.FItemId,
                                FCreatedDate=DateTime.Now,
                                FOrderQty=attractionItem.FQuantity,
                                FUnitPrice=unitPrice                            
                            };
                            _context.TAttractionTicketOrders.Add(newTicket);
                            await _context.SaveChangesAsync();
                            var orderTotal = 0m;

                            foreach (var item in group)
                            {
                                orderTotal += unitPrice * newTicket.FOrderQty.GetValueOrDefault();

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
                                    FChangeLog = $"付款：票券報名編號{newTicket.FAttractionTicketOrderId}",
                                    FChangeTime = DateTime.Now
                                };
                                _context.TWallets.Add(walletTransaction);
                            }
                        }
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
            try
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
                        return _context.TEvents
                                   .Where(e => e.FEventId == itemId)
                                   .Select(e => (decimal?)e.FEventFee)
                                   .FirstOrDefault() ?? 0;

                    default:
                        return 0; // 預設回傳 0 避免錯誤
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine("產生價格有誤", ex.Message);
                return 0;
            }

        }

        //取得買家所有訂單
        //GET: api/TOrders/getBuyerOrder
        [HttpGet("getBuyerOrder")]
        [Authorize]
        public async Task<ActionResult<TOrderBuyerAllDTO>> GetBuyerOrders()
        {
            try
            {
                var buyerId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
                //var buyerId = 3004;

                var orders = await (from o in _context.TOrders
                                    where o.FBuyerId == buyerId
                                    join os in _context.TOrderStatuses on o.FOrderStatusId equals os.FOrderStatusId
                                    join od in _context.TOrdersDetails on o.FOrderId equals od.FOrderId
                                    join p in _context.TProducts on od.FItemId equals p.FProductId
                                    join u in _context.TUsers on p.FUserId equals u.FUserId
                                    group new { o, os, od, p, u } by new
                                    {
                                        o.FOrderId,
                                        o.FOrderStatusId,
                                        o.FExtraInfo,
                                        os.FStatusName,
                                        o.FShipAddress,
                                        o.FOrderDate
                                    } into g
                                    select new TOrderBuyerAllDTO
                                    {
                                        FOrderId = g.Key.FOrderId,
                                        FOrderStatusId = g.Key.FOrderStatusId,
                                        FStatusName = g.Key.FStatusName,
                                        FExtraInfo = g.Key.FExtraInfo,
                                        FShipAddress = g.Key.FShipAddress,
                                        FOrderDate = g.Key.FOrderDate,
                                        FOrderAmount = g.Sum(x => (int)(x.od.FUnitPrice * x.od.FOrderQty)),
                                        SellerName = g.Select(x => x.u.FUserNickName).FirstOrDefault(),
                                        FProductName = g.Select(x => x.p.FProductName).ToList()
                                    }).ToListAsync();

                if (orders == null || orders.Count == 0)
                {
                    return NotFound(new { message = "尚無訂單，快去消費吧!" });
                }
                return Ok(orders);
            }
            catch (Exception ex) 
            {
                return StatusCode(500, new { message = "獲取訂單時發生錯誤", error = ex.Message });
            }
        }

        //取得買家訂單明細
        //GET: api/TOrders/details/{orderId}
        [HttpGet("details/{orderId}")]
        public async Task<ActionResult<TOrderDetailForBuyerDTO>> GetOrderDetails(int orderId)
        {
            try
            {
                var orderDetails = await (from od in _context.TOrdersDetails
                                          where od.FOrderId == orderId
                                          join p in _context.TProducts on od.FItemId equals p.FProductId
                                          select new TOrderDetailDTO
                                          {
                                              FOrderDetailsId = od.FOrderDetailsId,
                                              FItemId = p.FProductId,
                                              FOrderQty = od.FOrderQty ?? 0,
                                              FUnitPrice = od.FUnitPrice ?? 0,
                                              FProductName = p.FProductName
                                          }).ToListAsync();

                //處理圖片
                foreach (var detail in orderDetails)
                {
                    var productImage = await _context.TProductImages
                        .Where(img => img.FProductId == detail.FItemId)
                        .OrderBy(img => img.FProductId)
                        .Select(img => img.FImage)
                        .FirstOrDefaultAsync();

                    if (productImage != null)
                    {
                        detail.FProductImage = ConvertToThumbnailBase64(productImage, 100, 100);
                    }
                }

                var statusHistory = await _context.TOrderStatusHistories
                    .Where(h => h.FOrderId == orderId)
                    .OrderBy(h => h.FTimestamp)
                    .Select(h => new TOrderStatusHistoryDTO
                    {
                        FOrderStatusId = h.FOrderStatusId,
                        FStatusName = h.FStatusName,
                        FTimestamp = h.FTimestamp
                    }).ToListAsync();

                return Ok(new TOrderDetailForBuyerDTO
                {
                    OrderDetails = orderDetails,
                    StatusHistory = statusHistory,
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "獲取訂單詳情時發生錯誤", error = ex.Message });
            }
        }

        private static string ConvertToThumbnailBase64(byte[] fUserImage, int width, int height)
        {
            try
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
            catch(Exception ex)
            {
                Console.WriteLine($"縮圖轉換失敗: {ex.Message}");
                return null;
            }
        }

        //取得賣家所有訂單
        //GET: api/TOrders/getSellerOrder
        [HttpGet("getSellerOrder")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<TOrderSellerAllDTO>>> GetSellerOrders()
        {
            try
            {
                var sellerId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
                //var sellerId = 3009; //測試用

                var orders = await (from od in _context.TOrdersDetails
                                    join p in _context.TProducts on od.FItemId equals p.FProductId
                                    where p.FUserId == sellerId //只篩選該賣家
                                    join o in _context.TOrders on od.FOrderId equals o.FOrderId
                                    join os in _context.TOrderStatuses on o.FOrderStatusId equals os.FOrderStatusId
                                    join u in _context.TUsers on o.FBuyerId equals u.FUserId
                                    group new { o, os, od, u } by new
                                    {
                                        o.FOrderId,
                                        o.FOrderStatusId,
                                        o.FExtraInfo,
                                        os.FStatusName,
                                        o.FShipAddress,
                                        o.FOrderDate,
                                        u.FUserName
                                    } into g
                                    select new TOrderSellerAllDTO
                                    {
                                        FOrderId = g.Key.FOrderId,
                                        FOrderStatusId = g.Key.FOrderStatusId,
                                        FStatusName = g.Key.FStatusName,
                                        FShipAddress = g.Key.FShipAddress,
                                        FExtraInfo = g.Key.FExtraInfo,
                                        FOrderDate = g.Key.FOrderDate,
                                        FOrderAmount = g.Sum(x => (int)(x.od.FUnitPrice * x.od.FOrderQty)),
                                        BuyerName = g.Key.FUserName,
                                        StatusHistory = (from h in _context.TOrderStatusHistories
                                                         where h.FOrderId == g.Key.FOrderId
                                                         select new TOrderStatusHistoryDTO
                                                         {
                                                             FOrderStatusId = h.FOrderStatusId,
                                                             FStatusName = h.FStatusName,
                                                             FTimestamp = h.FTimestamp
                                                         }).ToList()
                                    }).ToListAsync();

                if (orders == null || !orders.Any())
                {
                    return NotFound(new { message = "尚無銷售訂單" });
                }
                return Ok(orders);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "獲取銷售訂單時發生錯誤", error = ex.Message });
            }
        }

        //賣家更新訂單狀態
        //PUT :api/TOrders/shipOrder/{orderId}
        [HttpPut("shipOrder/{orderId}")]
        public async Task<IActionResult> ShipOrder(int orderId, [FromBody] ShipOrderDTO shipOrderDTO)
        {
            try
            {   //找訂單
                var order = await _context.TOrders.FindAsync(orderId);
                if (order == null)
                {
                    return NotFound(new { message = "訂單不存在" });
                }
                //確定訂單狀態是1
                if (order.FOrderStatusId == 1)
                {
                    order.FOrderStatusId = 2; //更新為待收貨
                                          //新增狀態歷史紀錄
                    var statusHistory = new TOrderStatusHistory
                    {
                        FOrderId = orderId,
                        FOrderStatusId = 2,
                        FStatusName = _context.TOrderStatuses.FirstOrDefault(s => s.FOrderStatusId == 2).FStatusName,
                        FTimestamp = DateTime.Now
                    };
                    _context.TOrderStatusHistories.Add(statusHistory);
                }
                order.FExtraInfo = shipOrderDTO.extraInfo;

                await _context.SaveChangesAsync();
                return Ok(new { message = "訂單狀態已更新" }); 
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "更新訂單時發生錯誤", error = ex.Message });
            }
        }


        //賣家更新訂單BY QRcode 
        //PUT :api/TOrders/shipOrder/{orderId}
        [HttpGet("shipOrderByQR/{orderId}")]
        [EnableCors("AllowWebSite")]
        public async Task<IActionResult> ShipOrderByQR(int orderId, [FromServices] IHubContext<OrderHub> hubContext)
        {
            try
            {   //找訂單
                var order = await _context.TOrders.FindAsync(orderId);
                if (order == null)
                {
                    return NotFound(new { message = "訂單不存在" });
                }
                if (order.FOrderStatusId != 1)
                {
                    return Ok(new { message = "訂單已出貨" });
                }
                //確定訂單狀態是1
                if (order.FOrderStatusId == 1)
                {
                    order.FOrderStatusId = 2; //更新為待收貨
                                              //新增狀態歷史紀錄

                    //自動產生快遞單號
                    order.FExtraInfo = "537快遞： #" + new Random().Next(1000000, 9999999).ToString();
                    var statusHistory = new TOrderStatusHistory
                    {
                        FOrderId = orderId,
                        FOrderStatusId = 2,
                        FStatusName = _context.TOrderStatuses.FirstOrDefault(s => s.FOrderStatusId == 2).FStatusName,
                        FTimestamp = DateTime.Now
                    };
                    _context.TOrderStatusHistories.Add(statusHistory);
                    _context.TOrderStatusHistories.Add(statusHistory);
                }                
                await _context.SaveChangesAsync();
                await hubContext.Clients.All.SendAsync("OrderUpdated", orderId);
                return Ok(new { message = "訂單狀態已更新" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "更新訂單時發生錯誤", error = ex.Message });
            }
        }

        //中介程式GET
        //[HttpGet("webhook/shipOrder/{orderId}")]
        //[EnableCors("AllowQRScan")]
        //public async Task<IActionResult> WebhookShipOrder(int orderId, [FromServices] IHubContext<OrderHub> hubContext)
        //{
        //    try
        //    {
        //        Console.WriteLine($"Webhook 被觸發，訂單 ID: {orderId}");
        //        using (var handler = new HttpClientHandler() { AllowAutoRedirect = true })
        //        using (var client = new HttpClient())
        //        {
        //            string apiUrl = $"https://localhost:7112/api/TOrders/shipOrderByQR/{orderId}";

        //            // 透過 `PUT` 請求更新訂單狀態
        //            var response = await client.PutAsync(apiUrl, null);

        //            if (response.IsSuccessStatusCode)
        //            {
        //                // 使用 SignalR 通知前端 (sellerOrder.component)
        //                await hubContext.Clients.All.SendAsync("OrderUpdated", orderId);
        //                return Ok(new { message = $"訂單 {orderId} 已成功更新！" });
        //            }
        //            else
        //            {
        //                return StatusCode((int)response.StatusCode, new { message = "訂單更新失敗", error = await response.Content.ReadAsStringAsync() });
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        return StatusCode(500, new { message = "內部錯誤", error = ex.Message });
        //    }
        //}


        //買家更新訂單
        //PUT :api/TOrders/buyerUpdateAddress/{orderId}
        [HttpPut("buyerUpdateAddress/{orderId}")]
        [EnableCors("AllowWebSite")]
        public async Task<IActionResult> BuyerUpdateAddress(int orderId, [FromBody] BuyerUpdateDTO? buyerUpdate)
        {
            try
            {
                // 檢查 buyerUpdate 是否為 null
                if (buyerUpdate == null || string.IsNullOrEmpty(buyerUpdate.FShipAddress))
                {
                    return BadRequest(new { message = "請提供有效的地址資訊" });
                }
                //找訂單
                var order = await _context.TOrders.FindAsync(orderId);
                if (order == null)
                {
                    return NotFound(new { message = "訂單不存在" });
                }
                //確定訂單狀態為1
                if (order.FOrderStatusId != 1)
                {
                    return BadRequest(new { message = "此訂單地址無法進行更新" });
                }
                //更新地址
                order.FShipAddress = buyerUpdate.FShipAddress;
                await _context.SaveChangesAsync();
                return Ok(new { message="地址已變更完畢!" });

            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "更新訂單時發生錯誤", error = ex.Message });
            }
        }

        //買家完成訂單
        //PUT :api/TOrders/completeOrder/{orderId}
        [HttpPut("completeOrder/{orderId}")]
        public async Task<IActionResult> CompleteOrder(int orderId)
        {
            using var transaction = await _context.Database.BeginTransactionAsync(); //開始交易
            try
            {
                //找訂單
                var order = await _context.TOrders.FindAsync(orderId);
                if (order == null)
                {
                    return NotFound(new { message = "訂單不存在" });
                }

                if (order.FOrderStatusId == 2)
                {
                    order.FOrderStatusId = 3;  //訂單改為3已完成
                    var statusHistory = new TOrderStatusHistory //新增狀態紀錄
                    {
                        FOrderId = orderId,
                        FOrderStatusId = 3,
                        FStatusName = _context.TOrderStatuses.FirstOrDefault(s => s.FOrderStatusId == 3).FStatusName,
                        FTimestamp = DateTime.Now,
                    };
                    _context.TOrderStatusHistories.Add(statusHistory);

                    //計算訂單金額
                    var orderDetails = await _context.TOrdersDetails
                        .Where(d => d.FOrderId == orderId && d.FItemType == "product")
                        .ToListAsync();

                    decimal totalAmount = orderDetails.Sum(d => (d.FUnitPrice ?? 0) * (d.FOrderQty ?? 0));
                    decimal sellerAmount = totalAmount * 0.98m; //扣除手續費2%

                    //找商品Id
                    var productId = orderDetails.FirstOrDefault()?.FItemId;
                    if (productId == null)
                    {
                        await transaction.RollbackAsync();
                        return BadRequest(new { message = "Error456:訂單更新失敗，請洽客服" });
                    }

                    //賣家Id
                    var sellerId =await _context.TProducts
                        .Where(p=>p.FProductId == productId)
                        .Select(p => p.FUserId)
                        .FirstOrDefaultAsync();
                    if (sellerId == null) 
                    {
                        await transaction.RollbackAsync();
                        return BadRequest(new { message = "Error789:訂單更新失敗，請洽客服" });
                    }

                    var walletTransaction = new TWallet
                    {
                        FUserId = sellerId.Value,
                        FAmountChange = (int)(sellerAmount),
                        FChangeLog = $"收款：訂單編號#{orderId}，扣除2%手續費後金額 {(int)sellerAmount} 元",
                        FChangeTime = DateTime.Now,
                    };
                    _context.TWallets.Add(walletTransaction);
                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();
                    return Ok(new { message = "訂單已完成,感謝購買!" });
                }
                else
                {
                    // 如果訂單狀態既不是1也不是2，回傳一個預設的訊息
                    return BadRequest(new { message = "此訂單狀態無法進行更新" });
                }
            }
            catch (Exception ex)
            {
                // 若出現錯誤，回滾交易
                await transaction.RollbackAsync();
                return StatusCode(500, new { message = "更新訂單時發生錯誤，請洽客服。", error = ex.Message });
            }
        }


        //產生QRcode
        [HttpGet("generateQR/{orderId}")]
        public async Task<IActionResult> generateQRCode (int orderId)
        {
            try
            {
                //QR內容是呼叫API的URL
                string qrText = $"https://special-publicly-humpback.ngrok-free.app/api/TOrders/shipOrderByQR/{orderId}";            

                // 生成 QR Code
                QRCodeGenerator qrGenerator = new QRCodeGenerator();
                QRCodeData qRCodeData = qrGenerator.CreateQrCode(qrText, QRCodeGenerator.ECCLevel.Q);
                QRCode qrCode = new QRCode(qRCodeData);

              
                using (Bitmap qrBitmap = qrCode.GetGraphic(20))
                {
                    using (MemoryStream ms = new MemoryStream())
                    {
                        await Task.Run(() => qrBitmap.Save(ms, ImageFormat.Png));
                        return File(ms.ToArray(), "image/png");
                    }
                }
            }catch(Exception ex)
            {
                return StatusCode(500, new { message = "生成 QR Code 失敗", error = ex.Message });
            }
            
        }


        private bool TOrderExists(int id)
        {
            return _context.TOrders.Any(e => e.FOrderId == id);
        }
    }
}