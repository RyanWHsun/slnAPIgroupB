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
    public class TProductCategoriesController : ControllerBase
    {
        private readonly dbGroupBContext _context;

        public TProductCategoriesController(dbGroupBContext context)
        {
            _context = context;
        }

        // GET: api/TProductCategories
        [HttpGet]
        public async Task<IEnumerable<TProductCategoryDTO>> GetTProductCategories()
        {
            return _context.TProductCategories.Select(c => new TProductCategoryDTO
            {
                FCategoryName=c.FCategoryName,
                FProductCategoryId=c.FProductCategoryId,
                ProductCount = _context.TProducts.Count(p=>p.FProductCategoryId==c.FProductCategoryId)
            });
        }
            
        //    // GET: api/TProductCategories/5
        //    [HttpGet("{id}")]
        //    public async Task<ActionResult<TProductCategory>> GetTProductCategory(int id)
        //    {
        //        var tProductCategory = await _context.TProductCategories.FindAsync(id);

        //        if (tProductCategory == null)
        //        {
        //            return NotFound();
        //        }

        //        return tProductCategory;
        //    }

        //    // PUT: api/TProductCategories/5
        //    // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        //    [HttpPut("{id}")]
        //    public async Task<IActionResult> PutTProductCategory(int id, TProductCategory tProductCategory)
        //    {
        //        if (id != tProductCategory.FProductCategoryId)
        //        {
        //            return BadRequest();
        //        }

        //        _context.Entry(tProductCategory).State = EntityState.Modified;

        //        try
        //        {
        //            await _context.SaveChangesAsync();
        //        }
        //        catch (DbUpdateConcurrencyException)
        //        {
        //            if (!TProductCategoryExists(id))
        //            {
        //                return NotFound();
        //            }
        //            else
        //            {
        //                throw;
        //            }
        //        }

        //        return NoContent();
        //    }

        //    // POST: api/TProductCategories
        //    // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        //    [HttpPost]
        //    public async Task<ActionResult<TProductCategory>> PostTProductCategory(TProductCategory tProductCategory)
        //    {
        //        _context.TProductCategories.Add(tProductCategory);
        //        await _context.SaveChangesAsync();

        //        return CreatedAtAction("GetTProductCategory", new { id = tProductCategory.FProductCategoryId }, tProductCategory);
        //    }

        //    // DELETE: api/TProductCategories/5
        //    [HttpDelete("{id}")]
        //    public async Task<IActionResult> DeleteTProductCategory(int id)
        //    {
        //        var tProductCategory = await _context.TProductCategories.FindAsync(id);
        //        if (tProductCategory == null)
        //        {
        //            return NotFound();
        //        }

        //        _context.TProductCategories.Remove(tProductCategory);
        //        await _context.SaveChangesAsync();

        //        return NoContent();
        //    }

        //    private bool TProductCategoryExists(int id)
        //    {
        //        return _context.TProductCategories.Any(e => e.FProductCategoryId == id);
        //    }
    }
}
