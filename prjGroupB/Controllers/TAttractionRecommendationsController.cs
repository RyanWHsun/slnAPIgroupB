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
    public class TAttractionRecommendationsController : ControllerBase
    {
        private readonly dbGroupBContext _context;

        public TAttractionRecommendationsController(dbGroupBContext context)
        {
            _context = context;
        }

        // GET: api/TAttractionRecommendations/5
        // id is attraction id
        // 取得跟此景點有關的所有推薦景點
        // 例如：id=1 的景點，會推薦 id=2, id=3 的景點
        [HttpGet("{id}")]
        public async Task<ActionResult<IEnumerable<TAttractionRecommendationDTO>>> GetTAttractionRecommendation(int id)
        {
            try
            {
                var tAttractionRecommendations = await _context.TAttractionRecommendations
                    .Include(recommend => recommend.FAttraction)
                    .Include(recommend => recommend.FRecommendation)
                    .Where(recommend => recommend.FAttractionId == id)
                    .ToListAsync();

                if (tAttractionRecommendations == null || !tAttractionRecommendations.Any())
                {
                    return NotFound(new { error = "找不到相關推薦景點" });
                }

                var tAttractionRecommendationDTOs = tAttractionRecommendations.Select(recommend => new TAttractionRecommendationDTO
                {
                    FAttractionRecommendationId = recommend.FAttractionRecommendationId,
                    FAttractionId = recommend.FAttractionId,
                    FAttractionName = recommend.FAttraction.FAttractionName,
                    FRecommendationId = recommend.FRecommendationId,
                    FRecommendAttractionName = recommend.FRecommendation.FAttractionName,
                    FReason = recommend.FReason
                }).ToList();

                return Ok(tAttractionRecommendationDTOs);
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

        private bool TAttractionRecommendationExists(int id)
        {
            return _context.TAttractionRecommendations.Any(e => e.FAttractionRecommendationId == id);
        }
    }
}
