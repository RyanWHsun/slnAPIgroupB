using System;
using System.Collections;
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
            var attractionTicketDTOs = await _context.TAttractionTickets
                .Select(ticket => new TAttractionTicketDTO {
                    FAttractionTicketId = ticket.FAttractionTicketId,
                    FAttractionId = ticket.FAttractionId,
                    FAttractionName = ticket.FAttraction.FAttractionName,
                    FTicketType = ticket.FTicketType,
                    FPrice = ticket.FPrice,
                    FDiscountInformation = ticket.FDiscountInformation,
                    FCreatedDate = ticket.FCreatedDate
                }
                ).ToListAsync();
            return attractionTicketDTOs;
        }

        // GET: api/TAttractionTickets/Search?isDistinct=true&pageSize=9&pageIndex=0
        [HttpGet]
        [Route("Search")]
        public async Task<IEnumerable<TAttractionTicketDTO>> GetTAttractionTickets(bool isDistinct, int pageSize = 9, int pageIndex = 0) {
            var tickets = new List<TAttractionTicket>();
            // 先載入所有資料到記憶體
            var allTickets = await _context.TAttractionTickets
                .Include(ticket => ticket.FAttraction) // 確保包含關聯屬性
                .ToListAsync();

            if (isDistinct) {
                // 在記憶體中分組並選取每組的第一筆
                tickets = allTickets
                    .GroupBy(ticket => ticket.FAttractionId) // 按 FAttractionId 分組
                    .Select(group => group.First()) // 每組取第一筆
                    .ToList();
            }
            else {
                tickets = allTickets;
            }
            // .Skip(pageSize * pageIndex):
            // 跳過 pageSize *pageIndex 筆資料。
            // 假設 pageIndex = 0，則跳過 10 * 0 = 0 筆，表示從第一筆開始。
            // 假設 pageIndex = 1，則跳過 10 * 1 = 10 筆，表示從第 11 筆開始。

            // .Take(pageSize):
            // 取出最多 pageSize 筆資料。
            // 在這裡，表示從跳過的筆數後開始，取出最多 10 筆資料。
            tickets = tickets
                .Skip(pageSize * pageIndex)
                .Take(pageSize).ToList();

            // .Any() 是 LINQ 的一個方法，檢查集合中是否存在至少一個元素。
            // 如果集合中有資料，.Any() 會回傳 true。
            // 如果集合為空，.Any() 會回傳 false。
            if (tickets == null || !tickets.Any()) {
                return new List<TAttractionTicketDTO>();
            }

            var attractionTicketDTOs = tickets.Select(
                ticket => new TAttractionTicketDTO {
                    FAttractionTicketId = ticket.FAttractionTicketId,
                    FAttractionId = ticket.FAttractionId,
                    FAttractionName = ticket.FAttraction.FAttractionName,
                    FTicketType = ticket.FTicketType,
                    FPrice = ticket.FPrice,
                    FDiscountInformation = ticket.FDiscountInformation,
                    FCreatedDate = ticket.FCreatedDate
                }
            ).ToList();
            return attractionTicketDTOs;
        }

        // GET: api/TAttractionTickets/5
        // id is the attraction id
        [HttpGet("{id}")]
        public async Task<IEnumerable<TAttractionTicketDTO>> GetTAttractionTicket(int id) {
            var attractionTickets = await _context.TAttractionTickets
                .Include(ticket => ticket.FAttraction)
                .Where(ticket => ticket.FAttractionId == id)
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

        // 取得門票種類
        // GET: api/TAttractionTickets/{ticketId}/types
        // id is the attraction id
        [HttpGet("{attractionId}/types")]
        public async Task<List<string>> GetTAttractionTicketTypeById(int attractionId) {
            List<TAttractionTicket> attractionTickets = await _context.TAttractionTickets.Where(ticket=>ticket.FAttractionId== attractionId).ToListAsync();
            if (attractionTickets == null || !attractionTickets.Any()) {
                return [];
            }

            List<string> ticketTypes = new List<string>();
            foreach(var ticket in attractionTickets) {
                ticketTypes.Add(ticket.FTicketType);
            }
            return ticketTypes;
        }

        [HttpGet("Count")]
        public async Task<int> GetTicketQuantities() {
            return await _context.TAttractionTickets
                .Select(t=>t.FAttractionId) // 只取 fAttractionId
                .Distinct()
                .CountAsync();
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
