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
    public class TAttractionUserFavoritesController : ControllerBase {
        private readonly dbGroupBContext _context;

        public TAttractionUserFavoritesController(dbGroupBContext context) {
            _context = context;
        }

        // GET: api/TAttractionUserFavorites
        //[HttpGet]
        //public async Task<ActionResult<IEnumerable<TAttractionUserFavorite>>> GetTAttractionUserFavorites()
        //{
        //    return await _context.TAttractionUserFavorites.ToListAsync();
        //}

        // GET: api/TAttractionUserFavorites/5
        // id is user id
        [HttpGet("{id}")]
        public async Task<IEnumerable<TAttractionUserFavoriteDTO>> GetTAttractionUserFavorite(int id) {
            var favorites = await _context.TAttractionUserFavorites
                .Include(favorite => favorite.FAttraction)
                .Include(favorite => favorite.FUser)
                .Where(favorite => favorite.FUserId == id)
                .ToListAsync();

            if (favorites == null) {
                return null;
            }

            var favoriteDTOs = favorites.Select(favorite => new TAttractionUserFavoriteDTO {
                FFavoriteId = favorite.FFavoriteId,
                FUserId = favorite.FUserId,
                FUsername = favorite.FUser.FUserName,
                FAttractionId = favorite.FAttractionId,
                FAttractionName = favorite.FAttraction.FAttractionName
            });

            return favoriteDTOs;
        }

        // PUT: api/TAttractionUserFavorites/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        //[HttpPut("{id}")]
        //public async Task<IActionResult> PutTAttractionUserFavorite(int id, TAttractionUserFavorite tAttractionUserFavorite)
        //{
        //    if (id != tAttractionUserFavorite.FFavoriteId)
        //    {
        //        return BadRequest();
        //    }

        //    _context.Entry(tAttractionUserFavorite).State = EntityState.Modified;

        //    try
        //    {
        //        await _context.SaveChangesAsync();
        //    }
        //    catch (DbUpdateConcurrencyException)
        //    {
        //        if (!TAttractionUserFavoriteExists(id))
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

        // POST: api/TAttractionUserFavorites
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<TAttractionUserFavoriteDTO>> PostTAttractionUserFavorite(TAttractionUserFavorite tAttractionUserFavorite) {
            _context.TAttractionUserFavorites.Add(tAttractionUserFavorite);
            await _context.SaveChangesAsync();

            var favoriteDTO = new TAttractionUserFavoriteDTO {
                FFavoriteId = tAttractionUserFavorite.FFavoriteId,
                FUserId = tAttractionUserFavorite.FUserId,
                FUsername = tAttractionUserFavorite.FUser.FUserName,
                FAttractionId = tAttractionUserFavorite.FAttractionId,
                FAttractionName = tAttractionUserFavorite.FAttraction.FAttractionName
            };

            return favoriteDTO;
        }

        // DELETE: api/TAttractionUserFavorites
        [HttpDelete]
        public async Task<IActionResult> DeleteTAttractionUserFavorite(TAttractionUserFavorite tAttractionUserFavorite) {            
            if (tAttractionUserFavorite == null) {
                return NotFound();
            }

            _context.TAttractionUserFavorites.Remove(tAttractionUserFavorite);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool TAttractionUserFavoriteExists(int id) {
            return _context.TAttractionUserFavorites.Any(e => e.FFavoriteId == id);
        }
    }
}
