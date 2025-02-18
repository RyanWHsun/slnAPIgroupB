using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using prjGroupB.DTO;
using prjGroupB.Models;

namespace prjGroupB.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TPostCommentsController : ControllerBase
    {
        private readonly dbGroupBContext _context;

        public TPostCommentsController(dbGroupBContext context)
        {
            _context = context;
        }

        // GET: api/TPostComments/5
        [HttpGet("{id}")]
        public async Task<IEnumerable<TPostCommentsDTO>> GetTPostComment(int id)
        {
            TPost post = await _context.TPosts.FindAsync(id);
            if (post.FIsPublic != true)
            {
                return null;
            }
            return _context.TPostComments
                .Where(c => c.FPostId == id)
                .OrderByDescending(t => t.FCreatedAt)
                .Include(e=>e.FUser)
                .Select(e => new TPostCommentsDTO{
                    FCommentId = e.FCommentId,
                    FPostId = e.FPostId,
                    FUserId = e.FUser.FUserId,
                    FUserName = e.FUser.FUserName,
                    FUserNickName = e.FUser.FUserNickName,
                    FUserImage = Convert.ToBase64String(e.FUser.FUserImage),
                    FContent = e.FContent,
                    FCreatedAt = e.FCreatedAt,
                    FUpdatedAt = e.FUpdatedAt,
                    FParentCommentId = e.FParentCommentId
                });
        }

        // PUT: api/TPostComments/5
        [HttpPut("{id}")]
        [Authorize]
        public async Task<string> PutTPostComment(int id, TPostCommentsDTO PostCommentsDTO)
        {
            int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            TPostComment comment = await _context.TPostComments.FindAsync(id);
            if (comment == null)
            {
                return "查無留言";
            }
            if (comment.FUserId != userId)
            {
                return "你沒有權限修改此留言";
            }
            comment.FContent = PostCommentsDTO.FContent;
            comment.FUpdatedAt = DateTime.Now;
            try
            {
                _context.TPostComments.Update(comment);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                return "修改資料庫失敗";
            }
            return "修改留言成功";
        }

        // POST: api/TPostComments/
        [HttpPost]
        [Authorize]
        public async Task<TPostCommentsDTO> PostTPostComment(TPostCommentsDTO PostCommentsDTO)
        {
            int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            TPostComment comment = new TPostComment
            {
                FPostId = PostCommentsDTO.FPostId,
                FUserId = userId,
                FContent = PostCommentsDTO.FContent,
                FCreatedAt = DateTime.Now,
                FParentCommentId = null
            };
            _context.TPostComments.Add(comment);
            await _context.SaveChangesAsync();
            PostCommentsDTO.FCommentId = comment.FCommentId;
            return PostCommentsDTO;
        }

        // DELETE: api/TPostComments/5
        [HttpDelete("{id}")]
        [Authorize]
        public async Task<string> DeleteTPostComment(int id)
        {
            int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            TPostComment comment = await _context.TPostComments.FindAsync(id);
            if (comment == null)
            {
                return "查無留言";
            }
            if (comment.FUserId != userId)
            {
                return "你沒有權限修改此留言";
            }
            try
            {
                _context.TPostComments.Remove(comment);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                return "刪除資料庫失敗";
            }
            return "刪除留言成功";
        }
    }
}
