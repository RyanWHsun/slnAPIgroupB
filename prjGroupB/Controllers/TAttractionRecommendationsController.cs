using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using prjGroupB.DTO;
using prjGroupB.Models;

namespace prjGroupB.Controllers {
    [Route("api/[controller]")]
    [ApiController]
    public class TAttractionRecommendationsController : ControllerBase {
        private readonly dbGroupBContext _context;

        public TAttractionRecommendationsController(dbGroupBContext context) {
            _context = context;
        }

        // GET: api/TAttractionRecommendations
        //[HttpGet]
        //public async Task<ActionResult<IEnumerable<TAttractionRecommendation>>> GetTAttractionRecommendations()
        //{
        //    return await _context.TAttractionRecommendations.ToListAsync();
        //}

        // GET: api/TAttractionRecommendations/5
        // id is attraction id
        // 取得跟此景點有關的所有推薦景點
        [HttpGet("{id}")]
        public async Task<IEnumerable<TAttractionRecommendationDTO>> GetTAttractionRecommendation(int id) {
            var tAttractionRecommendations = await _context.TAttractionRecommendations
                .Include(recommend => recommend.FAttraction)
                .Include(recommend => recommend.FRecommendation)
                .Where(recommend => recommend.FAttractionId == id)
                .ToListAsync();

            if (tAttractionRecommendations == null) {
                return null;
            }

            var tAttractionRecommendationDTOs = tAttractionRecommendations.Select(recommend => new TAttractionRecommendationDTO {
                FAttractionRecommendationId = recommend.FAttractionRecommendationId,
                FAttractionId = recommend.FAttractionId,
                FAttractionName = recommend.FAttraction.FAttractionName,
                FRecommendationId = recommend.FRecommendationId,
                FRecommendAttractionName = recommend.FRecommendation.FAttractionName,
                FReason = recommend.FReason
            });

            return tAttractionRecommendationDTOs;
        }

        // PUT: api/TAttractionRecommendations/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        //[HttpPut("{id}")]
        //public async Task<IActionResult> PutTAttractionRecommendation(int id, TAttractionRecommendation tAttractionRecommendation)
        //{
        //    if (id != tAttractionRecommendation.FAttractionRecommendationId)
        //    {
        //        return BadRequest();
        //    }

        //    _context.Entry(tAttractionRecommendation).State = EntityState.Modified;

        //    try
        //    {
        //        await _context.SaveChangesAsync();
        //    }
        //    catch (DbUpdateConcurrencyException)
        //    {
        //        if (!TAttractionRecommendationExists(id))
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

        // POST: api/TAttractionRecommendations
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        //[HttpPost]
        //public async Task<ActionResult<TAttractionRecommendation>> PostTAttractionRecommendation(TAttractionRecommendation tAttractionRecommendation)
        //{
        //    _context.TAttractionRecommendations.Add(tAttractionRecommendation);
        //    await _context.SaveChangesAsync();

        //    return CreatedAtAction("GetTAttractionRecommendation", new { id = tAttractionRecommendation.FAttractionRecommendationId }, tAttractionRecommendation);
        //}

        // DELETE: api/TAttractionRecommendations/5
        //[HttpDelete("{id}")]
        //public async Task<IActionResult> DeleteTAttractionRecommendation(int id)
        //{
        //    var tAttractionRecommendation = await _context.TAttractionRecommendations.FindAsync(id);
        //    if (tAttractionRecommendation == null)
        //    {
        //        return NotFound();
        //    }

        //    _context.TAttractionRecommendations.Remove(tAttractionRecommendation);
        //    await _context.SaveChangesAsync();

        //    return NoContent();
        //}

        private bool TAttractionRecommendationExists(int id) {
            return _context.TAttractionRecommendations.Any(e => e.FAttractionRecommendationId == id);
        }
    }
}
