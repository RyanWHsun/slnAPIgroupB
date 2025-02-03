using System;
using System.Collections.Generic;
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
    public class TAttractionCategoriesController : ControllerBase
    {
        private readonly dbGroupBContext _context;

        public TAttractionCategoriesController(dbGroupBContext context)
        {
            _context = context;
        }

        // GET: api/TAttractionCategories
        [HttpGet]
        public async Task<IEnumerable<TAttractionCategoryDTO>> GetTAttractionCategories()
        {
            var categorieDTOs = await _context.TAttractionCategories.Select(
                data =>new TAttractionCategoryDTO {
                    FAttractionCategoryId = data.FAttractionCategoryId,
                    FAttractionCategoryName = data.FAttractionCategoryName
                }
            ).ToListAsync();
            return categorieDTOs;
        }

        // GET: api/TAttractionCategories/5
        // id is CategoryId
        [HttpGet("{id}")]
        public async Task<TAttractionCategoryDTO> GetTAttractionCategory(int id)
        {
            var tAttractionCategory = await _context.TAttractionCategories.FindAsync(id);

            if (tAttractionCategory == null)
            {
                return null;
            }

            TAttractionCategoryDTO categoryDTO = new TAttractionCategoryDTO {
                FAttractionCategoryId = tAttractionCategory.FAttractionCategoryId,
                FAttractionCategoryName = tAttractionCategory.FAttractionCategoryName
            };

            return categoryDTO;
        }

        // PUT: api/TAttractionCategories/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        //[HttpPut("{id}")]
        //public async Task<IActionResult> PutTAttractionCategory(int id, TAttractionCategory tAttractionCategory)
        //{
        //    if (id != tAttractionCategory.FAttractionCategoryId)
        //    {
        //        return BadRequest();
        //    }

        //    _context.Entry(tAttractionCategory).State = EntityState.Modified;

        //    try
        //    {
        //        await _context.SaveChangesAsync();
        //    }
        //    catch (DbUpdateConcurrencyException)
        //    {
        //        if (!TAttractionCategoryExists(id))
        //        {
        //            return NotFound();
        //        }
        //        else
        //        {
        //            throw;
        //        }
        //    }

        //    return NoContent();
        //}

        // POST: api/TAttractionCategories
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        //[HttpPost]
        //public async Task<ActionResult<TAttractionCategory>> PostTAttractionCategory(TAttractionCategory tAttractionCategory)
        //{
        //    _context.TAttractionCategories.Add(tAttractionCategory);
        //    await _context.SaveChangesAsync();

        //    return CreatedAtAction("GetTAttractionCategory", new { id = tAttractionCategory.FAttractionCategoryId }, tAttractionCategory);
        //}

        //// DELETE: api/TAttractionCategories/5
        //[HttpDelete("{id}")]
        //public async Task<IActionResult> DeleteTAttractionCategory(int id)
        //{
        //    var tAttractionCategory = await _context.TAttractionCategories.FindAsync(id);
        //    if (tAttractionCategory == null)
        //    {
        //        return NotFound();
        //    }

        //    _context.TAttractionCategories.Remove(tAttractionCategory);
        //    await _context.SaveChangesAsync();

        //    return NoContent();
        //}

        //private bool TAttractionCategoryExists(int id)
        //{
        //    return _context.TAttractionCategories.Any(e => e.FAttractionCategoryId == id);
        //}
    }
}
