using System;
using System.Collections.Generic;
using System.Data.Common;
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
    public class TAttractionCommentsController : ControllerBase
    {
        private readonly dbGroupBContext _context;

        public TAttractionCommentsController(dbGroupBContext context)
        {
            _context = context;
        }

        // GET: api/TAttractionComments/5
        // id is attraction id
        [HttpGet("{id}")]
        public async Task<ActionResult<IEnumerable<TAttractionCommentDTO>>> GetTAttractionComment(int id)
        {
            try
            {
                var attractionComments = await _context.TAttractionComments
                    .Include(c => c.FAttraction)
                    .Include(c => c.FUser)
                    .Where(c => c.FAttractionId == id)
                    .ToListAsync();

                if (attractionComments == null)
                {
                    return NotFound(new { error = $"找不到ID為 {id} 的景點評論" });
                }

                var attractionCommentDTOs = attractionComments.Select(
                        comment => new TAttractionCommentDTO
                        {
                            FCommentId = comment.FCommentId,
                            FAttractionId = comment.FAttractionId,
                            FAttractionName = comment.FAttraction.FAttractionName,
                            FUserId = comment.FUserId,
                            FUserName = comment.FUser.FUserName,
                            FUserNickName = comment.FUser.FUserNickName,
                            FUserImage = comment.FUser.FUserImage != null ? Convert.ToBase64String(comment.FUser.FUserImage) : null,
                            FRating = comment.FRating,
                            FComment = comment.FComment,
                            FCreatedDate = comment.FCreatedDate
                        }
                    );
                return Ok(attractionCommentDTOs);
            }
            catch (DbException ex)
            {
                // 資料庫連線錯誤
                return StatusCode(500, new { error = "資料庫連線錯誤", details = ex.Message });
            }
            catch (Exception ex)
            {
                // 一般錯誤
                return StatusCode(500, new { error = "內部錯誤", details = ex.Message });
            }
        }

        // GET: api/TAttractionComments/comments?id=1&count=5&isDescending=true&isCollapsed=true
        // 取 N 筆 or 全部資料
        [HttpGet("comments")]
        public async Task<ActionResult<IEnumerable<TAttractionCommentDTO>>> GetTAttractionComment([FromQuery] int id, [FromQuery] int count = 5, [FromQuery] bool isDescending = true, [FromQuery] bool isCollapsed = true)
        {
            try
            {
                var attractionComments = await _context.TAttractionComments
                    .Include(c => c.FAttraction)
                    .Include(c => c.FUser)
                    .Where(c => c.FAttractionId == id)
                    .ToListAsync();

                if (attractionComments == null)
                {
                    return NotFound(new { error = $"找不到ID為 {id} 的景點評論" });
                }

                if (isDescending)
                {
                    attractionComments = attractionComments.OrderByDescending(c => c.FCreatedDate).ToList(); // 依日期遞減排序，從新到舊
                }
                else
                {
                    attractionComments = attractionComments.OrderBy(c => c.FCreatedDate).ToList(); // 依日期遞增排序，從舊到新
                }

                if (isCollapsed)
                {
                    attractionComments = attractionComments.Take(count).ToList(); // 取前 5 筆
                }

                var attractionCommentDTOs = attractionComments.Select(
                        comment => new TAttractionCommentDTO
                        {
                            FCommentId = comment.FCommentId,
                            FAttractionId = comment.FAttractionId,
                            FAttractionName = comment.FAttraction.FAttractionName,
                            FUserId = comment.FUserId,
                            FUserName = comment.FUser.FUserName,
                            FUserNickName = comment.FUser.FUserNickName,
                            FUserImage = comment.FUser.FUserImage != null ? Convert.ToBase64String(comment.FUser.FUserImage) : null,
                            FRating = comment.FRating,
                            FComment = comment.FComment,
                            FCreatedDate = comment.FCreatedDate
                        }
                    );
                return Ok(attractionCommentDTOs);
            }
            catch (DbException ex)
            {
                // 資料庫連線錯誤
                return StatusCode(500, new { error = "資料庫連線錯誤", details = ex.Message });
            }
            catch (Exception ex)
            {
                // 一般錯誤
                return StatusCode(500, new { error = "內部錯誤", details = ex.Message });
            }
        }

        // GET: api/TAttractionComments/Search?keyword=A&pageSize=10&pageIndex=0
        [HttpGet]
        [Route("Search")]
        public async Task<ActionResult<IEnumerable<TAttractionCommentDTO>>> GetAttractionCommentByCondition([FromQuery] string keyword = "", [FromQuery] int pageSize = 10, [FromQuery] int pageIndex = 0)
        {
            try
            {
                var comments = await _context.TAttractionComments
                    .Include(data => data.FAttraction)
                    .Include(data => data.FUser)
                    .Where(data => data.FAttraction.FAttractionName.ToLower().Contains(keyword.ToLower())
                        || data.FUser.FUserName.ToLower().Contains(keyword.ToLower())
                        || data.FUser.FUserNickName.ToLower().Contains(keyword.ToLower())
                        || data.FComment.ToLower().Contains(keyword.ToLower()))
                    .Skip(pageSize * pageIndex)
                    .Take(pageSize).ToListAsync();

                if (comments == null || !comments.Any())
                {
                    return NotFound(new { error = "找不到符合條件的評論" });
                }

                var attractionCommentDTOs = comments.Select(
                    comment => new TAttractionCommentDTO
                    {
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

                return Ok(attractionCommentDTOs);
            }
            catch (DbException ex)
            {
                // 資料庫連線錯誤
                return StatusCode(500, new { error = "資料庫連線錯誤", details = ex.Message });
            }
            catch (Exception ex)
            {
                // 一般錯誤
                return StatusCode(500, new { error = "內部錯誤", details = ex.Message });
            }
        }

        // https://localhost:7112/api/TAttractionComments/commenter
        // 取得評論者(也就是登入用戶)的資訊
        [HttpGet("commenter")]
        [Authorize]
        public async Task<ActionResult<TCommenterDTO>> GetCommenterInfo()
        {
            try
            {
                // FindFirstValue(): 從 User.Claims 查找 第一個符合 ClaimTypes.NameIdentifier 的 Claim，並回傳它的值。
                // ClaimTypes.NameIdentifier 是一個 標準的 Claim 類型，表示「使用者的唯一識別碼」（通常是 UserId）。
                var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
                int userId = int.TryParse(userIdValue, out var parsedId) ? parsedId : 0;
                if (userId == 0)
                {
                    return BadRequest(new { error = "user Id 不存在" });
                }

                var user = await _context.TUsers.FindAsync(userId);
                if (user == null)
                {
                    return NotFound(new { error = "user 不存在" });
                }

                var userDto = new TCommenterDTO
                {
                    FUserId = user.FUserId,
                    FUserName = user.FUserName,
                    FUserNickName = user.FUserNickName,
                    FUserImage = user.FUserImage != null ? Convert.ToBase64String(user.FUserImage) : null,
                    FUserRankId = user.FUserRankId.HasValue ? user.FUserRankId.Value : 0,
                };

                return Ok(userDto);
            }
            catch (DbException ex)
            {
                // 資料庫連線錯誤
                return StatusCode(500, new { error = "資料庫連線錯誤", details = ex.Message });
            }
            catch (Exception ex)
            {
                // 一般錯誤
                return StatusCode(500, new { error = "內部錯誤", details = ex.Message });
            }
        }

        // POST: api/TAttractionComments
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        [Authorize]
        public async Task<ActionResult<TAttractionCommentDTO>> PostTAttractionComment([FromBody] TAttractionCommentDTO attractionCommentDTO)
        {
            try
            {
                // FindFirstValue(): 從 User.Claims 查找 第一個符合 ClaimTypes.NameIdentifier 的 Claim，並回傳它的值。
                // ClaimTypes.NameIdentifier 是一個 標準的 Claim 類型，表示「使用者的唯一識別碼」（通常是 UserId）。
                var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
                int userId = int.TryParse(userIdValue, out var parsedId) ? parsedId : 0;

                if (attractionCommentDTO == null || userId == 0)
                {
                    return BadRequest(new { error = "無效的請求" });
                }

                TAttractionComment comment = new TAttractionComment
                {
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
            catch (DbUpdateException ex)
            {
                // 資料庫更新錯誤
                return StatusCode(500, new { error = "資料庫更新錯誤", details = ex.Message });
            }
            catch (Exception ex)
            {
                // 一般錯誤
                return StatusCode(500, new { error = "內部錯誤", details = ex.Message });
            }
        }

        private bool TAttractionCommentExists(int id)
        {
            return _context.TAttractionComments.Any(e => e.FCommentId == id);
        }
    }
}
