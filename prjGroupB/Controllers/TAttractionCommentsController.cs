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

namespace prjGroupB.Controllers {
    [Route("api/[controller]")]
    [ApiController]
    public class TAttractionCommentsController : ControllerBase {
        private readonly dbGroupBContext _context;

        public TAttractionCommentsController(dbGroupBContext context) {
            _context = context;
        }

        // GET: api/TAttractionComments
        //[HttpGet]
        //public async Task<IEnumerable<TAttractionCommentDTO>> GetTAttractionComments() {
        //    var attractionCommentDTOs = await _context.TAttractionComments.Select(
        //        comment => new TAttractionCommentDTO {
        //            FCommentId = comment.FCommentId,
        //            FAttractionId = comment.FAttractionId,
        //            FAttractionName = comment.FAttraction.FAttractionName,
        //            FUserId = comment.FUserId,
        //            FUserName = comment.FUser.FUserName,
        //            FUserNickName = comment.FUser.FUserNickName,
        //            FRating = comment.FRating,
        //            FComment = comment.FComment,
        //            FCreatedDate = comment.FCreatedDate
        //        }).ToListAsync();

        //    return attractionCommentDTOs;
        //}

        // GET: api/TAttractionComments/5
        // id is attraction id
        [HttpGet("{id}")]
        public async Task<IEnumerable<TAttractionCommentDTO>> GetTAttractionComment(int id) {
            var attractionComments = await _context.TAttractionComments
                .Include(c => c.FAttraction)
                .Include(c => c.FUser)
                .Where(c => c.FAttractionId == id)
                .ToListAsync();

            if (attractionComments == null) {
                return null;
            }

            var attractionCommentDTOs = attractionComments.Select(
                    comment => new TAttractionCommentDTO {
                        FCommentId = comment.FCommentId,
                        FAttractionId = comment.FAttractionId,
                        FAttractionName = comment.FAttraction.FAttractionName,
                        FUserId = comment.FUserId,
                        FUserName = comment.FUser.FUserName,
                        FUserNickName = comment.FUser.FUserNickName,
                        FRating = comment.FRating,
                        FComment = comment.FComment,
                        FCreatedDate = comment.FCreatedDate
                    }
                );
            return attractionCommentDTOs;
        }

        // GET: api/TAttractionComments/comments?id=1&count=5&isDescending=true&isCollapsed=true
        // 取 N 筆 or 全部資料
        [HttpGet("comments")]
        public async Task<IEnumerable<TAttractionCommentDTO>> GetTAttractionComment([FromQuery] int id, [FromQuery] int count = 5, [FromQuery] bool isDescending = true, [FromQuery] bool isCollapsed = true) {

            var attractionComments = await _context.TAttractionComments
                .Include(c => c.FAttraction)
                .Include(c => c.FUser)
                .Where(c => c.FAttractionId == id)
                .ToListAsync();

            if (attractionComments == null) {
                return null;
            }

            if (isDescending) {
                attractionComments = attractionComments.OrderByDescending(c => c.FCreatedDate).ToList(); // 依日期遞減排序，從新到舊
            }
            else {
                attractionComments = attractionComments.OrderBy(c => c.FCreatedDate).ToList(); // 依日期遞增排序，從舊到新
            }

            if (isCollapsed) {
                attractionComments = attractionComments.Take(count).ToList(); // 取前 5 筆
            }

            var attractionCommentDTOs = attractionComments.Select(
                    comment => new TAttractionCommentDTO {
                        FCommentId = comment.FCommentId,
                        FAttractionId = comment.FAttractionId,
                        FAttractionName = comment.FAttraction.FAttractionName,
                        FUserId = comment.FUserId,
                        FUserName = comment.FUser.FUserName,
                        FUserNickName = comment.FUser.FUserNickName,
                        FRating = comment.FRating,
                        FComment = comment.FComment,
                        FCreatedDate = comment.FCreatedDate
                    }
                );
            return attractionCommentDTOs;
        }

        // GET: api/TAttractionComments/Search?keyword=A&pageSize=10&pageIndex=0
        [HttpGet]
        [Route("Search")]
        public async Task<IEnumerable<TAttractionCommentDTO>> GetAttractionCommentByCondition([FromQuery] string keyword = "", [FromQuery] int pageSize = 10, [FromQuery] int pageIndex = 0) {
            var comments = await _context.TAttractionComments
                .Include(data => data.FAttraction)
                .Include(data => data.FUser)
                .Where(data => data.FAttraction.FAttractionName.ToLower().Contains(keyword.ToLower())
                    || data.FUser.FUserName.ToLower().Contains(keyword.ToLower())
                    || data.FUser.FUserNickName.ToLower().Contains(keyword.ToLower())
                    || data.FComment.ToLower().Contains(keyword.ToLower()))
                .Skip(pageSize * pageIndex)
                .Take(pageSize).ToListAsync();

            if (comments == null || !comments.Any()) {
                return new List<TAttractionCommentDTO>();
            }

            var attractionCommentDTOs = comments.Select(
                comment => new TAttractionCommentDTO {
                    FCommentId = comment.FCommentId,
                    FAttractionId = comment.FAttractionId,
                    FAttractionName = comment.FAttraction.FAttractionName,
                    FUserId = comment.FUserId,
                    FUserName = comment.FUser.FUserName,
                    FUserNickName = comment.FUser.FUserNickName,
                    FRating = comment.FRating,
                    FComment = comment.FComment,
                    FCreatedDate = comment.FCreatedDate
                }
            ).ToList();

            return attractionCommentDTOs;
        }

        // get an user info
        [HttpGet("commenter")]
        [Authorize]
        public async Task<TUserDTO> GetCommenterInfo() {
            // FindFirstValue(): 從 User.Claims 查找 第一個符合 ClaimTypes.NameIdentifier 的 Claim，並回傳它的值。
            // ClaimTypes.NameIdentifier 是一個 標準的 Claim 類型，表示「使用者的唯一識別碼」（通常是 UserId）。
            var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
            int userId = int.TryParse(userIdValue, out var parsedId) ? parsedId : 0;
            if (userId == 0) return null;

            var user = await _context.TUsers.FindAsync(userId);
            if (user == null) return null;

            var userDto = new TUserDTO {
                FUserId = user.FUserId,
                FUserRankId = user.FUserRankId.HasValue ? user.FUserRankId.Value : 0,
                FUserName = user.FUserName,
                FUserImage = user.FUserImage != null ? Convert.ToBase64String(user.FUserImage) : null,
                FUserNickName = user.FUserNickName,
                FUserSex = user.FUserSex,
                FUserBirthday = user.FUserBirthday,
                FUserPhone = user.FUserPhone,
                FUserEmail = user.FUserEmail,
                FUserAddress = user.FUserAddress,
                FUserComeDate = user.FUserComeDate ?? DateTime.Now,
                FUserPassword = user.FUserPassword
            };

            return userDto;
        }

        // PUT: api/TAttractionComments/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        //[HttpPut("{id}")]
        //public async Task<IActionResult> PutTAttractionComment(int id, TAttractionCommentDTO attractionCommentDTO) {
        //    if (id != attractionCommentDTO.FCommentId) {
        //        return BadRequest("comment Id 不符合");
        //    }

        //    TAttractionComment comment = await _context.TAttractionComments.FindAsync(id);
        //    if(comment == null) {
        //        return NotFound("找不到該筆評論");
        //    }

        //    comment.FCommentId = attractionCommentDTO.FCommentId;
        //    comment.FAttractionId = attractionCommentDTO.FAttractionId;
        //    comment.FUserId = attractionCommentDTO.FUserId;
        //    comment.FRating = attractionCommentDTO.FRating;
        //    comment.FComment = attractionCommentDTO.FComment;
        //    comment.FCreatedDate = attractionCommentDTO.FCreatedDate;

        //    _context.Entry(comment).State = EntityState.Modified;

        //    try {
        //        await _context.SaveChangesAsync();
        //    }
        //    catch (DbUpdateConcurrencyException) {
        //        if (!TAttractionCommentExists(id)) {
        //            return NotFound();
        //        }
        //        else {
        //            throw;
        //        }
        //    }

        //    return NoContent();
        //}

        // POST: api/TAttractionComments
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> PostTAttractionComment([FromBody] TAttractionCommentDTO attractionCommentDTO) {
            // FindFirstValue(): 從 User.Claims 查找 第一個符合 ClaimTypes.NameIdentifier 的 Claim，並回傳它的值。
            // ClaimTypes.NameIdentifier 是一個 標準的 Claim 類型，表示「使用者的唯一識別碼」（通常是 UserId）。
            var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
            int userId = int.TryParse(userIdValue, out var parsedId) ? parsedId : 0;

            if (attractionCommentDTO == null || userId == 0) {
                return BadRequest("無效的請求");
            }

            TAttractionComment comment = new TAttractionComment {
                FCommentId = 0,
                FAttractionId = attractionCommentDTO.FAttractionId,
                FUserId = userId,
                FRating = attractionCommentDTO.FRating,
                FComment = attractionCommentDTO.FComment,
                FCreatedDate = DateTime.Now
            };

            _context.TAttractionComments.Add(comment);
            await _context.SaveChangesAsync();

            attractionCommentDTO.FCommentId = comment.FCommentId;
            return Ok(attractionCommentDTO);
        }


        // DELETE: api/TAttractionComments/5
        //[HttpDelete("{id}")]
        //public async Task<IActionResult> DeleteTAttractionComment(int id) {
        //    var tAttractionComment = await _context.TAttractionComments.FindAsync(id);
        //    if (tAttractionComment == null) {
        //        return NotFound();
        //    }

        //    _context.TAttractionComments.Remove(tAttractionComment);
        //    await _context.SaveChangesAsync();

        //    return NoContent();
        //}

        private bool TAttractionCommentExists(int id) {
            return _context.TAttractionComments.Any(e => e.FCommentId == id);
        }
    }
}
