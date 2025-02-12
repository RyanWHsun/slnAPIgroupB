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
    public class TAttractionCommentsController : ControllerBase {
        private readonly dbGroupBContext _context;

        public TAttractionCommentsController(dbGroupBContext context) {
            _context = context;
        }

        // GET: api/TAttractionComments
        [HttpGet]
        public async Task<IEnumerable<TAttractionCommentDTO>> GetTAttractionComments() {
            var attractionCommentDTOs = await _context.TAttractionComments.Select(
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
                }).ToListAsync();

            return attractionCommentDTOs;
        }

        // GET: api/TAttractionComments/5
        [HttpGet("{id}")]
        public async Task<TAttractionCommentDTO> GetTAttractionComment(int id) {
            var attractionComment = await _context.TAttractionComments
                .Include(c => c.FAttraction)
                .Include(c => c.FUser)
                .FirstOrDefaultAsync(c => c.FCommentId == id);

            if (attractionComment == null) {
                return null;
            }

            var attractionCommentDTO = new TAttractionCommentDTO {
                FCommentId = attractionComment.FCommentId,
                FAttractionId = attractionComment.FAttractionId,
                FAttractionName = attractionComment.FAttraction.FAttractionName,
                FUserId = attractionComment.FUserId,
                FUserName = attractionComment.FUser.FUserName,
                FUserNickName = attractionComment.FUser.FUserNickName,
                FRating = attractionComment.FRating,
                FComment = attractionComment.FComment,
                FCreatedDate = attractionComment.FCreatedDate
            };
            return attractionCommentDTO;
        }

        // GET: api/TAttractions/Search?keyword=A&pageSize=10&pageIndex=0
        [HttpGet]
        [Route("Search")]
        public async Task<IEnumerable<TAttractionCommentDTO>> GetAttractionCommentByCondition([FromQuery] string keyword = "", [FromQuery] int pageSize = 10, [FromQuery] int pageIndex = 0) {
            // .Skip(pageSize * pageIndex):
            // 跳過 pageSize *pageIndex 筆資料。
            // 假設 pageIndex = 0，則跳過 10 * 0 = 0 筆，表示從第一筆開始。
            // 假設 pageIndex = 1，則跳過 10 * 1 = 10 筆，表示從第 11 筆開始。

            // .Take(pageSize):
            // 取出最多 pageSize 筆資料。
            // 在這裡，表示從跳過的筆數後開始，取出最多 10 筆資料。
            var comments = await _context.TAttractionComments
                .Include(data => data.FAttraction)
                .Include(data => data.FUser)
                .Where(data => data.FAttraction.FAttractionName.ToLower().Contains(keyword.ToLower())
                    || data.FUser.FUserName.ToLower().Contains(keyword.ToLower())
                    || data.FUser.FUserNickName.ToLower().Contains(keyword.ToLower())
                    || data.FComment.ToLower().Contains(keyword.ToLower()))
                .Skip(pageSize * pageIndex)
                .Take(pageSize).ToListAsync();

            // .Any() 是 LINQ 的一個方法，檢查集合中是否存在至少一個元素。
            // 如果集合中有資料，.Any() 會回傳 true。
            // 如果集合為空，.Any() 會回傳 false。
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

        // PUT: api/TAttractionComments/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutTAttractionComment(int id, TAttractionCommentDTO attractionCommentDTO) {
            if (id != attractionCommentDTO.FCommentId) {
                return BadRequest("comment Id 不符合");
            }

            TAttractionComment comment = await _context.TAttractionComments.FindAsync(id);
            if(comment == null) {
                return NotFound("找不到該筆評論");
            }

            comment.FCommentId = attractionCommentDTO.FCommentId;
            comment.FAttractionId = attractionCommentDTO.FAttractionId;
            comment.FUserId = attractionCommentDTO.FUserId;
            comment.FRating = attractionCommentDTO.FRating;
            comment.FComment = attractionCommentDTO.FComment;
            comment.FCreatedDate = attractionCommentDTO.FCreatedDate;

            _context.Entry(comment).State = EntityState.Modified;

            try {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException) {
                if (!TAttractionCommentExists(id)) {
                    return NotFound();
                }
                else {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/TAttractionComments
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<TAttractionCommentDTO> PostTAttractionComment(TAttractionCommentDTO attractionCommentDTO) {
            TAttractionComment comment = new TAttractionComment {
                FCommentId = 0,
                FAttractionId = attractionCommentDTO.FAttractionId,
                FUserId = attractionCommentDTO.FUserId,
                FRating = attractionCommentDTO.FRating,
                FComment = attractionCommentDTO.FComment,
                FCreatedDate = DateTime.Now
            };

            _context.TAttractionComments.Add(comment);
            await _context.SaveChangesAsync();

            attractionCommentDTO.FCommentId = comment.FCommentId;// 更新 attractionCommentDTO 的 FCommentId
            return attractionCommentDTO;
        }

        // DELETE: api/TAttractionComments/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTAttractionComment(int id) {
            var tAttractionComment = await _context.TAttractionComments.FindAsync(id);
            if (tAttractionComment == null) {
                return NotFound();
            }

            _context.TAttractionComments.Remove(tAttractionComment);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool TAttractionCommentExists(int id) {
            return _context.TAttractionComments.Any(e => e.FCommentId == id);
        }
    }
}
