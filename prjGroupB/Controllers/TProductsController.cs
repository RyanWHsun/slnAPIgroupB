using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Net.NetworkInformation;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using prjGroupB.DTO;
using prjGroupB.Models;

namespace prjGroupB.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TProductsController : ControllerBase
    {
        private readonly dbGroupBContext _context;

        public TProductsController(dbGroupBContext context)
        {
            _context = context;
        }

        [HttpGet("myProduct")]
        [Authorize]
        public async Task<IEnumerable<TProductListDTO>> GetMyProduct()
        {
            try
            {
                var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

                var myProduct = await _context.TProducts
                    .Where(p => p.FUserId == userId)
                    .Include(p => p.TProductImages)
                    .ToListAsync();

                var productListDTO = myProduct.Select(p => new TProductListDTO
                {
                    FProductId = p.FProductId,
                    FProductName = p.FProductName,
                    FIsOnSales = p.FIsOnSales,
                    FProductDateAdd = p.FProductDateAdd,
                    FProductUpdated = p.FProductUpdated,
                    FStock = p.FStock,
                    FProductPrice = p.FProductPrice,
                    FProductCategoryId = p.FProductCategoryId,
                    FSingleImage = p.TProductImages
                    .OrderBy(img => img.FProductImageId)
                    .Select(img => img.FImage)
                    .FirstOrDefault(img => img != null) is byte[] firstImage ? ConvertToThumbnailBase64(firstImage, 64, 64) : string.Empty // 只對第一張有效圖片進行轉換
                });
                return productListDTO;
            }
            catch (Exception ex) 
            {
                Console.WriteLine($"取得商品列表失敗: {ex.Message}");
                return new List<TProductListDTO>(); // 返回空列表，避免影響系統
            }
        }     

        

        //顯示所有商品
        // GET: api/TProducts
        [HttpGet]
        public async Task<IEnumerable<TProductAllDTO>> getAllProducts([FromQuery] int page = 1, [FromQuery] int pageSize = 6, [FromQuery] string? keyword = null, [FromQuery] int? categoryId = null)
        {
            try
            {
                var products = await _context.TProducts
                    .Include(p => p.FUser) // 載入會員導覽屬性
                    .Include(p => p.TProductImages) // 載入商品圖片導覽屬性
                    .Where(p => (bool)p.FIsOnSales) // 只篩選 FIsOnSales 為 true 的商品
                    .Where(p => string.IsNullOrEmpty(keyword) || p.FProductName.Contains(keyword) || p.FProductDescription.Contains(keyword)) // 搜尋關鍵字
                    .Where(p => !categoryId.HasValue || p.FProductCategoryId == categoryId) // 篩選類別
                    .OrderByDescending(p => (p.FProductUpdated ?? p.FProductDateAdd)) // 取較新的時間排序
                    .Skip((page - 1) * pageSize) // 跳過前面 (page - 1) * pageSize 筆數據
                    .Take(pageSize) // 取 pageSize 筆數據
                    .ToListAsync(); // 將查詢結果載入記憶體

                // 在記憶體中進行圖片處理
                var allProducts = products.Select(p => new TProductAllDTO
                {
                    FProductId = p.FProductId,
                    FProductCategoryId = p.FProductCategoryId,
                    FProductName = p.FProductName,
                    FProductPrice = p.FProductPrice,
                    FIsOnSales = p.FIsOnSales,
                    FStock = p.FStock,
                    FUserId = p.FUserId,
                    FUserNickName = p.FUser.FUserNickName,
                    FUserImage = p.FUser.FUserImage != null ? ConvertToThumbnailBase64(p.FUser.FUserImage, 64, 64) : null,
                    FImage = p.TProductImages
                                 .OrderBy(img => img.FProductImageId) // 確保順序
                                 .Select(img => img.FImage) // 只取得 byte[] 圖片資料
                                 .FirstOrDefault() is byte[] firstImage ? ConvertToThumbnailBase64(firstImage, 200, 200) : null
                });
                return allProducts;
            }
            catch(Exception ex)
            {
                Console.WriteLine($"取得所有商品時發生錯誤: {ex.Message}");
                return new List<TProductAllDTO>(); 
            }
        }

        private string ConvertToThumbnailBase64(byte[] fUserImage, int width, int height)
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
            catch (Exception ex) 
            {
                Console.WriteLine($"縮圖轉換失敗: {ex.Message}");
                return null;
            }
        }

        // GET: api/TProducts/5
        [HttpGet("myProductWithUserId")]
        [Authorize]
        public async Task<ActionResult<TProductDetailDTO>> GetTProductWithUserId(int id)
        {
            try
            {
                var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));//取得用戶ID
                var product = await _context.TProducts
                    .Include(p => p.TProductImages)
                    .FirstOrDefaultAsync(p => p.FProductId == id && p.FUserId == userId); //確保商品屬於該用戶

                if (product == null)
                {
                    return Unauthorized("您無權存取或變更此商品");
                }
                var productDTO = new TProductDetailDTO
                {
                    FProductId = product.FProductId,
                    FProductCategoryId = product.FProductCategoryId,
                    FProductName = product.FProductName,
                    FProductPrice = product.FProductPrice,
                    FProductDescription = product.FProductDescription,
                    FIsOnSales = product.FIsOnSales,
                    FStock = product.FStock,
                    FImage = product.TProductImages
                        .OrderBy(img => img.FProductImageId) // 確保圖片順序一致
                        .Select(img => Convert.ToBase64String(img.FImage)) // 將 byte[] 轉為 Base64
                        .ToArray() // 轉為陣列
                };
                return Ok(productDTO);
            }
            catch (Exception ex) 
            {
                Console.WriteLine($"取得商品時發生錯誤: {ex.Message}");
                return StatusCode(500, "伺服器錯誤，請稍後再試");
            }
        }

        // GET: api/TProducts/5
        [HttpGet("{id}")]
        public async Task<ActionResult<TProductDetailDTO>> GetTProduct(int id)
        {
            try
            {
                var productDetail = await _context.TProducts
                    .Include(p => p.TProductImages)
                    .FirstOrDefaultAsync(p => p.FProductId == id);

                if (productDetail == null)
                {
                    return null;
                }
                TProductDetailDTO productDetailDTO = new TProductDetailDTO
                {
                    FProductId = productDetail.FProductId,
                    FProductCategoryId = productDetail.FProductCategoryId,
                    FProductName = productDetail.FProductName,
                    FProductPrice = productDetail.FProductPrice,
                    FProductDescription = productDetail.FProductDescription,
                    FIsOnSales = productDetail.FIsOnSales,
                    FStock = productDetail.FStock,
                    FProductDateAdd = productDetail.FProductDateAdd,
                    FProductUpdated = productDetail.FProductUpdated,
                    FImage = productDetail.TProductImages
                        .OrderBy(img => img.FProductImageId) // 確保圖片順序一致
                        .Select(img => Convert.ToBase64String(img.FImage)) // 將 byte[] 轉為 Base64
                        .ToArray() // 轉為陣列
                };
                return Ok(productDetailDTO);
            }
            catch (Exception ex) 
            {
                Console.WriteLine($"取得商品時發生錯誤: {ex.Message}");
                return StatusCode(500, new { message = "伺服器錯誤，請稍後再試" }); // 確保回傳型別一致
            }

        }

        // PUT: api/TProducts/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutTProduct(int id,TProductDetailDTO productDetailDTO)
        {
            try
            {
                if (id != productDetailDTO.FProductId)
                {
                    return BadRequest(new { message = "商品修改失敗!" });
                }

                // 找到對應的商品
                TProduct? product = await _context.TProducts
                    .Include(p => p.TProductImages) // 確保載入圖片
                    .FirstOrDefaultAsync(p => p.FProductId == id);
                if (product == null)
                {
                    return NotFound(new { message = "商品不存在!" });
                }
                    product.FProductName=productDetailDTO.FProductName;
                    product.FProductPrice = productDetailDTO.FProductPrice;
                    product.FProductCategoryId= productDetailDTO.FProductCategoryId;
                    product.FProductDescription = productDetailDTO.FProductDescription;
                    product.FIsOnSales = productDetailDTO.FIsOnSales;
                    product.FStock = productDetailDTO.FStock;
                    product.FProductUpdated=DateTime.Now;
                    _context.Entry(product).State = EntityState.Modified;

                // 更新圖片邏輯
                if (productDetailDTO.FImage != null && productDetailDTO.FImage.Length > 0)
                {
                    var existingImage = product.TProductImages.OrderBy(img => img.FProductImageId).ToList();
                    for (int i = 0; i < productDetailDTO.FImage.Length; i++)
                    {
                        string base64Image = productDetailDTO.FImage[i];

                        if (!string.IsNullOrEmpty(base64Image))
                        {
                            byte[] imageBytes = Convert.FromBase64String(base64Image);
                        
                            // 取得現有圖片                       
                            if (i<existingImage.Count)
                            {
                                // 更新現有圖片
                                existingImage[i].FImage = Convert.FromBase64String(base64Image);
                                _context.Entry(existingImage[i]).State = EntityState.Modified;
                            }
                            else
                            {
                                // 新增新圖片
                                var newImage = new TProductImage
                                {
                                    FProductId = product.FProductId,
                                    FImage = imageBytes
                                };
                                _context.TProductImages.Add(newImage);
                            }
                        }
                    }
                    if (existingImage.Count > productDetailDTO.FImage.Length) 
                    { 
                        var imagesToRemove=existingImage.Skip(productDetailDTO.FImage.Length).ToList();
                        _context.TProductImages.RemoveRange(imagesToRemove);
                    }
                }
                await _context.SaveChangesAsync();
                return Ok(new { message = "商品修改成功!" });
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!TProductExists(id))
                {
                    return NotFound(new { message = "商品不存在!" });
                }
                else
                {
                    throw;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"修改商品時發生錯誤: {ex.Message}");
                return StatusCode(500, new { message = "伺服器錯誤，請稍後再試!" });
            }
        }


        [HttpPut("batchUpdateStatus")]
        [Authorize]
        public async Task<IActionResult> BatchUpdateStatus([FromBody] List<int> productIds)
        {
            try
            {
                if (productIds ==null || productIds.Count == 0)
                {
                    return BadRequest(new {message="未提供商品ID"});
                }
                //查找符合商品
                var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
                // 確保所有請求修改的產品都屬於當前使用者
                var userProducts = await _context.TProducts
                    .Where(p => productIds.Contains(p.FProductId) && p.FUserId==userId)
                    .ToListAsync();
                if (userProducts.Count != productIds.Count)
                {
                    return Unauthorized(new { message = "部分或全部商品無權修改" });
                }

                //切換狀態
                foreach (var product in userProducts) 
                {
                    product.FIsOnSales = !product.FIsOnSales;
                    _context.Entry(product).State = EntityState.Modified;
                }

                    await _context.SaveChangesAsync();
                    return Ok(new { message = "狀態更新成功!", updatedCount = userProducts.Count });
            }
            catch (DbUpdateException dbEx)
            {
                Console.WriteLine($"批次修改失敗 (資料庫異常): {dbEx.Message}");
                return StatusCode(500, new { message = "批次修改失敗! 請稍後再試。", error = "資料庫錯誤" });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"批次修改失敗: {ex.Message}");
                return StatusCode(500, new { message = "批次修改失敗! 發生未知錯誤，請稍後再試。", error = ex.Message });
            }
        }

        //取得最新商品
        //GET: api/TProducts/latest
        [HttpGet("latest")]
        public async Task<ActionResult<IEnumerable<TProductLatestDTO>>> GetLatestProducts()
        {
            try
            {
                var products = await _context.TProducts
                    .Include(p => p.TProductImages) // 載入商品圖片導覽屬性
                    .Where(p => p.FIsOnSales == true)
                    .OrderByDescending(p => p.FProductDateAdd)
                    .Take(4)
                    .ToListAsync();

                var latestProducts = products.Select(p => new TProductLatestDTO
                {
                    FProductId = p.FProductId,
                    FProductName = p.FProductName,
                    FProductDateAdd = p.FProductDateAdd,
                    FSingleImage = p.TProductImages
                                    .OrderBy(img => img.FProductImageId)
                                    .Select(img => img.FImage)
                                    .FirstOrDefault() is byte[] firstImage ? ConvertToThumbnailBase64(firstImage, 100, 100) : null
                });
                return Ok(latestProducts);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"取得最新商品時發生錯誤: {ex.Message}");
                return StatusCode(500, new { message = "伺服器錯誤，請稍後再試!" });
            }
        }


        // POST: api/TProducts
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> PostTProduct(TProductDetailDTO productDetailDTO)
        {
            try
            {
                // 取得目前登入的 UserId
                var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

                //建立商品
                TProduct product = new TProduct
                {
                    FUserId = userId,
                    FProductName = productDetailDTO.FProductName,
                    FProductCategoryId = productDetailDTO.FProductCategoryId,
                    FProductDescription = productDetailDTO.FProductDescription,
                    FProductPrice = productDetailDTO.FProductPrice,
                    FIsOnSales = productDetailDTO.FIsOnSales,
                    FProductDateAdd = DateTime.Now,
                    FProductUpdated = null,
                    FStock = productDetailDTO.FStock,
                };
                _context.TProducts.Add(product);
                await _context.SaveChangesAsync(); //先儲存以取得id

                //新增圖片
                if (productDetailDTO.FImage != null && productDetailDTO.FImage.Length > 0)
                {
                    foreach (var base64Image in productDetailDTO.FImage)
                    {
                        if (!string.IsNullOrEmpty(base64Image))
                        {
                            var productImage = new TProductImage
                            {
                                FProductId = product.FProductId,
                                FImage = Convert.FromBase64String(base64Image), // 轉換 Base64 為 byte[]
                            };
                            _context.TProductImages.Add(productImage);
                        }
                    }
                }
                await _context.SaveChangesAsync(); //儲存圖片
                return Ok(new { message = "商品新增成功!" });
            }
            catch (DbUpdateException dbEx)
            {
                Console.WriteLine($"商品新增失敗 (資料庫異常): {dbEx.Message}");
                return StatusCode(500, new { message = "商品新增失敗，請稍後再試!", error = "資料庫錯誤" });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"商品新增失敗: {ex.Message}");
                return StatusCode(500, new { message = "商品新增失敗，發生未知錯誤，請稍後再試!", error = ex.Message });
            }
        }

        // DELETE: api/TProducts/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTProduct(int id)
        {
            try
            {
                var product = await _context.TProducts
                .Include(p => p.TProductImages)
                .FirstOrDefaultAsync(p=>p.FProductId==id);

                if (product == null)
                {
                    return NotFound(new { message = "刪除商品失敗!" });
                }

                // 檢查是否有符合條件的訂單明細
                var orderDetails = await _context.TOrdersDetails
                    .Where(i => i.FItemType == "product" && i.FItemId == id)
                    .ToListAsync();

                if (orderDetails.Any())
                {
                    // 如果有符合條件的訂單明細，則不允許刪除
                    return BadRequest(new { message = "此商品已經被訂單使用，無法刪除,只可改為下架" });
                }

                if(product.TProductImages != null && product.TProductImages.Any())
                {
                    _context.TProductImages.RemoveRange(product.TProductImages); ;
                }
                _context.TProducts.Remove(product);
                await _context.SaveChangesAsync();
                return Ok(new { message = "刪除商品成功!" });
            }
            catch(DbUpdateException ex)
            {
                return BadRequest(new { message = "刪除商品失敗，可能因為與其他資料有關聯!", error = ex.Message });
            }
        }
      
        private bool TProductExists(int id)
        {
            return _context.TProducts.Any(e => e.FProductId == id);
        }
    }
}
