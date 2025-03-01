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

namespace prjGroupB.Controllers {
    [Route("api/[controller]")]
    [ApiController]
    public class TAttractionCategoriesController : ControllerBase {
        private readonly dbGroupBContext _context;

        public TAttractionCategoriesController(dbGroupBContext context) {
            _context = context;
        }

        // GET: api/TAttractionCategories
        [HttpGet]
        public async Task<ActionResult<IEnumerable<TAttractionCategoryDTO>>> GetTAttractionCategories() {
            try {
                var categorieDTOs = await _context.TAttractionCategories.Select(
                    data => new TAttractionCategoryDTO {
                        FAttractionCategoryId = data.FAttractionCategoryId,
                        FAttractionCategoryName = data.FAttractionCategoryName
                    }
                ).ToListAsync();

                if (categorieDTOs == null || !categorieDTOs.Any()) {
                    return NotFound(new { error = "找不到任何景點分類" });
                }

                return Ok(categorieDTOs);
            }
            catch (DbException ex)  // 捕捉資料庫連線問題
            {
                return StatusCode(500, new { error = "資料庫連線錯誤", details = ex.Message });
            }
            catch (Exception ex) {
                return StatusCode(500, new { error = "內部錯誤", details = ex.Message });
            }
        }

        // GET: api/TAttractionCategories/5
        // id is CategoryId
        [HttpGet("{id}")]
        public async Task<ActionResult<TAttractionCategoryDTO>> GetTAttractionCategory(int id) {
            try {
                var tAttractionCategory = await _context.TAttractionCategories.FindAsync(id);

                if (tAttractionCategory == null) {
                    return NotFound(new { error = $"找不到ID為 {id} 的景點分類" });
                }

                TAttractionCategoryDTO categoryDTO = new TAttractionCategoryDTO {
                    FAttractionCategoryId = tAttractionCategory.FAttractionCategoryId,
                    FAttractionCategoryName = tAttractionCategory.FAttractionCategoryName
                };

                return Ok(categoryDTO);
            }
            catch (DbException ex)  // 捕捉資料庫連線問題
            {
                return StatusCode(500, new { error = "資料庫連線錯誤", details = ex.Message });
            }
            catch (Exception ex) {
                return StatusCode(500, new { error = "內部錯誤", details = ex.Message });
            }
        }
    }
}
