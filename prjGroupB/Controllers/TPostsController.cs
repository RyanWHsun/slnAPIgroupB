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
    public class TPostsController : ControllerBase
    {
        private readonly dbGroupBContext _context;

        public TPostsController(dbGroupBContext context)
        {
            _context = context;
        }
        // GET: api/TPosts
        [HttpGet]
        [Authorize]
        public async Task<IEnumerable<TPostsDTO>> GetEmployees()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            return _context.TPosts.Where(t=>t.FUserId == userId).Select(e => new TPostsDTO
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

        //以下為原始範例程式
        // GET: api/TPosts
        //[HttpGet]
        //public async Task<ActionResult<IEnumerable<TPost>>> GetTPosts()
        //{
        //    return await _context.TPosts.ToListAsync();
        //}

        // GET: api/TPosts/5
        //[HttpGet("{id}")]
        //public async Task<ActionResult<TPost>> GetTPost(int id)
        //{
        //    var tPost = await _context.TPosts.FindAsync(id);

        //    if (tPost == null)
        //    {
        //        return NotFound();
        //    }

        //    return tPost;
        //}

        // PUT: api/TPosts/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutTPost(int id, TPost tPost)
        {
            if (id != tPost.FPostId)
            {
                return BadRequest();
            }

            _context.Entry(tPost).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!TPostExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/TPosts
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<TPost>> PostTPost(TPost tPost)
        {
            _context.TPosts.Add(tPost);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetTPost", new { id = tPost.FPostId }, tPost);
        }

        // DELETE: api/TPosts/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTPost(int id)
        {
            var tPost = await _context.TPosts.FindAsync(id);
            if (tPost == null)
            {
                return NotFound();
            }

            _context.TPosts.Remove(tPost);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool TPostExists(int id)
        {
            return _context.TPosts.Any(e => e.FPostId == id);
        }
    }
}
