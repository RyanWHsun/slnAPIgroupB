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
    public class TAttractionJoinAttractionTagsController : ControllerBase {
        private readonly dbGroupBContext _context;

        public TAttractionJoinAttractionTagsController(dbGroupBContext context) {
            _context = context;
        }

        // GET: api/TAttractionJoinAttractionTags
        //[HttpGet]
        //public async Task<ActionResult<IEnumerable<TAttractionJoinAttractionTag>>> GetTAttractionJoinAttractionTags()
        //{
        //    return await _context.TAttractionJoinAttractionTags.ToListAsync();
        //}

        // GET: api/TAttractionJoinAttractionTags/5
        // id is the AttractionId
        [HttpGet("{id}")]
        public async Task<IEnumerable<TAttractionJoinAttractionTagDTO>> GetTAttractionJoinAttractionTag(int id) {
            var tAttractionJoinAttractionTags = await _context.TAttractionJoinAttractionTags
                .Include(tag => tag.FAttraction)
                .Include(tag => tag.FTag)
                .Where(tag => tag.FAttractionId == id)
                .ToListAsync();

            if (tAttractionJoinAttractionTags == null) {
                return null;
            }

            var tagDTOs = tAttractionJoinAttractionTags
                .Select(tag => new TAttractionJoinAttractionTagDTO {
                    FTagId = tag.FTagId,
                    FTagName = tag.FTag.FTagName,
                    FAttractionId = tag.FAttractionId,
                    FAttractionName = tag.FAttraction.FAttractionName
                });

            return tagDTOs;
        }

        // PUT: api/TAttractionJoinAttractionTags/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        //[HttpPut("{id}")]
        //public async Task<IActionResult> PutTAttractionJoinAttractionTag(int id, TAttractionJoinAttractionTag tAttractionJoinAttractionTag)
        //{
        //    if (id != tAttractionJoinAttractionTag.FAttractionJoinAttractionTagId)
        //    {
        //        return BadRequest();
        //    }

        //    _context.Entry(tAttractionJoinAttractionTag).State = EntityState.Modified;

        //    try
        //    {
        //        await _context.SaveChangesAsync();
        //    }
        //    catch (DbUpdateConcurrencyException)
        //    {
        //        if (!TAttractionJoinAttractionTagExists(id))
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

        // POST: api/TAttractionJoinAttractionTags
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        //[HttpPost]
        //public async Task<ActionResult<TAttractionJoinAttractionTag>> PostTAttractionJoinAttractionTag(TAttractionJoinAttractionTag tAttractionJoinAttractionTag)
        //{
        //    _context.TAttractionJoinAttractionTags.Add(tAttractionJoinAttractionTag);
        //    await _context.SaveChangesAsync();

        //    return CreatedAtAction("GetTAttractionJoinAttractionTag", new { id = tAttractionJoinAttractionTag.FAttractionJoinAttractionTagId }, tAttractionJoinAttractionTag);
        //}

        // DELETE: api/TAttractionJoinAttractionTags/5
        //[HttpDelete("{id}")]
        //public async Task<IActionResult> DeleteTAttractionJoinAttractionTag(int id)
        //{
        //    var tAttractionJoinAttractionTag = await _context.TAttractionJoinAttractionTags.FindAsync(id);
        //    if (tAttractionJoinAttractionTag == null)
        //    {
        //        return NotFound();
        //    }

        //    _context.TAttractionJoinAttractionTags.Remove(tAttractionJoinAttractionTag);
        //    await _context.SaveChangesAsync();

        //    return NoContent();
        //}

        private bool TAttractionJoinAttractionTagExists(int id) {
            return _context.TAttractionJoinAttractionTags.Any(e => e.FAttractionJoinAttractionTagId == id);
        }
    }
}
