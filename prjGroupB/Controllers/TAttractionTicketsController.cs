using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using prjGroupB.DTO;
using prjGroupB.Models;

namespace prjGroupB.Controllers {
    [Route("api/[controller]")]
    [ApiController]
    public class TAttractionTicketsController : ControllerBase {
        private readonly dbGroupBContext _context;

        public TAttractionTicketsController(dbGroupBContext context) {
            _context = context;
        }

        // GET: api/TAttractionTickets
        [HttpGet]
        public async Task<IEnumerable<TAttractionTicketDTO>> GetTAttractionTickets() {
            var attractionTicketDTOs = await _context.TAttractionTickets.Select(
                attractionTicket => new TAttractionTicketDTO {
                    FAttractionTicketId = attractionTicket.FAttractionTicketId,
                    FAttractionId = attractionTicket.FAttractionId,
                    FAttractionName = attractionTicket.FAttraction.FAttractionName,
                    FTicketType = attractionTicket.FTicketType,
                    FPrice = attractionTicket.FPrice,
                    FDiscountInformation = attractionTicket.FDiscountInformation,
                    FCreatedDate = attractionTicket.FCreatedDate
                }
            ).ToListAsync();
            return attractionTicketDTOs;
        }

        // GET: api/TAttractionTickets/5
        // id is the attraction id
        [HttpGet("{id}")]
        public async Task<IEnumerable<TAttractionTicketDTO>> GetTAttractionTicket(int id) {
            var attractionTickets = await _context.TAttractionTickets
                .Include(ticket=>ticket.FAttraction)
                .Where(ticket=>ticket.FAttractionId == id)
                .ToListAsync();

            // .Any() 是 LINQ 的一個方法，檢查集合中是否存在至少一個元素。
            // 如果集合中有資料，.Any() 會回傳 true。
            // 如果集合為空，.Any() 會回傳 false。
            if (attractionTickets == null || !attractionTickets.Any()) {
                return new List<TAttractionTicketDTO>();
            }

            var attractionTicketDTOs = attractionTickets.Select(
                attractionTicket => new TAttractionTicketDTO {
                    FAttractionTicketId = attractionTicket.FAttractionTicketId,
                    FAttractionId = attractionTicket.FAttractionId,
                    FAttractionName = attractionTicket.FAttraction.FAttractionName,
                    FTicketType = attractionTicket.FTicketType,
                    FPrice = attractionTicket.FPrice,
                    FDiscountInformation = attractionTicket.FDiscountInformation,
                    FCreatedDate = attractionTicket.FCreatedDate
                }
            ).ToList();

            return attractionTicketDTOs;
        }

        // PUT: api/TAttractionTickets/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutTAttractionTicket(int id, TAttractionTicketDTO attractionTicketDTO) {
            if (id != attractionTicketDTO.FAttractionTicketId) {
                return BadRequest("ticket Id 不符合");
            }

            TAttractionTicket attractionTicket = await _context.TAttractionTickets.FindAsync(id);
            if (attractionTicket == null) {
                return NotFound("找不到 ticket");
            }

            attractionTicket.FAttractionTicketId = attractionTicketDTO.FAttractionTicketId;
            attractionTicket.FAttractionId = attractionTicketDTO.FAttractionId;
            attractionTicket.FTicketType = attractionTicketDTO.FTicketType;
            attractionTicket.FPrice = attractionTicketDTO.FPrice;
            attractionTicket.FDiscountInformation = attractionTicketDTO.FDiscountInformation;
            attractionTicket.FCreatedDate = attractionTicketDTO.FCreatedDate;

            _context.Entry(attractionTicket).State = EntityState.Modified;

            try {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException) {
                if (!TAttractionTicketExists(id)) {
                    return NotFound();
                }
                else {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/TAttractionTickets
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<TAttractionTicketDTO> PostTAttractionTicket(TAttractionTicketDTO attractionTicketDTO) {
            TAttractionTicket attractionTicket = new TAttractionTicket {
                // Id 是資料庫自動產生的，這裡先預設為 0
                FAttractionTicketId = 0,
                FAttractionId = attractionTicketDTO.FAttractionId,
                FTicketType = attractionTicketDTO.FTicketType,
                FPrice = attractionTicketDTO.FPrice,
                FDiscountInformation = attractionTicketDTO.FDiscountInformation,
                FCreatedDate = DateTime.Now
            };

            _context.TAttractionTickets.Add(attractionTicket);

            // 1. 新的記錄插入資料庫。
            // 2. 資料庫生成並返回新的 FAttractionTicketId。
            // 3. EF 將新生成的 ID 更新到 attractionTicket.FAttractionTicketId。
            await _context.SaveChangesAsync();

            attractionTicketDTO.FAttractionTicketId = attractionTicket.FAttractionTicketId;// 更新 attractionTicketDTO 的 FAttractionTicketId
            return attractionTicketDTO;
        }

        // DELETE: api/TAttractionTickets/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTAttractionTicket(int id) {
            var tAttractionTicket = await _context.TAttractionTickets.FindAsync(id);
            if (tAttractionTicket == null) {
                return NotFound();
            }

            _context.TAttractionTickets.Remove(tAttractionTicket);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool TAttractionTicketExists(int id) {
            return _context.TAttractionTickets.Any(e => e.FAttractionTicketId == id);
        }
    }
}
