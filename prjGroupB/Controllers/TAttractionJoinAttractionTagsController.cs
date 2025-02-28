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
    public class TAttractionJoinAttractionTagsController : ControllerBase
    {
        private readonly dbGroupBContext _context;

        public TAttractionJoinAttractionTagsController(dbGroupBContext context)
        {
            _context = context;
        }

        // GET: api/TAttractionJoinAttractionTags/5
        // id is the AttractionId
        [HttpGet("{id}")]
        public async Task<ActionResult<IEnumerable<TAttractionJoinAttractionTagDTO>>> GetTAttractionJoinAttractionTag(int id)
        {
            try
            {
                var tAttractionJoinAttractionTags = await _context.TAttractionJoinAttractionTags
                    .Include(tag => tag.FAttraction)
                    .Include(tag => tag.FTag)
                    .Where(tag => tag.FAttractionId == id)
                    .ToListAsync();

                if (tAttractionJoinAttractionTags == null || !tAttractionJoinAttractionTags.Any())
                {
                    return NotFound(new { error = "找不到相關標籤" });
                }

                var tagDTOs = tAttractionJoinAttractionTags
                    .Select(tag => new TAttractionJoinAttractionTagDTO
                    {
                        FTagId = tag.FTagId,
                        FTagName = tag.FTag.FTagName,
                        FAttractionId = tag.FAttractionId,
                        FAttractionName = tag.FAttraction.FAttractionName
                    }).ToList();

                return Ok(tagDTOs);
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

        private bool TAttractionJoinAttractionTagExists(int id)
        {
            return _context.TAttractionJoinAttractionTags.Any(e => e.FAttractionJoinAttractionTagId == id);
        }
    }
}
