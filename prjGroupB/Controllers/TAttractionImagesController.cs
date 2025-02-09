using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using prjGroupB.DTO;
using prjGroupB.Models;

namespace prjGroupB.Controllers {
    [Route("api/[controller]")]
    [ApiController]
    public class TAttractionImagesController : ControllerBase {
        private readonly dbGroupBContext _context;

        public TAttractionImagesController(dbGroupBContext context) {
            _context = context;
        }

        // GET: api/TAttractionImages
        [HttpGet]
        public async Task<IEnumerable<TAttractionImageDTO>> GetTAttractionImages() {
            var attractionImageDTOs = await _context.TAttractionImages.Select(
                attractionImage => new TAttractionImageDTO {
                    FAttractionId = attractionImage.FAttractionId,
                    FAttractionName = attractionImage.FAttraction.FAttractionName,
                    FAttractionImageId = attractionImage.FAttractionImageId,
                    FImage = attractionImage.FImage
                    //FImage = Convert.ToBase64String(attractionImage.FImage)
                }).ToListAsync();
            return attractionImageDTOs;
        }

        // GET: api/TAttractionImages/5
        // id is the attraction id
        [HttpGet("{id}")]
        public async Task<IEnumerable<TAttractionImageDTO>> GetTAttractionImage(int id) {
            var attractionImages = await _context.TAttractionImages
                .Include(image => image.FAttraction)
                .Where(image => image.FAttractionId == id).ToListAsync();

            // .Any() 是 LINQ 的一個方法，檢查集合中是否存在至少一個元素。
            // 如果集合中有資料，.Any() 會回傳 true。
            // 如果集合為空，.Any() 會回傳 false。
            if (attractionImages == null || !attractionImages.Any()) {
                return new List<TAttractionImageDTO>();
            }

            var attractionImageDTOs = attractionImages.Select(
                attractionImage => new TAttractionImageDTO {
                    FAttractionId = attractionImage.FAttractionId,
                    FAttractionName = attractionImage.FAttraction.FAttractionName,
                    FAttractionImageId = attractionImage.FAttractionImageId,
                    FImage = attractionImage.FImage
                    //FImage = Convert.ToBase64String(attractionImage.FImage) // 轉換 byte[] 為 Base64 字串
                }
            ).ToList();

            return attractionImageDTOs;
        }

        // GET: api/TAttractionImages/Search?id=5&amount=1
        [HttpGet("Search")]
        public async Task<TAttractionImageDTO> GetOneTAttractionImage(int id, int amount = 1) {
            var attractionImage = await _context.TAttractionImages.Include(image=>image.FAttraction).Where(image => image.FAttractionId == id).FirstOrDefaultAsync();
            // .Any() 是 LINQ 的一個方法，檢查集合中是否存在至少一個元素。
            // 如果集合中有資料，.Any() 會回傳 true。
            // 如果集合為空，.Any() 會回傳 false。
            if (attractionImage == null) {
                return null;
            }

            var attractionImageDTO = new TAttractionImageDTO {
                FAttractionId = attractionImage.FAttractionId,
                FAttractionName = attractionImage.FAttraction.FAttractionName,
                FAttractionImageId = attractionImage.FAttractionImageId,
                FImage = attractionImage.FImage
            };

            return attractionImageDTO;
        }

        // PUT: api/TAttractionImages/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        //[HttpPut("{id}")]
        //public async Task<IActionResult> PutTAttractionImage(int id, TAttractionImage tAttractionImage) {
        //    if (id != tAttractionImage.FAttractionImageId) {
        //        return BadRequest();
        //    }

        //    _context.Entry(tAttractionImage).State = EntityState.Modified;

        //    try {
        //        await _context.SaveChangesAsync();
        //    }
        //    catch (DbUpdateConcurrencyException) {
        //        if (!TAttractionImageExists(id)) {
        //            return NotFound();
        //        }
        //        else {
        //            throw;
        //        }
        //    }

        //    return NoContent();
        //}

        // POST: api/TAttractionImages
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<IActionResult> PostTAttractionImage([FromForm] int fAttractionId, [FromForm] List<IFormFile> fImages) {
            if (fImages == null || fImages.Count == 0) {
                return BadRequest("未選擇任何圖片");
            }

            List<TAttractionImageDTO> uploadedImages = new List<TAttractionImageDTO>();

            foreach (var image in fImages) {
                using var memoryStream = new MemoryStream();
                await image.CopyToAsync(memoryStream);
                var imageData = memoryStream.ToArray();

                TAttractionImage attractionImage = new TAttractionImage {
                    FAttractionId = fAttractionId,
                    FImage = imageData
                };

                // 1. 新的記錄插入資料庫。
                // 2. 資料庫生成並返回新的 FAttractionImageId。
                // 3. EF 將新生成的 ID 更新到 attractionImage.FAttractionImageId。
                _context.TAttractionImages.Add(attractionImage);
                await _context.SaveChangesAsync();

                uploadedImages.Add(new TAttractionImageDTO {
                    FAttractionId = attractionImage.FAttractionId,
                    FAttractionName = "",
                    FAttractionImageId = attractionImage.FAttractionImageId,
                    FImage = attractionImage.FImage
                });
            }
            return Ok(uploadedImages);
        }

        // DELETE: api/TAttractionImages/5
        // id is the attraction id
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTAttractionImage(int id) {
            var attractionImages = await _context.TAttractionImages
                .Include(image => image.FAttraction)
                .Where(image => image.FAttractionId == id).ToListAsync();

            if (attractionImages == null || !attractionImages.Any()) {
                return NotFound();
            }

            // 刪除所有查詢到的圖片
            _context.TAttractionImages.RemoveRange(attractionImages);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool TAttractionImageExists(int id) {
            return _context.TAttractionImages.Any(e => e.FAttractionImageId == id);
        }
    }
}
