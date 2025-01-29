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
        public async Task<IEnumerable<TProductDetailDTO>> GetMyProduct()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            var myProduct = await _context.TProducts
                .Where(p => p.FUserId == userId)
                .Include(p=>p.TProductImages)
                .ToListAsync();

            var productDTO = myProduct.Select(p => new TProductDetailDTO
            {

                FProductName = p.FProductName,
                FIsOnSales = p.FIsOnSales,
                FProductDateAdd = p.FProductDateAdd,
                FProductUpdated = p.FProductUpdated,
                FStock = p.FStock,
                FProductPrice = p.FProductPrice,
                FImage = p.TProductImages
                .OrderBy(img => img.FProductImageId)
                .Take(1)
                .Select(img => img.FImage != null ? ConvertToThumbnailBase64(img.FImage, 64, 64) : string.Empty)
                .Where(base64 => !string.IsNullOrEmpty(base64)) // 過濾掉空字串
                .ToArray()
            });
            return productDTO;
        }
      

        

        //顯示所有商品
        // GET: api/TProducts
        [HttpGet]
        public async Task<IEnumerable<TProductAllDTO>> getAllProducts([FromQuery] int page = 1, [FromQuery] int pageSize = 6, [FromQuery] string? keyword = null, [FromQuery] int? categoryId = null)
        {
            var products = await _context.TProducts
                .Include(p => p.FUser) // 載入會員導覽屬性
                .Include(p => p.TProductImages) // 載入商品圖片導覽屬性
                .Where(p => (bool)p.FIsOnSales) // 只篩選 FIsOnSales 為 true 的商品
                .Where(p => string.IsNullOrEmpty(keyword) || p.FProductName.Contains(keyword) || p.FProductDescription.Contains(keyword)) // 搜尋關鍵字
                .Where(p => !categoryId.HasValue || p.FProductCategoryId == categoryId) // 篩選類別
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
                            .OrderBy(img => img.FProductImageId)
                            .Select(img => img.FImage != null ? Convert.ToBase64String(img.FImage) : null) // 將 byte[] 轉為 Base64
                            .FirstOrDefault()// 取第一張圖片
            });

            return allProducts;
        }

        private string ConvertToThumbnailBase64(byte[] fUserImage, int width, int height)
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
        //將原始的 byte[] 圖片縮小到指定尺寸，然後轉換為 Base64 格式。整體流程總結:
        //1.將圖片數據（byte[]）載入 MemoryStream。
        //2.通過 Image.FromStream 將數據轉為圖片對象。
        //3.使用 GetThumbnailImage 生成縮圖。
        //4.將縮圖保存到內存流（PNG 格式）。
        //5.將內存流轉換為 Base64 字串並返回。

        // GET: api/TProducts/5
        [HttpGet("{id}")]
        public async Task<ActionResult<TProductDetailDTO>> GetTProductDetail(int id)
        {
            var productDetail = await _context.TProducts
                .Include(p => p.TProductImages)
                .FirstOrDefaultAsync(p=>p.FProductId == id);

            if (productDetail == null)
            {
                return NotFound();
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


        // PUT: api/TProducts/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutTProduct(int id, TProduct tProduct)
        {
            if (id != tProduct.FProductId)
            {
                return BadRequest();
            }

            _context.Entry(tProduct).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!TProductExists(id))
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

        // POST: api/TProducts
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<TProduct>> PostTProduct(TProduct tProduct)
        {
            _context.TProducts.Add(tProduct);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetTProduct", new { id = tProduct.FProductId }, tProduct);
        }

        // DELETE: api/TProducts/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTProduct(int id)
        {
            var tProduct = await _context.TProducts.FindAsync(id);
            if (tProduct == null)
            {
                return NotFound();
            }

            _context.TProducts.Remove(tProduct);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool TProductExists(int id)
        {
            return _context.TProducts.Any(e => e.FProductId == id);
        }
    }
}
