using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using prjGroupB.DTO;
using prjGroupB.Models;

namespace prjGroupB.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TPostLikesController : ControllerBase
    {
        private readonly dbGroupBContext _context;

        public TPostLikesController(dbGroupBContext context)
        {
            _context = context;
        }

        // GET: api/TPostLikes/GetTPostLikeCount/5
        [HttpGet("GetTPostLikeCount/{id}")]
        public int GetTPostLikeCount(int id)
        {
            return _context.TPostLikes.Where(e=>e.FPostId==id).Count();
        }

        // GET: api/TPostLikes/5
        [HttpGet("{id}")]
        [Authorize]
        public async Task<TPostLikesDTO?> GetTPostLike(int id)
        {
            int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            return _context.TPostLikes
                .Where(e => e.FPostId == id && e.FUserId == userId)
                .Select(e => new TPostLikesDTO
                {
                    FLikeId = e.FLikeId,
                    FUserId = e.FUserId,
                    FPostId = e.FPostId
                })
                .FirstOrDefault();
        }

        // POST: api/TPostLikes
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        [Authorize]
        public async Task<TPostLikesDTO> PostTPostLike(TPostLikesDTO likeDTO)
        {
            int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            TPostLike like = new TPostLike
            {
                FUserId = userId,
                FPostId = likeDTO.FPostId
            };
            _context.TPostLikes.Add(like);
            await _context.SaveChangesAsync();
            likeDTO.FLikeId = like.FLikeId;
            return likeDTO;
        }

        // DELETE: api/TPostLikes/5
        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> DeleteTPostLike(int id)
        {
            int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            TPostLike like = await _context.TPostLikes.FindAsync(id);
            if (like == null)
            {
                return NotFound(new { message = "查無喜歡紀錄" });
            }
            if (like.FUserId != userId)
            {
                return Unauthorized(new { message = "你沒有權限刪除此喜歡紀錄" });
            }
            try
            {
                _context.TPostLikes.Remove(like);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                return StatusCode(500, new { message = "刪除資料庫失敗" });
            }
            return Ok(new { message = "刪除喜歡紀錄成功" });
        }

    }
}
