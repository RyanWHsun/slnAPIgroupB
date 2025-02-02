using System;
using System.Collections.Generic;
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
        [HttpGet("GetAllPosts")]
        public async Task<IEnumerable<TPostsDTO>> GetAllPosts()
        {
            return _context.TPosts.Where(t => t.FIsPublic == true).Select(e => new TPostsDTO
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

        // GET: api/TPosts/GetMyPosts
        [HttpGet]
        [Authorize]
        public async Task<IEnumerable<TPostsDTO>> GetMyPosts()
        {
            int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            return _context.TPosts.Where(t => t.FUserId == userId).Select(e => new TPostsDTO
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

        // PUT: api/TPosts/id
        [HttpPut("{id}")]
        [Authorize]
        public async Task<string> PutTPost(int id, TPostsDTO PostsDTO)
        {
            int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            TPost post = await _context.TPosts.FindAsync(id);
            if (post == null)
            {
                return "查無文章";
            }
            if (post.FUserId != userId)
            {
                return "你沒有權限修改此文章";
            }
            post.FTitle = PostsDTO.FTitle;
            post.FContent = PostsDTO.FContent;
            post.FUpdatedAt = DateTime.Now;
            post.FIsPublic = PostsDTO.FIsPublic;
            //post.FCategoryId = PostsDTO.FCategoryId;
            try
            {
                _context.TPosts.Update(post);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                return "修改資料庫失敗";
            }
            return "修改文章成功";
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
                //FCategoryId = PostsDTO.FCategoryId
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
    }
}
