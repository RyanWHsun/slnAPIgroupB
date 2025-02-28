using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using prjGroupB.DTO;
using prjGroupB.Models;

namespace prjGroupB.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TAttractionImagesController : ControllerBase
    {
        private readonly dbGroupBContext _context;

        public TAttractionImagesController(dbGroupBContext context)
        {
            _context = context;
        }

        // GET: api/TAttractionImages
        [HttpGet]
        public async Task<ActionResult<IEnumerable<TAttractionImageDTO>>> GetTAttractionImages()
        {
            try
            {
                var attractionImageDTOs = await _context.TAttractionImages.Select(
                    attractionImage => new TAttractionImageDTO
                    {
                        FAttractionId = attractionImage.FAttractionId,
                        FAttractionName = attractionImage.FAttraction.FAttractionName,
                        FAttractionImageId = attractionImage.FAttractionImageId,
                        FImage = attractionImage.FImage
                    }).ToListAsync();
                return Ok(attractionImageDTOs);
            }
            catch (DbException ex)
            {
                // 資料庫連線錯誤
                return StatusCode(500, new { error = "資料庫連線錯誤", details = ex.Message });
            }
            catch (Exception ex)
            {
                // 一般錯誤
                return StatusCode(500, new { error = "內部錯誤", details = ex.Message });
            }
        }

        // GET: api/TAttractionImages/5
        // id is the attraction id
        [HttpGet("{id}")]
        public async Task<ActionResult<IEnumerable<TAttractionImageDTO>>> GetTAttractionImage(int id)
        {
            try
            {
                var attractionImages = await _context.TAttractionImages
                    .Include(image => image.FAttraction)
                    .Where(image => image.FAttractionId == id).ToListAsync();

                // .Any() 是 LINQ 的一個方法，檢查集合中是否存在至少一個元素。
                // 如果集合中有資料，.Any() 會回傳 true。
                // 如果集合為空，.Any() 會回傳 false。
                if (attractionImages == null || !attractionImages.Any())
                {
                    return NotFound(new { error = "找不到相關圖片" });
                }

                var attractionImageDTOs = attractionImages.Select(
                    attractionImage => new TAttractionImageDTO
                    {
                        FAttractionId = attractionImage.FAttractionId,
                        FAttractionName = attractionImage.FAttraction.FAttractionName,
                        FAttractionImageId = attractionImage.FAttractionImageId,
                        FImage = attractionImage.FImage
                    }).ToList();

                return Ok(attractionImageDTOs);
            }
            catch (DbException ex)
            {
                // 資料庫連線錯誤
                return StatusCode(500, new { error = "資料庫連線錯誤", details = ex.Message });
            }
            catch (Exception ex)
            {
                // 一般錯誤
                return StatusCode(500, new { error = "內部錯誤", details = ex.Message });
            }
        }

        // GET: api/TAttractionImages/Search?id=5&amount=1
        // 根據 attraction ID 取得圖片(可選張數)
        [HttpGet("Search")]
        public async Task<ActionResult<TAttractionImageDTO>> GetOneTAttractionImage(int id, int amount = 1)
        {
            try
            {
                var attractionImage = await _context.TAttractionImages
                    .Include(image => image.FAttraction)
                    .Where(image => image.FAttractionId == id)
                    .FirstOrDefaultAsync();

                // .Any() 是 LINQ 的一個方法，檢查集合中是否存在至少一個元素。
                // 如果集合中有資料，.Any() 會回傳 true。
                // 如果集合為空，.Any() 會回傳 false。
                if (attractionImage == null)
                {
                    return NotFound(new { error = "找不到相關圖片" });
                }

                var attractionImageDTO = new TAttractionImageDTO
                {
                    FAttractionId = attractionImage.FAttractionId,
                    FAttractionName = attractionImage.FAttraction.FAttractionName,
                    FAttractionImageId = attractionImage.FAttractionImageId,
                    FImage = attractionImage.FImage
                };

                return Ok(attractionImageDTO);
            }
            catch (DbException ex)
            {
                // 資料庫連線錯誤
                return StatusCode(500, new { error = "資料庫連線錯誤", details = ex.Message });
            }
            catch (Exception ex)
            {
                // 一般錯誤
                return StatusCode(500, new { error = "內部錯誤", details = ex.Message });
            }
        }

        // POST: api/TAttractionImages
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<IEnumerable<TAttractionImageDTO>>> PostTAttractionImage([FromForm] int fAttractionId, [FromForm] List<IFormFile> fImages)
        {
            try
            {
                if (fImages == null || fImages.Count == 0)
                {
                    return BadRequest(new { error = "未選擇任何圖片" });
                }

                var uploadedImages = new List<TAttractionImageDTO>();

                foreach (var image in fImages)
                {
                    using var memoryStream = new MemoryStream();
                    await image.CopyToAsync(memoryStream);
                    var imageData = memoryStream.ToArray();

                    TAttractionImage attractionImage = new TAttractionImage
                    {
                        FAttractionId = fAttractionId,
                        FImage = imageData
                    };

                    // 1. 新的記錄插入資料庫。
                    // 2. 資料庫生成並返回新的 FAttractionImageId。
                    // 3. EF 將新生成的 ID 更新到 attractionImage.FAttractionImageId。
                    _context.TAttractionImages.Add(attractionImage);
                    await _context.SaveChangesAsync();

                    uploadedImages.Add(new TAttractionImageDTO
                    {
                        FAttractionId = attractionImage.FAttractionId,
                        FAttractionName = "",
                        FAttractionImageId = attractionImage.FAttractionImageId,
                        FImage = attractionImage.FImage
                    });
                }

                return Ok(uploadedImages);
            }
            catch (DbUpdateException ex)
            {
                // 資料庫更新錯誤
                return StatusCode(500, new { error = "資料庫更新錯誤", details = ex.Message });
            }
            catch (Exception ex)
            {
                // 一般錯誤
                return StatusCode(500, new { error = "內部錯誤", details = ex.Message });
            }
        }

        // DELETE: api/TAttractionImages/5
        // id is the attraction id
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTAttractionImage(int id)
        {
            try
            {
                var attractionImages = await _context.TAttractionImages
                    .Include(image => image.FAttraction)
                    .Where(image => image.FAttractionId == id).ToListAsync();

                if (attractionImages == null || !attractionImages.Any())
                {
                    return NotFound(new { error = "找不到相關圖片" });
                }

                // 刪除所有查詢到的圖片
                _context.TAttractionImages.RemoveRange(attractionImages);
                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (DbUpdateException ex)
            {
                // 資料庫更新錯誤
                return StatusCode(500, new { error = "資料庫更新錯誤", details = ex.Message });
            }
            catch (Exception ex)
            {
                // 一般錯誤
                return StatusCode(500, new { error = "內部錯誤", details = ex.Message });
            }
        }

        private bool TAttractionImageExists(int id)
        {
            return _context.TAttractionImages.Any(e => e.FAttractionImageId == id);
        }
    }
}
