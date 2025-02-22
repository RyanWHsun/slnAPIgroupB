using System;
using System.Collections.Generic;
using System.Drawing.Printing;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using prjGroupB.DTO;
using prjGroupB.Models;

namespace prjGroupB.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    public class TPostsController : ControllerBase
    {
        private readonly dbGroupBContext _context;

        public TPostsController(dbGroupBContext context)
        {
            _context = context;
        }

        // GET: api/TPosts/GetAllPosts
        [HttpGet("GetPublicPosts")]
        public async Task<IEnumerable<TPostsDTO>> GetPublicPosts(int page = 1, int pageSize = 6)
        {
            return _context.TPosts
                .Where(t => t.FIsPublic == true)
                .OrderByDescending(t => t.FCreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(e => new TPostsDTO
                {
                    FPostId = e.FPostId,
                    FUserId = e.FUserId,
                    FTitle = e.FTitle,
                    FContent = e.FContent,
                    FCreatedAt = e.FCreatedAt,
                    FUpdatedAt = e.FUpdatedAt,
                    FIsPublic = e.FIsPublic,
                    FCategoryId = e.FCategoryId
                });
        }

        // GET: api/TPosts/
        [HttpGet]
        [Authorize]
        public async Task<IEnumerable<TPostsDTO>> GetMyPosts(int page = 1, int pageSize = 9)
        {
            int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            return _context.TPosts
                .Where(t => t.FUserId == userId)
                .OrderByDescending(t => t.FCreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(e => new TPostsDTO
                {
                    FPostId = e.FPostId,
                    FUserId = e.FUserId,
                    FTitle = e.FTitle,
                    FContent = e.FContent,
                    FCreatedAt = e.FCreatedAt,
                    FUpdatedAt = e.FUpdatedAt,
                    FIsPublic = e.FIsPublic,
                    FCategoryId = e.FCategoryId
                });
        }

        // PUT: api/TPosts/
        [HttpPut]
        [Authorize]
        public async Task<IActionResult> PutTPost(TPostsDTO PostsDTO)
        {
            int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            TPost post = await _context.TPosts.FindAsync(PostsDTO.FPostId);
            if (post == null)
            {
                return NotFound(new { message = "查無文章" });
            }
            if (post.FUserId != userId)
            {
                return Unauthorized(new { message = "你沒有權限修改此文章" });
            }
            post.FTitle = PostsDTO.FTitle;
            post.FContent = PostsDTO.FContent;
            post.FUpdatedAt = DateTime.Now;
            post.FIsPublic = PostsDTO.FIsPublic;
            post.FCategoryId = PostsDTO.FCategoryId;
            try
            {
                _context.TPosts.Update(post);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                return StatusCode(500, new { message = "修改資料庫失敗" });
            }
            return Ok(new { message = "修改成功" });
        }

        // POST: api/TPosts
        [HttpPost]
        [Authorize]
        public async Task<TPostsDTO> PostTPost(TPostsDTO PostsDTO)
        {
            int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            TPost post = new TPost
            {
                FUserId = userId,
                FTitle = PostsDTO.FTitle,
                FContent = PostsDTO.FContent,
                FCreatedAt = DateTime.Now,
                FIsPublic = PostsDTO.FIsPublic,
                FCategoryId = PostsDTO.FCategoryId
            };
            _context.TPosts.Add(post);
            await _context.SaveChangesAsync();
            PostsDTO.FPostId = post.FPostId;
            return PostsDTO;
        }

        // DELETE: api/TPosts/id
        [HttpDelete("{id}")]
        [Authorize]
        public async Task<string> DeleteTPost(int id)
        {
            int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            TPost post = await _context.TPosts.FindAsync(id);
            if (post == null)
            {
                return "查無文章";
            }
            if (post.FUserId != userId)
            {
                return "你沒有權限刪除此文章";
            }
            try
            {
                _context.TPosts.Remove(post);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                return "刪除資料庫失敗";
            }
            return "刪除文章成功";
        }

        // GET: api/TPosts/userInfo
        [HttpGet("userInfo/{id}")]
        public async Task<ActionResult<TPostsUserInfoDTO>> GetUserInfo(int id)
        {
            var tUser = await _context.TUsers.FindAsync(id);

            var userDTO = new TPostsUserInfoDTO
            {
                FUserName = tUser.FUserName,
                FUserNickName = tUser.FUserNickName,
                FUserImage = tUser.FUserImage != null ? Convert.ToBase64String(tUser.FUserImage) : null,
            };
            return userDTO;
        }

        [HttpGet("loginUserId")]
        [Authorize]
        public int GetLoginUserId()
        {
            int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            return userId;
        }
    }
}
