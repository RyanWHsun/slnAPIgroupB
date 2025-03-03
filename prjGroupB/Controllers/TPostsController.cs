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
        public async Task<IEnumerable<TPostsDTO>> GetPublicPosts(int page = 1, int pageSize = 6, bool popular = false, int categoryId = 0, string keyword = null)
        {
            try
            {
                var query = _context.TPosts
                .Include(p => p.TPostLikes)
                .Where(t => t.FIsPublic == true);
                if (categoryId != 0)
                {
                    query = query.Where(t => t.FCategoryId == categoryId);
                }
                if (!string.IsNullOrEmpty(keyword))
                {
                    query = query.Where(t => t.FTitle.Contains(keyword));
                }
                if (popular)
                {
                    query = query.OrderByDescending(t => t.TPostLikes.Count);
                }
                else
                {
                    query = query.OrderByDescending(t => t.FCreatedAt);
                }
                return query.Skip((page - 1) * pageSize).Take(pageSize)
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
            catch (Exception ex) {
                Console.WriteLine($"取得公開文章失敗: {ex.Message}");
                return new List<TPostsDTO>();
            }
        }

        //public async Task<IEnumerable<TPostsDTO>> GetPublicPosts(int page = 1, int pageSize = 6)
        //{
        //    return _context.TPosts
        //        .Where(t => t.FIsPublic == true)
        //        .OrderByDescending(t => t.FCreatedAt)
        //        .Skip((page - 1) * pageSize)
        //        .Take(pageSize)
        //        .Select(e => new TPostsDTO
        //        {
        //            FPostId = e.FPostId,
        //            FUserId = e.FUserId,
        //            FTitle = e.FTitle,
        //            FContent = e.FContent,
        //            FCreatedAt = e.FCreatedAt,
        //            FUpdatedAt = e.FUpdatedAt,
        //            FIsPublic = e.FIsPublic,
        //            FCategoryId = e.FCategoryId
        //        });
        //}


        // GET: api/TPosts/
        [HttpGet]
        [Authorize]
        public async Task<IEnumerable<TPostsDTO>> GetMyPosts(int page = 1, int pageSize = 9, int categoryId = 0, string afterDate = null, string keyword = null)
        {
            try
            {
                int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
                var query = _context.TPosts.Where(t => t.FUserId == userId);
                DateTime? parsedDate = string.IsNullOrEmpty(afterDate) ? null : DateTime.Parse(afterDate);
                if (categoryId != 0)
                {
                    query = query.Where(t => t.FCategoryId == categoryId);
                }
                if (afterDate != null)
                {
                    query = query.Where(t => t.FCreatedAt >= parsedDate);
                }
                if (!string.IsNullOrEmpty(keyword))
                {
                    query = query.Where(t => t.FTitle.Contains(keyword));
                }
                query = query.OrderByDescending(t => t.FCreatedAt);
                return query.Skip((page - 1) * pageSize)
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
            catch (Exception ex) {
                Console.WriteLine($"取得個人文章失敗: {ex.Message}");
                return new List<TPostsDTO>();
            }
        }

        // PUT: api/TPosts/
        [HttpPut]
        [Authorize]
        public async Task<IActionResult> PutTPost(TPostsDTO PostsDTO)
        {
            try
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
                return Ok(new { message = "修改文章成功" });
            }
            catch (Exception ex) {
                Console.WriteLine($"修改文章失敗: {ex.Message}");
                return Ok(new { message = $"修改文章失敗: {ex.Message}" });
            }
        }

        // POST: api/TPosts
        [HttpPost]
        [Authorize]
        public async Task<TPostsDTO> PostTPost(TPostsDTO PostsDTO)
        {
            try
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
            catch (Exception ex) {
                Console.WriteLine($"新增文章失敗: {ex.Message}");
                return new TPostsDTO();
            }
        }

        // DELETE: api/TPosts/id
        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> DeleteTPost(int id)
        {
            try
            {
                int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
                TPost post = await _context.TPosts.FindAsync(id);
                if (post == null)
                {
                    return NotFound(new { message = "查無文章" });
                }
                if (post.FUserId != userId)
                {
                    return Unauthorized(new { message = "你沒有權限刪除此文章" });
                }
                try
                {
                    _context.TPosts.Remove(post);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateException ex)
                {
                    return StatusCode(500, new { message = "刪除資料庫失敗" });
                }
                return Ok(new { message = "刪除文章成功" });
            }
            catch (Exception ex) {
                Console.WriteLine($"刪除文章失敗: {ex.Message}");
                return Ok(new { message = $"刪除文章失敗: {ex.Message}" });
            }
        }

        // GET: api/TPosts/userInfo
        [HttpGet("userInfo/{id}")]
        public async Task<TPostsUserInfoDTO> GetUserInfo(int id)
        {
            try
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
            catch (Exception ex) {
                Console.WriteLine($"取得使用者資訊失敗: {ex.Message}");
                return new TPostsUserInfoDTO();
            }
        }
        [HttpGet("searchUserInfo/{keyword}")]
        public async Task<IEnumerable<TPostsUserInfoDTO>> SearchUserInfo(string keyword)
        {
            try
            {
                return _context.TUsers
                    .Where(u => u.FUserNickName.Contains(keyword))
                    .Select(e => new TPostsUserInfoDTO
                    {
                        FUserId = e.FUserId,
                        FUserName = e.FUserName,
                        FUserNickName = e.FUserNickName,
                        FUserImage = e.FUserImage != null ? Convert.ToBase64String(e.FUserImage) : null
                    });
            }
            catch (Exception ex) {
                Console.WriteLine($"搜尋會員資訊失敗: {ex.Message}");
                return new List<TPostsUserInfoDTO>();
            }
        }

        [HttpGet("loginUserId")]
        [Authorize]
        public int GetLoginUserId()
        {
            try
            {
                int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
                return userId;
            }
            catch (Exception ex) {
                Console.WriteLine($"取得登入者ID失敗: {ex.Message}");
                return 0;
            }
        }
    }
}
