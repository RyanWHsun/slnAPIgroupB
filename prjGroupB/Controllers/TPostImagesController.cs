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
    public class TPostImagesController : ControllerBase
    {
        private readonly dbGroupBContext _context;

        public TPostImagesController(dbGroupBContext context)
        {
            _context = context;
        }

        // GET: api/TPostImages/getPublicImages/5
        [HttpGet("getPublicImages/{id}")]
        public async Task<IActionResult> GetPublicImages(int id)
        {
            TPost post = await _context.TPosts.FindAsync(id);
            if (post.FIsPublic != true)
            {
                return Unauthorized("您沒有權限查看此文章的圖片");
            }
            var images = _context.TPostImages.Where(i => i.FPostId == id).Select(e => e.FImage);
            List<string> imageList = new List<string>();
            foreach (byte[] image in images)
            {
                imageList.Add(Convert.ToBase64String(image));
            }
            return Ok(imageList);
        }

        // GET: api/TPostImages/5
        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> GetTPostImages(int id)
        {
            int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            TPost post = await _context.TPosts.FindAsync(id);
            if (post.FUserId != userId)
            {
                return Unauthorized("您沒有權限查看此文章的圖片");
            }
            var images = _context.TPostImages.Where(i => i.FPostId == id).Select(e => e.FImage);
            List<string> imageList = new List<string>();
            foreach (byte[] image in images)
            {
                imageList.Add(Convert.ToBase64String(image));
            }
            return Ok(imageList);
        }

        // POST: api/TPostImages/
        [HttpPost]
        [Authorize]
        public async Task<List<TPostImagesDTO>> PostTPostImage(List<TPostImagesDTO> PostImagesDTOs)
        {
            int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            int postId = (int)PostImagesDTOs.First().FPostId;
            TPost post = await _context.TPosts.FindAsync(postId);
            if (post.FUserId != userId)
            {
                return null;
            }
            List<TPostImage> postImages = new List<TPostImage>();
            foreach (TPostImagesDTO DTO in PostImagesDTOs)
            {
                postImages.Add(new TPostImage
                {
                    FPostId = DTO.FPostId,
                    FImage = DTO.FImage
                });
            }
            _context.TPostImages.AddRange(postImages);
            await _context.SaveChangesAsync();
            for (int i = 0; i < postImages.Count; i++)
            {
                PostImagesDTOs[i].FImageId = postImages[i].FImageId;
            }
            return PostImagesDTOs;
        }

        // DELETE: api/TPostImages/5
        [HttpDelete("{id}")]
        [Authorize]
        public async Task<string> DeleteTPost(int id)
        {
            int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            TPostImage postImage = await _context.TPostImages.FindAsync(id);
            int postId = (int)postImage.FPostId;
            TPost post = await _context.TPosts.FindAsync(postId);
            if (post.FUserId != userId)
            {
                return "你沒有權限刪除此圖片";
            }
            try
            {
                _context.TPostImages.Remove(postImage);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                return "刪除資料庫失敗";
            }
            return "刪除圖片成功";
        }
    }
}
