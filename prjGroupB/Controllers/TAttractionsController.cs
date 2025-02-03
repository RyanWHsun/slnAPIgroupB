using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net.NetworkInformation;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using prjGroupB.DTO;
using prjGroupB.Models;

namespace prjGroupB.Controllers {
    // route: api/TAttractions
    [Route("api/[controller]")]
    [ApiController]
    public class TAttractionsController : ControllerBase {
        private readonly dbGroupBContext _context;

        public TAttractionsController(dbGroupBContext context) {
            _context = context;
        }

        // GET: api/TAttractions
        [HttpGet]
        public async Task<IEnumerable<TAttractionDTO>> GetTAttractions() {
            var attractionDTOs = await _context.TAttractions.Select(
                    attraction => new TAttractionDTO {
                        FAttractionId = attraction.FAttractionId,
                        FAttractionName = attraction.FAttractionName,
                        FCategoryId = attraction.FCategoryId,
                        FCategoryName = attraction.FCategory.FAttractionCategoryName,
                        FDescription = attraction.FDescription,
                        FRegion = attraction.FRegion,
                        FAddress = attraction.FAddress,
                        FStatus = attraction.FStatus,
                        FOpeningTime = attraction.FOpeningTime,
                        FClosingTime = attraction.FClosingTime,
                        FPhoneNumber = attraction.FPhoneNumber,
                        FWebsiteUrl = attraction.FWebsiteUrl,
                        FCreatedDate = attraction.FCreatedDate,
                        FUpdatedDate = attraction.FUpdatedDate,
                        FTrafficInformation = attraction.FTrafficInformation,
                        FLongitude = attraction.FLongitude,
                        FLatitude = attraction.FLatitude
                    }
                ).ToListAsync();
            return attractionDTOs;
        }

        // GET: api/TAttractions/5
        [HttpGet("{id}")]
        public async Task<TAttractionDTO> GetTAttraction(int id) {
            var attraction = await _context.TAttractions.Include(t => t.FCategory).FirstOrDefaultAsync(t => t.FAttractionId == id);
            TAttractionDTO attractionDTO = null;

            if (attraction == null) {
                return null;
            }

            attractionDTO = new TAttractionDTO {
                FAttractionId = attraction.FAttractionId,
                FAttractionName = attraction.FAttractionName,
                FCategoryId = attraction.FCategoryId,
                FCategoryName = attraction.FCategory.FAttractionCategoryName,
                FDescription = attraction.FDescription,
                FRegion = attraction.FRegion,
                FAddress = attraction.FAddress,
                FStatus = attraction.FStatus,
                FOpeningTime = attraction.FOpeningTime,
                FClosingTime = attraction.FClosingTime,
                FPhoneNumber = attraction.FPhoneNumber,
                FWebsiteUrl = attraction.FWebsiteUrl,
                FCreatedDate = attraction.FCreatedDate,
                FUpdatedDate = attraction.FUpdatedDate,
                FTrafficInformation = attraction.FTrafficInformation,
                FLongitude = attraction.FLongitude,
                FLatitude = attraction.FLatitude
            };
            return attractionDTO;
        }

        // GET: api/TAttractions/Search?keyword=A&pageSize=10&pageIndex=0
        [HttpGet]
        [Route("Search")]
        public async Task<IEnumerable<TAttractionDTO>> GetAttractionByCondition([FromQuery] string keyword = "", [FromQuery] int pageSize = 10, [FromQuery] int pageIndex = 0) {

            // .Skip(pageSize * pageIndex):
            // 跳過 pageSize *pageIndex 筆資料。
            // 假設 pageIndex = 0，則跳過 10 * 0 = 0 筆，表示從第一筆開始。
            // 假設 pageIndex = 1，則跳過 10 * 1 = 10 筆，表示從第 11 筆開始。

            // .Take(pageSize):
            // 取出最多 pageSize 筆資料。
            // 在這裡，表示從跳過的筆數後開始，取出最多 10 筆資料。
            var attractions = await _context.TAttractions
                .Include(attraction => attraction.FCategory)
                .Where(attraction => attraction.FAttractionName.ToLower().Contains(keyword.ToLower()))
                .Skip(pageSize * pageIndex)
                .Take(pageSize).ToListAsync();

            // .Any() 是 LINQ 的一個方法，檢查集合中是否存在至少一個元素。
            // 如果集合中有資料，.Any() 會回傳 true。
            // 如果集合為空，.Any() 會回傳 false。
            if (attractions == null || !attractions.Any()) {
                return new List<TAttractionDTO>();
            }

            var attractionDTOs = attractions.Select(
                attraction => new TAttractionDTO {
                    FAttractionId = attraction.FAttractionId,
                    FAttractionName = attraction.FAttractionName,
                    FCategoryId = attraction.FCategoryId,
                    FCategoryName = attraction.FCategory.FAttractionCategoryName,
                    FDescription = attraction.FDescription,
                    FRegion = attraction.FRegion,
                    FAddress = attraction.FAddress,
                    FStatus = attraction.FStatus,
                    FOpeningTime = attraction.FOpeningTime,
                    FClosingTime = attraction.FClosingTime,
                    FPhoneNumber = attraction.FPhoneNumber,
                    FWebsiteUrl = attraction.FWebsiteUrl,
                    FCreatedDate = attraction.FCreatedDate,
                    FUpdatedDate = attraction.FUpdatedDate,
                    FTrafficInformation = attraction.FTrafficInformation,
                    FLongitude = attraction.FLongitude,
                    FLatitude = attraction.FLatitude
                }
            ).ToList();

            return attractionDTOs;
        }

        // PUT: api/TAttractions/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutTAttraction(int id, TAttractionDTO attractionDTO) {
            if (id != attractionDTO.FAttractionId) {
                return BadRequest("Id 不符合");
            }

            TAttraction attraction = await _context.TAttractions.FindAsync(id);
            if (attraction == null) {
                return NotFound();
            }
            attraction.FAttractionName = attractionDTO.FAttractionName;
            attraction.FCategoryId = attractionDTO.FCategoryId;
            attraction.FDescription = attractionDTO.FDescription;
            attraction.FRegion = attractionDTO.FRegion;
            attraction.FAddress = attractionDTO.FAddress;
            attraction.FStatus = attractionDTO.FStatus;


            //if (TimeOnly.TryParse(attractionDTO.FOpeningTime, out var result)) {
            //    //attraction.FOpeningTime = fOpeningTime;
            //}
            //if (TimeOnly.TryParse(attractionDTO.FClosingTime, out var fClosingTime)) {
            //    attraction.FClosingTime = fClosingTime;
            //}
            attraction.FOpeningTime = attractionDTO.FOpeningTime;
            attraction.FClosingTime = attractionDTO.FClosingTime;
            attraction.FPhoneNumber = attractionDTO.FPhoneNumber;
            attraction.FWebsiteUrl = attractionDTO.FWebsiteUrl;
            attraction.FCreatedDate = attractionDTO.FCreatedDate;
            attraction.FUpdatedDate = DateTime.Now;
            attraction.FTrafficInformation = attractionDTO.FTrafficInformation;
            attraction.FLongitude = attractionDTO.FLongitude;
            attraction.FLatitude = attractionDTO.FLatitude;

            _context.Entry(attraction).State = EntityState.Modified;

            try {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException) {
                if (!TAttractionExists(id)) {
                    return NotFound();
                }
                else {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/TAttractions
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<TAttractionDTO> PostTAttraction(TAttractionDTO attractionDTO) {
            TAttraction attraction = new TAttraction {
                // Id 是資料庫自動產生的，這裡先預設為 0
                FAttractionId = 0,
                FAttractionName = attractionDTO.FAttractionName,
                FCategoryId = attractionDTO.FCategoryId,
                FDescription = attractionDTO.FDescription,
                FRegion = attractionDTO.FRegion,
                FAddress = attractionDTO.FAddress,
                FStatus = attractionDTO.FStatus,
                FOpeningTime = attractionDTO.FOpeningTime,
                FClosingTime = attractionDTO.FClosingTime,
                FPhoneNumber = attractionDTO.FPhoneNumber,
                FWebsiteUrl = attractionDTO.FWebsiteUrl,
                FCreatedDate = DateTime.Now,
                FUpdatedDate = DateTime.Now,
                FTrafficInformation = attractionDTO.FTrafficInformation,
                FLongitude = attractionDTO.FLongitude,
                FLatitude = attractionDTO.FLatitude
            };

            _context.TAttractions.Add(attraction);

            // 1. 新的記錄插入資料庫。
            // 2. 資料庫生成並返回新的 FAttractionId。
            // 3. EF 將新生成的 ID 更新到 attraction.FAttractionId。
            await _context.SaveChangesAsync();

            attractionDTO.FAttractionId = attraction.FAttractionId;// 更新 attractionDTO 的 FAttractionId
            return attractionDTO;
        }

        // DELETE: api/TAttractions/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTAttraction(int id) {
            var tAttraction = await _context.TAttractions.FindAsync(id);
            if (tAttraction == null) {
                return NotFound();
            }

            _context.TAttractions.Remove(tAttraction);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool TAttractionExists(int id) {
            return _context.TAttractions.Any(e => e.FAttractionId == id);
        }
    }
}
