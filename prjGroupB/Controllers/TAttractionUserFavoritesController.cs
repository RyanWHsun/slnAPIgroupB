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
    public class TAttractionUserFavoritesController : ControllerBase {
        private readonly dbGroupBContext _context;

        public TAttractionUserFavoritesController(dbGroupBContext context) {
            _context = context;
        }

        // GET: api/TAttractionUserFavorites/5
        // id is user id
        [HttpGet("{id}")]
        public async Task<ActionResult<IEnumerable<TAttractionUserFavoriteDTO>>> GetTAttractionUserFavorite(int id) {
            try {
                var favorites = await _context.TAttractionUserFavorites
                    .Include(favorite => favorite.FAttraction)
                    .Include(favorite => favorite.FUser)
                    .Where(favorite => favorite.FUserId == id)
                    .ToListAsync();

                if (favorites == null || !favorites.Any()) {
                    return NotFound(new { error = "找不到收藏的景點" });
                }

                var favoriteDTOs = favorites.Select(favorite => new TAttractionUserFavoriteDTO {
                    FFavoriteId = favorite.FFavoriteId,
                    FUserId = favorite.FUserId,
                    FUsername = favorite.FUser.FUserName,
                    FAttractionId = favorite.FAttractionId,
                    FAttractionName = favorite.FAttraction.FAttractionName
                }).ToList();

                return Ok(favoriteDTOs);
            }
            catch (DbException ex) {
                // 資料庫連線錯誤
                return StatusCode(500, new { error = "資料庫連線錯誤", details = ex.Message });
            }
            catch (Exception ex) {
                // 一般錯誤
                return StatusCode(500, new { error = "內部錯誤", details = ex.Message });
            }
        }

        // POST: api/TAttractionUserFavorites
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<TAttractionUserFavoriteDTO>> PostTAttractionUserFavorite(TAttractionUserFavorite tAttractionUserFavorite) {
            try {
                if (tAttractionUserFavorite == null) {
                    return BadRequest(new { error = "收藏資訊不能為空" });
                }

                _context.TAttractionUserFavorites.Add(tAttractionUserFavorite);
                await _context.SaveChangesAsync();

                var favoriteDTO = new TAttractionUserFavoriteDTO {
                    FFavoriteId = tAttractionUserFavorite.FFavoriteId,
                    FUserId = tAttractionUserFavorite.FUserId,
                    FUsername = tAttractionUserFavorite.FUser.FUserName,
                    FAttractionId = tAttractionUserFavorite.FAttractionId,
                    FAttractionName = tAttractionUserFavorite.FAttraction.FAttractionName
                };

                return CreatedAtAction(nameof(GetTAttractionUserFavorite), new { id = favoriteDTO.FUserId }, favoriteDTO);
            }
            catch (DbUpdateException ex) {
                // 資料庫更新錯誤
                return StatusCode(500, new { error = "資料庫更新錯誤", details = ex.Message });
            }
            catch (Exception ex) {
                // 一般錯誤
                return StatusCode(500, new { error = "內部錯誤", details = ex.Message });
            }
        }

        // DELETE: api/TAttractionUserFavorites
        [HttpDelete]
        public async Task<IActionResult> DeleteTAttractionUserFavorite(TAttractionUserFavorite tAttractionUserFavorite) {
            try {
                if (tAttractionUserFavorite == null) {
                    return BadRequest(new { error = "刪除的收藏資訊不能為空" });
                }

                var existingFavorite = await _context.TAttractionUserFavorites
                    .FirstOrDefaultAsync(f => f.FFavoriteId == tAttractionUserFavorite.FFavoriteId);

                if (existingFavorite == null) {
                    return NotFound(new { error = "找不到要刪除的收藏" });
                }

                _context.TAttractionUserFavorites.Remove(existingFavorite);
                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (DbUpdateException ex) {
                // 資料庫更新錯誤
                return StatusCode(500, new { error = "資料庫更新錯誤", details = ex.Message });
            }
            catch (Exception ex) {
                // 一般錯誤
                return StatusCode(500, new { error = "內部錯誤", details = ex.Message });
            }
        }

        private bool TAttractionUserFavoriteExists(int id) {
            return _context.TAttractionUserFavorites.Any(e => e.FFavoriteId == id);
        }
    }
}
